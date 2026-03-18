#!/usr/bin/env python
# -*- coding: utf-8 -*-


import os, time, sys, json, requests
import zipfile
import shutil
import logging as log

try:
    from filelock import FileLock
except:
    log.error('''
    you need to install  filelock
    you could do as:
    windows:     python -m pip install filelock
    mac/linux:  pip(pip3) install  filelock
    ''')
    sys.exit(-1)

if sys.version_info.major < 3:
    reload(sys)
    sys.setdefaultencoding('utf8')

cur_dir = os.path.dirname(os.path.realpath(__file__))

log.basicConfig(level=log.INFO)
#log.basicConfig(level=log.DEBUG)


FILE_BLOCK_LEN = 20 * 1024 * 1024

def zip_unpack_all(file_path, output_path):
    with zipfile.ZipFile(file_path, 'r') as zf:
        for item in zf.infolist():
            zf.extract(item, output_path)
   
def http_get_file(file_path, url, user_auth=None, user_headers=None, user_params=None, user_data=None, user_verify=None):
    rsp = requests.get(url, auth=user_auth, headers=user_headers, params = user_params, data = user_data, stream=True, verify=user_verify, timeout=(10, 30)) 
    with open(file_path, 'wb') as of:
        for chunk in rsp.iter_content(chunk_size=FILE_BLOCK_LEN):
            if chunk:
                of.write(chunk)
        of.flush()
    rsp.raise_for_status()


def http_get(url, user_auth=None, user_headers=None, user_params=None, user_data=None, user_verify=None):
    rsp = requests.get(url, auth=user_auth, headers=user_headers, params = user_params, data = user_data, verify=user_verify, timeout=(10, 30)) 
    rsp.raise_for_status()
    return rsp.content

def http_put_file(file_path, url, user_auth=None, user_headers=None, user_verify=None):
    with open(file_path, 'rb') as f:
        rsp = requests.put(url, auth=user_auth, headers = user_headers, data = f, verify=user_verify, timeout=(10, 30)) 
        rsp.raise_for_status()

class Transfer():
    def __init__(self, auth=None):
        self.auth = auth
    
    def do_http_req(self, url, data=None, verify=None, retry=3):
        while 1:
            try:
                return http_get(url, user_data=data, user_verify=verify)
            except Exception as e:
                if retry == 0:
                    raise e
                retry -= 1

    def put_file(self, url, file_path, verify=None, retry=3):
        st = int(time.time())
        while 1:
            try:
                log.info(url)
                http_put_file(file_path, url, user_auth = self.auth, user_verify=verify)
                break
            except Exception as e:
                if retry == 0:
                    raise e
                retry -= 1
        log.info('put file [%s] done , used:%ds' % (file_path, int(time.time() -st)))

    def download_file(self, file_path, file_size, file_md5, url, verify=None, retry=3):
        st = int(time.time())
        while 1:
            try:
                http_get_file(file_path, url, user_auth = self.auth, user_verify=verify)
                break
            except Exception as e:
                log.info('%s', str(e))
                if retry == 0:
                    raise e
                retry -= 1
        log.info('download file used:%ds' % (int(time.time()) - st, ))
        if file_size == os.stat(file_path).st_size:
            log.info('download file check success [%s]' % (file_path))
        else:
            raise(Exception('download file failed from %s' % url))


class FileUpdater():
    def __init__(self, cur_dir):
        #self.update_file_url = 'http://10.225.88.116:9389/get_update_info?' 
        self.update_file_url = 'https://safe-protection.bytedance.com/get_update_info?' 
        self.cur_dir = cur_dir
        self.time_gap = 3600 * 24 * 1
        self.lock_file = os.path.join(cur_dir, 'wk.lock')

    def update_file(self):
        with FileLock(self.lock_file):
            log.debug('process[%d] do update' % os.getpid())
            self.do_update_file()

    def get_client_version(self, filepath):
        with open(filepath, 'rb') as fd:
            for line in fd:
                if 'so_protection_version = ' in line:
                    return int(line.split('\'')[1])
        raise(Exception('not find client version'))

    def do_update_file(self):
        client_py = os.path.join(self.cur_dir, 'protection.py')
        my_version = self.get_client_version(client_py)

        data = {
                'file_type' : 'client_file',
                'ts' : 10000
            }
        t = Transfer()
        rsp = t.do_http_req(self.update_file_url, json.dumps(data))
        rsp_json = json.loads(rsp)
        if rsp_json['ret'] != 'success':
            log.info('update py failed:%s', rsp) 
            return

        if len(rsp_json['file_url']) == 0:
            log.info('no need to update py') 
            os.utime(client_py)
            return

        new_file = os.path.join(self.cur_dir, 'update.zip')
        tmp_dir = os.path.join(self.cur_dir, 'tmp_update')
        retry = 3
        for url in rsp_json['file_url'].split(';'):
            log.debug('get file %s' % url)
            try:
                t.download_file(new_file, rsp_json['file_size'], None, url)
                break
            except :
                if retry == 0:
                    log.info('download update file failed, you can ignore this')
                    return
                else:
                    retry -= 1

        if os.path.exists(tmp_dir):
            shutil.rmtree(tmp_dir)
        os.makedirs(tmp_dir)
        zip_unpack_all(new_file, tmp_dir)

        update_py = os.path.join(tmp_dir, 'protection.py')
        try:
            update_version = self.get_client_version(update_py)
            log.info('my_version: %d, update_version: %d' % (my_version, update_version))
            if update_version != my_version:
                for item in os.listdir(tmp_dir):
                    file_path = os.path.join(tmp_dir, item)
                    if file_path.endswith('.py') and item != os.path.basename(__file__):
                        log.debug('cp %s to %s' % (file_path, self.cur_dir))
                        shutil.copy2(file_path, self.cur_dir)
        except:
            pass
        
        shutil.rmtree(tmp_dir)
        os.unlink(new_file)
        return 
        
        
def main():
    try:
        FileUpdater(cur_dir).update_file()
    except Exception as e:
        log.debug('%s', str(e))
        pass
    from protection import wk_main
    wk_main()

if __name__ == '__main__':
    main()

