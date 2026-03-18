#!/usr/bin/env python
# -*- coding: utf-8 -*-


import os, time, sys, json, argparse, json
import zipfile
import platform
import shutil, hashlib
import base64, uuid
import logging as log
import check_so

if sys.version_info.major < 3:
    reload(sys)
    sys.setdefaultencoding('utf8')

try:
    import requests
    from Crypto.Cipher import AES
    from Crypto import Random

    from Crypto.PublicKey import RSA
    from Crypto.Cipher import PKCS1_v1_5 as PKCS1_v1_5_cipper
    from Crypto.Signature import PKCS1_v1_5 as PKCS1_v1_5_sign
    from Crypto.Hash import SHA256
    from Crypto.Util.Padding import unpad, pad
except:
    log.error('''
    you need to install requests and pycryptodome
    you could do as:
    windows:     python -m pip install pycryptodome
    mac/linux:  pip(pip3) install pycryptodome
    ''')
    sys.exit(-1)

cur_dir = os.path.dirname(os.path.realpath(__file__))

log.basicConfig(level=log.INFO)
so_protection_version = '1007'
FILE_BLOCK_LEN = 20 * 1024 * 1024


AES_BLOCK_SIZE = 16
AES_KEY_LEN = 16
AES_FILE_BLOCK_SIZE = 10 * 1024 * 1024

RSA_PUBLIC_KEY = '''-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC5BQWx1IcgamiI9/fbk1jWE7zn
4lRYQkwo+RCd2pnYp5oDtnm4PnT0bvmqe/AqWuch7yuLssH5jDfPegQbIIY5wJ77
9nQhiuk133rVj5iCrn9XaAv3mY/aTUU3re6jUR506vBP0FJw3QwsmFvzuKxWWw0/
MXLnvjiMXVjqvuMG+wIDAQAB
-----END PUBLIC KEY-----'''

g_target_so = ('libil2cpp.so', 'libmono.so', 'libmonobdwgc-2.0.so', 'libclient.so', 'libUE4.so', 'libFancy3D.so', 'libcocos2dlua.so', 'libMyGame.so', 'libcocos2djs.so')

def gen_uuid():
    return str(uuid.uuid1())[-12:]

class RsaCryptor(object):
    def __init__(self, pub_key=RSA_PUBLIC_KEY):
        self.public_key = RSA.importKey(pub_key)

    def encrypt(self, input_buff):
        enc_buff = b''
        cipher = PKCS1_v1_5_cipper.new(self.public_key)
        for buff in self.block_data(input_buff, self.public_key):
            enc_buff += cipher.encrypt(buff)
        return enc_buff

    def verify(self, data, signature):
        try:
            sha1 = SHA256.new(data)
            PKCS1_v1_5_sign.new(self.public_key).verify(sha1, signature)
            return True
        except:
            return False

    def get_block_size(self, rsa_key):
        reserve_size = 11
        key_size = 1024
        if (key_size % 8) != 0:
            raise(RuntimeError('rsa key size error'))
        if rsa_key.has_private():
            reserve_size = 0
        bs = int(key_size / 8) - reserve_size
        return bs

    def block_data(self, data, rsa_key):
        bs = self.get_block_size(rsa_key)
        for i in range(0, len(data), bs):
            yield data[i:i + bs]

class AesCryptor(object):
    def __init__(self, key=None):
        self.key = Random.new().read(AES_BLOCK_SIZE) if key == None else key
        self.mode = AES.MODE_CBC
        self.iv = 'XwOW83G3dE7NOmec'.encode()
 
    def encrypt(self, text):
        cryptor = AES.new(self.key, self.mode, self.iv)
        self.ciphertext = cryptor.encrypt(pad(text, AES_BLOCK_SIZE))
        return base64.b64encode(self.ciphertext)

    def encrypt_file(self, input_file, output_file):
        cryptor = AES.new(self.key, self.mode, self.iv)
        remaind = os.stat(input_file).st_size
        with open(input_file, 'rb') as ifd, open(output_file, 'wb') as ofd:
            while remaind > 0: 
                read_size = AES_FILE_BLOCK_SIZE if remaind >= AES_FILE_BLOCK_SIZE else remaind
                buff = ifd.read(read_size)
                if read_size == remaind:
                    buff = pad(buff, AES_BLOCK_SIZE)
                ofd.write(cryptor.encrypt(buff))
                remaind -= read_size
 
    def decrypt(self, text):
        decode = base64.b64decode(text)
        cryptor = AES.new(self.key, self.mode, self.iv)
        plain_text = cryptor.decrypt(decode)
        return unpad(plain_text, AES_BLOCK_SIZE)

    def decrypt_file(self, input_file, output_file):
        cryptor = AES.new(self.key, self.mode, self.iv)
        remaind = os.stat(input_file).st_size
        with open(input_file, 'rb') as ifd, open(output_file, 'wb') as ofd:
            while remaind > 0:
                read_size = AES_FILE_BLOCK_SIZE if remaind >= AES_FILE_BLOCK_SIZE else remaind
                buff = ifd.read(read_size)
                d_buff = cryptor.decrypt(buff)
                if read_size == remaind:
                    d_buff = unpad(d_buff, AES_BLOCK_SIZE)
                ofd.write(d_buff)
                remaind -= read_size

    def get_key(self):
        return self.key

def is_so(file_path):
    with open(file_path, 'rb') as of:
        if of.read(4) == '\x7fELF'.encode('utf-8'):
            return True         
    return False 

def is_apk(file_path):
    if not zipfile.is_zipfile(file_path):
        return False
    return True

def is_jar(file_path):
    if not zipfile.is_zipfile(file_path):
        return False
    if not file_path.endswith('.jar'): 
        return False
    return True

def is_sdk(file_path):
    if not zipfile.is_zipfile(file_path):
        return False
    if file_path.endswith('.aar'): 
        return True 

    has_libs = False
    has_jar = False
    has_others = False
    with zipfile.ZipFile(file_path, 'r') as fzip:
        for item in fzip.infolist():
            if item.filename.startswith('libs'):
                has_libs = True
            elif item.filename.endswith('.jar'):
                has_jar = True
            else:
                has_others = True 
                break
    return not has_others


def get_zip():
    if platform.system() == "Linux":
        return 'zip'
    elif platform.system() == "Darwin":
        return 'zip'
    else:
        return 'zip'

def get_zipalign():
    if sys.platform == 'win32':
        return which('zipalign.exe')[0]
    else:
        return which('zipalign')[0]

def zip_unpack_all(file_path, output_path):
    with zipfile.ZipFile(file_path, 'r') as zf:
        for item in zf.infolist():
            zf.extract(item, output_path)
    
def delete(apk_path, files):
    zip_cmd = get_zip() + ' -d ' + apk_path + ' ' + files + ' > ' + os.devnull
    return (os.system(zip_cmd) != 0)

def add(apk_path, files):
    zip_cmd = get_zip() + ' -r ' + apk_path + ' ' + files
    if os.system(zip_cmd) != 0:
        raise(UpdateZipException('package add file failed'))

def update(apk_path, files):
    if os.path.isfile(files):
        zip_cmd = get_zip() + ' -u ' + apk_path + ' ' + files
    else:
        zip_cmd = get_zip() + ' -r -u ' + apk_path + ' ' + files
    if os.system(zip_cmd) != 0:
        raise(UpdateZipException('package update file failed'))

def zipalign(ori_path, output_path):
    try:
        zipalign_cmd = get_zipalign() + ' -f 4 ' + ori_path + ' ' + output_path
    except:
        raise(UpdateZipException('can NOT find zipalign in env PATH'))

    log.info(zipalign_cmd)
    if os.system(zipalign_cmd) != 0:
        raise(UpdateZipException('zipalign  failed'))
    return 0

def calc_file_md5(file_path):
    file_size = os.stat(file_path).st_size
    with open(file_path, 'rb') as of:
        hash_md5 = hashlib.md5()
        while file_size > 0:
            read_size = FILE_BLOCK_LEN if file_size > FILE_BLOCK_LEN  else file_size
            buff = of.read(FILE_BLOCK_LEN)
            hash_md5.update(buff)
            file_size -= read_size 
        return hash_md5.hexdigest()

def http_get_file(file_path, url, user_auth=None, user_headers=None, user_params=None, user_data=None, user_verify=None):

    total_len = 0
    min_speed_KBps = 100
    DL_BLOCK_LEN = 500 * 1024
    start = time.time()
    rsp = requests.get(url, auth=user_auth, headers=user_headers, params = user_params, data = user_data, stream=True, verify=user_verify, timeout=(10, 60)) 
    print(rsp.headers)
    with open(file_path, 'wb') as of:
        for chunk in rsp.iter_content(chunk_size=DL_BLOCK_LEN):
            if chunk:
                now = time.time()
                dl_len = len(chunk)
                total_len += dl_len
                speed = dl_len/(now - start)/1024
                if total_len <= 1024 * 1024 * 2 and (speed < min_speed_KBps and dl_len > 1024 * 200):
                    rsp.close()
                    log.info('download %f KB/s, network is too slow, we close this transmission' % (speed, ))
                    raise (NetSpeedTooSlow)
                of.write(chunk)
                log.info('download %d Bytes, speed: %f KB/s' % (dl_len, speed))
                start = time.time()
        of.flush()
    rsp.raise_for_status()


def http_get(url, user_auth=None, user_headers=None, user_params=None, user_data=None, user_verify=None):
    rsp = requests.get(url, auth=user_auth, headers=user_headers, params = user_params, data = user_data, verify=user_verify, timeout=(10, 30)) 
    rsp.raise_for_status()
    if sys.version_info[0] < 3:
        return rsp.content
    else:
        return rsp.content.decode('utf-8')

def http_put_file(file_path, url, user_auth=None, user_headers=None, user_verify=None):
    with open(file_path, 'rb') as f:
        rsp = requests.put(url, auth=user_auth, headers = user_headers, data = f, verify=user_verify, timeout=(10, 30)) 
        rsp.raise_for_status()

class NetSpeedTooSlow(Exception):
    pass
class NotSupportEngineException(Exception):
    pass
class InputNotZIpException(Exception):
    pass
class Theft2WithJarException(Exception):
    pass
class UpdateZipException(Exception):
    pass
class ServerException(Exception):
    pass
class NetWorkException(Exception):
    pass
class AleadyPortectException(Exception):
    pass

def wk_exit(err_code, err_msg):
    print('client_err_message : %s' % (err_msg, ))
    sys.exit(err_code)

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

    def download_file(self, file_path, file_size, file_md5, url, verify=None, retry=6):
        st = int(time.time())
        while 1:
            try:
                http_get_file(file_path, url, user_auth = self.auth, user_verify=verify)
                break
            except NetSpeedTooSlow as e:
                time.sleep(3)
                if retry == 0:
                    raise e
                retry -= 1
            except Exception as e:
                if retry == 0:
                    raise e
                retry -= 1
        log.info('download file used:%ds' % (int(time.time()) - st, ))
        if file_size == os.stat(file_path).st_size and calc_file_md5(file_path) == file_md5:
            log.info('download file check success [%s]' % (file_path))
        else:
            raise(Exception('download file failed from %s' % url))

def log_error(msg):
    log.error('************failed************:[%s] ' % msg['ret'])
    if 'failed_reason' in msg:
        log.error('************failed_reason************: %s' % msg['failed_reason'])

def get_error(msg):
    if 'failed_reason' in msg:
        return msg['failed_reason']
    else:
        return 'failed in reinforce server'

CLIENT_MAGIC = '0x8f3621'

class Client():
    def __init__(self, wk_server, user):
        self.wk_server = wk_server
        self.wk_user = user
        self.version = int(so_protection_version)
        self.fs_user = 'security_official_user'
        self.fs_passwd = 'NYdUlzc1'

        self.filepath_url = '%s/container/' % (self.wk_server, )
        self.get_status_url = '%s/get_status?' % (self.wk_server, )
        self.do_so_protection_url = '%s/do_so_protection?' % (self.wk_server, )
        self.get_update_info_url = '%s/get_update_info?' % (self.wk_server, )
        self.get_version_url = '%s/get_all_version_info?' % (self.wk_server, )
        self.upload_statics_url = '%s/upload_statics?' % (self.wk_server, )

    def do_so_protection(self, file_path, para, keys, other_info):
        data = {
                'user_id' : self.wk_user,
                'ori_package_md5' : other_info['md5'],
                'ori_package_size' : other_info['size'],
                'reinforce_channels' : 0,
                'upload_begin_time' : other_info['upload_begin_time'],
                'jiagu_version' : other_info['jiagu_version'],
                'appid' : other_info['appid'],
                'version' : self.version,
                'file_size': os.stat(file_path).st_size,
                'file_md5' : calc_file_md5(file_path),
                'file_url': self.data_url,
                'para' : para,
                'data': keys.decode()
                }
       
        t = Transfer()
        rsp = t.do_http_req(self.do_so_protection_url, json.dumps(data))
        rsp_json = json.loads(rsp)
        if rsp_json['ret'] != 'success':
            log_error(rsp_json)
            raise(ServerException(get_error(rsp_json)))

        self.task_key = rsp_json['key']
        status_data = {
                'user_id' : self.wk_user,
                'key' : rsp_json['key']
                }
        rsp = t.do_http_req(self.get_status_url, json.dumps(status_data))
        rsp_json = json.loads(rsp)
        while rsp_json['ret'] == 'running':
            log.info('waiting for protection...')
            time.sleep(15)
            rsp = t.do_http_req(self.get_status_url, json.dumps(status_data))
            rsp_json = json.loads(rsp)

        if rsp_json['ret'] == 'success':
            return rsp_json
        log_error(rsp_json)
        err_msg = get_error(rsp_json)
        if err_msg == 'input_already_protected_error':
            raise(AleadyPortectException(err_msg))
        else:
            raise(ServerException(err_msg))
    
    def put_file(self, file_path):
        user_dir = self.fs_user + gen_uuid() + str(int(time.time() * 1000 * 1000)) + '/' + os.path.basename(file_path)
        self.data_url  = self.filepath_url + user_dir 
        t = Transfer((self.fs_user, self.fs_passwd))
        t.put_file(self.data_url, file_path)

    def get_file(self, file_path, file_size, file_md5, url_list):
        t = Transfer((self.fs_user, self.fs_passwd))
        for url in url_list.split(';'):
            log.info('get protected file %s' % url)
            try:
                t.download_file(file_path, file_size, file_md5, url)
                return 
            except :
                pass
        raise(NetWorkException('download file failed'))


    def do_protection(self, file_path, para, output_file, keys, other_info):
        other_info['upload_begin_time'] = int(time.time())
        log.info('uploading file %s...' % file_path)
        self.put_file(file_path)
        log.info('do protection...')
        rsp_json = self.do_so_protection(file_path, para, keys, other_info)
        log.info('get protected file...')
        self.get_file(output_file, rsp_json['file_size'], rsp_json['file_md5'], rsp_json['file_url'])

    def upload_statics(self, statics):
        data = {
                'key': self.task_key,
                'download_endtime': int(time.time()),
                'reinforce_package_md5': statics['md5'],
                'reinforce_package_size' : statics['size']
                }

        t = Transfer()
        rsp = t.do_http_req(self.upload_statics_url, json.dumps(data))
        rsp_json = json.loads(rsp)
        if rsp_json['ret'] != 'success':
            log.info('uoload_statics failed')

    def get_version(self):
        data = {
                'magic': CLIENT_MAGIC
                }

        t = Transfer()
        rsp = t.do_http_req(self.get_version_url, json.dumps(data))
        rsp_json = json.loads(rsp)
        if rsp_json['ret'] != 'success':
            raise(ServerException('get version failed'))
        print('*******************version info*********************')
        print('latest_version : %s' % rsp_json['lastest_version'])
        print('stable_version : %s' % rsp_json['stable_version'])
        print('all_version_list :\n%s' % '\n'.join(rsp_json['all_version_list'].split(',')))
        print('*******************version info*********************')

def get_file_info(self, apk_file, output_file):
    total = 0
    file_info = dict()
    with zipfile.ZipFile(apk_file, 'r') as apk_zip:
        for item in apk_zip.infolist():
            if item.filename.startswith('META-INF/'):
                continue
            if item.file_size == 0:
                continue
            file_info[item.filename] = item.CRC
    with open(output_file, 'w') as fp:
        json.dump(file_info, fp)

def do_protect(file_path, user, para, output_file, other_info, t):
    aes = AesCryptor()
    enc_file = file_path + '.enc' 
    start = time.time()
    try:
        aes.encrypt_file(file_path, enc_file)
        keys = base64.b64encode(RsaCryptor().encrypt(aes.get_key()))
        t.do_protection(enc_file, para, output_file+'.enc', keys, other_info)
        aes.decrypt_file(output_file+'.enc', output_file)
    except Exception as e:
        log.info('we get an Exception... {}'.format(time.time()- start))
        raise e
    finally:
        try:
            log.info('cleaning tmp files... {}'.format(time.time()- start))
            os.remove(enc_file)
            os.remove(output_file+'.enc')
        except:
            pass

def unzip_dir(input_path, target_dir, unpack_dir):
    with zipfile.ZipFile(input_path, 'r') as apk_zip:
            for item in apk_zip.namelist():
                if (item.startswith(target_dir)):
                    apk_zip.extract(item, unpack_dir)

def filter_dir(dir_set):
    target_dir = set()
    for a in list(dir_set):
        for b in dir_set:
            if a in os.path.dirname(b) and a != b:
                target_dir.add(b)
    for b in target_dir:
        dir_set.remove(b)

class ApkPacker():
    def __init__(self, args):
        self.input_path = args.input_path
        self.user = args.user
        self.t = Client('https://safe-protection.bytedance.com', self.user)
        self.so_list = args.so_list.split(',') if (args.so_list) else list() 
        self.regular_games_protection = args.regular_games_protection
        self.strick_game_protection = args.strick_game_protection
        self.output_file = self.input_path + '.pt' if not args.output else args.output
        self.cpu_arch = args.cpu_arch
        self.file_verify = args.file_verify
        self.check_gp = args.check_gp
        self.zipalign = args.zipalign
        self.anti_debug = args.anti_debug
        self.anti_so_theft1_set = set(args.anti_so_theft1)
        self.anti_so_theft2_set = set(args.anti_so_theft2)
        self.anti_so_theft = len(self.anti_so_theft1_set) != 0 or len(self.anti_so_theft2_set) != 0

        ts = str(int(time.time() * 1000 * 1000))
        self.work_dir = os.path.join(cur_dir, 'tmp_workspace'+ts)
        self.unpack_dir = os.path.join(self.work_dir, 'unpack')
        self.metadata_path = ''
        self.para = dict()
        self.para['p'] =  self.regular_games_protection
        self.para['sg'] = self.strick_game_protection

        self.games_protection = self.strick_game_protection or self.regular_games_protection 

        if self.games_protection:
            if len(self.so_list) == 0:
                self.so_list = g_target_so
            else:
                self.so_list.extend([x for x in g_target_so if x not in self.so_list ])
        else:
            if self.cpu_arch:
                self.para['a'] = self.cpu_arch
            if len(self.so_list) != 0:
                self.para['l'] = ','.join(self.so_list)
        if args.anti_debug:
            self.para['anti'] = True 
        if args.file_verify:
            self.para['fv'] = True 
        if args.check_gp:
            self.para['gp'] = True 
        if len(self.anti_so_theft1_set) != 0:
            self.para['so_theft1'] = ' '.join(self.anti_so_theft1_set) 
        if len(self.anti_so_theft2_set) != 0:
            self.para['so_theft2'] = ' '.join(self.anti_so_theft2_set) 
        if args.only_uc:
            self.para['uc_env'] = True 
        if args.not_check_libc_hook:
            self.para['nlh'] = True 


        self.pt_so_list = list()
        self.other_info = dict()
        self.other_info['md5'] = calc_file_md5(self.input_path)
        self.other_info['size'] = os.stat(self.input_path).st_size

        if args.specifed_version:
            self.other_info['jiagu_version'] = args.specifed_version
        elif args.lastest_version:
            self.other_info['jiagu_version'] = 'lastest_version'
        elif args.stable_version:
            self.other_info['jiagu_version'] = 'stable_version'
        else:
            self.other_info['jiagu_version'] = 'lastest_version'
        self.other_info['appid'] = args.appid if args.appid else 'not specifed'
    
    def do_protection(self):

        if is_so(self.input_path):
            do_protect(self.input_path, self.user, self.para, self.output_file, self.other_info, self.t)
            return 

        if not is_apk(self.input_path):
            raise(InputNotZIpException('input file not apk nor so'))

        self.zi = check_so.ZipInfo(self.input_path)
        if self.games_protection:
            if not (self.zi.is_mono() or self.zi.is_il2cpp() or self.zi.is_neox() or self.zi.is_ue4() or self.zi.is_wings() or self.zi.is_cocos2dx_cxx() or self.zi.is_cocos2dx_lua() or self.zi.is_cocos2dx_js()):
                raise(NotSupportEngineException("not supported game-engine"))

        if is_sdk(self.input_path):
            if len(self.anti_so_theft2_set) != 0:
                raise(Theft2WithJarException('so_theft2 cannot handle with jar'))
            do_protect(self.input_path, self.user, self.para, self.output_file, self.other_info, self.t)
            return 

        shutil.rmtree(self.work_dir, ignore_errors = True)
        os.makedirs(self.unpack_dir)

        update_dir = set()
        update_file_list = list()
        self.needs_dex = True

        apk_zip = zipfile.ZipFile(self.input_path, 'r')
        for item in apk_zip.infolist():
            if item.filename.startswith('lib'):
                self.pt_so_list.append(item.filename)
                apk_zip.extract(item, self.unpack_dir)
            if item.filename == 'AndroidManifest.xml':
                apk_zip.extract(item, self.unpack_dir)
                continue

            if self.games_protection:
                if self.zi.is_il2cpp():
                    if item.filename.endswith('global-metadata.dat'):
                        apk_zip.extract(item, self.unpack_dir)
                        self.metadata_path = item.filename
                elif self.zi.is_mono():
                    if item.filename.endswith('Assembly-CSharp.dll') or item.filename.endswith('Assembly-CSharp-firstpass.dll'):
                        apk_zip.extract(item, self.unpack_dir)
                        self.csharp_dll_path = os.path.dirname(item.filename)
                        update_file_list.append(item.filename)
                elif self.zi.is_cocos2dx_lua():
                    if item.filename.startswith('assets/') and item.filename.endswith('.luac'): 
                        apk_zip.extract(item, self.unpack_dir)
                        #update_file_list.append(item.filename)
                        update_dir.add(os.path.dirname(item.filename))
                elif self.zi.is_cocos2dx_js():
                    if item.filename.startswith('assets/') and item.filename.endswith('.jsc'): 
                        apk_zip.extract(item, self.unpack_dir)
                        #update_file_list.append(item.filename)
                        update_dir.add(os.path.dirname(item.filename))

            if self.needs_dex:
                if item.filename.endswith('.dex') and 'classes' in item.filename:
                    apk_zip.extract(item, self.unpack_dir)
                    update_file_list.append(item.filename)   
        apk_zip.close()

        os.makedirs(os.path.join(self.work_dir, 'tmp_dir'))
        rsp_file_path = os.path.join(self.work_dir, 'tmp_dir', 'new.zip')
        part_input_path = os.path.join(self.work_dir, 'tmp_dir', 'part')
        file_info_path = os.path.join(self.unpack_dir, 'wk_file_info.json')
        if self.file_verify:
            get_file_info(self.work_dir, self.input_path, file_info_path)
            update_file_list.append('assets/fvif.wk')
        update_file_list.append('assets/mdt.wk')
        shutil.make_archive(part_input_path, 'zip', self.unpack_dir)
        do_protect(part_input_path+'.zip', self.user, self.para, rsp_file_path, self.other_info, self.t)
        os.remove(part_input_path+'.zip')

        shutil.rmtree(os.path.join(self.unpack_dir, 'lib'), ignore_errors = True)
        if self.games_protection:
            if self.zi.is_il2cpp():
                shutil.rmtree(os.path.join(self.unpack_dir, self.metadata_path[0 : self.metadata_path.find('/')]), ignore_errors = True)
      
        unzip_dir(self.input_path, 'lib', self.unpack_dir)
        zip_unpack_all(rsp_file_path, self.unpack_dir)
        log.info('repacking...')

        tmp_file = os.path.join(self.unpack_dir, os.path.basename(self.input_path))
        shutil.copy(self.input_path, tmp_file)

        delete(tmp_file, 'META-INF/*.SF')
        delete(tmp_file, 'META-INF/*.MF')
        delete(tmp_file, 'META-INF/*.RSA')

        cwd_dir = os.getcwd()
        os.chdir(self.unpack_dir)
        delete(tmp_file, 'lib/*' if platform.system() == 'Windows' else  'lib/\*')
        if self.games_protection: 
            if self.zi.is_il2cpp():
                update(tmp_file, self.metadata_path)
        for filename in  update_file_list:
            delete(tmp_file, filename)
            update(tmp_file, filename)
        add(tmp_file, 'lib/')
        filter_dir(update_dir)
        for item in update_dir:
            update(tmp_file, item)

        os.chdir(cwd_dir)
        
        if self.zipalign:
            zipalign(os.path.abspath(tmp_file), self.output_file)
        else:
            shutil.copy(os.path.abspath(tmp_file), self.output_file)
        statics = dict()
        statics['md5'] = calc_file_md5(self.output_file)
        statics['size'] = os.stat(self.output_file).st_size
        self.t.upload_statics(statics)
        return 

    def do_finishing(self):
        shutil.rmtree(self.work_dir, ignore_errors = True)


def set_env():
    if platform.system() == 'Windows':
        os.putenv('PATH', os.path.join(cur_dir, 'tools', 'win_zip') + ';' + os.getenv('PATH'))
    elif platform.system() == 'Darwin':
        pass
    else:
        pass

def which(name, flags=os.X_OK):
    """
    Search PATH for executable files with the given name.
    On newer versions of MS-Windows, the PATHEXT environment variable will be
    set to the list of file extensions for files considered executable. This
    will normally include things like ".EXE". This function will also find files
    with the given name ending with any of these extensions.
    On MS-Windows the only flag that has any meaning is os.F_OK. Any other
    flags will be ignored.
    @type name: C{str}
    @param name: The name for which to search.
    @type flags: C{int}
    @param flags: Arguments to L{os.access}.
    @rtype: C{list}
    @param: A list of the full paths to files found, in the order in which they
    were found.
    """
    result = []
    exts = list(filter(None, os.environ.get('PATHEXT', '').split(os.pathsep)))
    path = os.environ.get('PATH', None)

    if path is None:
        return []

    for p in os.environ.get('PATH', '').split(os.pathsep):
        p = os.path.join(p, name)
        if os.access(p, flags):
            result.append(p)
        for e in exts:
            pext = p + e
            if os.access(pext, flags):
                result.append(pext)

    return result
    
def exchange_ch_str(src):
    return src
    if sys.version_info.major < 3:
        return src.decode('utf-8').encode('gb2312')
    else:
        return src
    
def wk_main():
    """so protection
    通用游戏保护方案:
        python client.py -u your_user_id -i your_apk_file.apk -o new_apk_file.apk -p
    动态保护方案(包含反调试, 无文件完整性校验):
        python client.py -u your_user_id -i your_apk_file.apk -o new_apk_file.apk --mg
    动态保护方案(包含反调试和文件完整性校验):
        python client.py -u your_user_id -i your_apk_file.apk -o new_apk_file.apk --sg
    """
    parser = argparse.ArgumentParser(description=exchange_ch_str(wk_main.__doc__), formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("-p", action = "store_true", dest = "regular_games_protection", default = False,  help=exchange_ch_str("通用游戏保护方案"))
    parser.add_argument("--sg", action = "store_true", dest = "strick_game_protection", default = False,  help=exchange_ch_str("游戏动态保护方案, 包含反调试和文件校验"))
    parser.add_argument("--mg", action = "store_true", dest = "medium_game_protection", default = False,  help=exchange_ch_str("游戏动态保护方案, 包含反调试无文件校验功能"))

    parser.add_argument("-u", dest = "user", required = False,  help="user id")
    parser.add_argument("-i", dest = "input_path", required = False, help="input_path")
    parser.add_argument("-l", dest = "so_list", required = False, help="so list to be protected")
    parser.add_argument("-a", dest = "cpu_arch", required = False, help="cpu arch of so, support [armeabi, armeabi-v7a, arm64-v8a, x86, x86-64]. if not specifed, we treate armeabi-v7a as armeabi")      
    parser.add_argument("-o", dest = "output", required = False, help="output file")
    parser.add_argument("-c", action = "store_true", dest = "check", required = False, help="check if already do so protection")
    parser.add_argument("-z", action = "store_true", dest = "zipalign", required = False, help="set if needs zipalign. zipalign[.exe] shall be found in env PARH")
    parser.add_argument('--anti', dest = "anti_debug", action = "store_true", default = False, required = False, help="use anti-debug")
    parser.add_argument("--fv",   dest = "file_verify", action = "store_true", default = False, required = False, help="apk files check")
    parser.add_argument("--gp",   dest = "check_gp",  action = "store_true",  default = False, required = False, help="check gp sdk")
    parser.add_argument("--so_theft1",  dest = "anti_so_theft1", nargs='+', default=list(), required = False, help="anti so theft protection1, target so should have jni_onload function. sdk input is zip-formate, it contains xxx.jar and libs dir. you could use 'zip -r input.zip xxx.jar libs' to get a zip file")
    parser.add_argument("--so_theft2",  dest = "anti_so_theft2", nargs='+', default=list(), required = False, help="anti so theft protection1, target so should have jni_onload function, your input should be apk file")
    parser.add_argument("--only_uc",   dest = "only_uc",  action = "store_true",  default = False, required = False, help="apk counld only run in uc sdk environment")
    parser.add_argument("--version",   dest = "get_version",  action = "store_true",  default = False, required = False, help="get all versions")

    parser.add_argument("--stable",   dest = "stable_version",  action = "store_true",  default = False, required = False, help="use stable version")
    parser.add_argument("--latest",   dest = "lastest_version",  action = "store_true",  default = False, required = False, help="use latest version")
    parser.add_argument("--sv",   dest = "specifed_version",  required = False, help="use the specifed version")
    parser.add_argument("--appid",   dest = "appid",  required = False, help="your appid")
    parser.add_argument("--nlh",  dest = "not_check_libc_hook", action = "store_true", default = False, required = False, help="not check libc hook")

    log.info('using client version :' + so_protection_version)
    args = parser.parse_args()
    set_env()

    if args.get_version:
        try:
            Client('https://safe-protection.bytedance.com', args.user).get_version()
        except ServerException as e:
            wk_exit(check_so.WK_SERVER_ERROR, str(e))
        sys.exit(0)

    if not args.input_path or not os.path.exists(args.input_path):
        log.error('input file %s not exists' % args.input_path)
        wk_exit(check_so.WK_INPUT_NOT_EXIST, 'input file not exists')
    args.input_path = os.path.abspath(args.input_path)
    
    if args.check:
        sys.exit(check_so.is_already_protected(args.input_path, g_target_so))
    if not args.user:
        log.error('you must set -u user')
        wk_exit(check_so.WK_PARA_ERROR, 'you must set -u user')

    if args.medium_game_protection:
        args.regular_games_protection = True
        args.anti_debug = True
    if not (args.strick_game_protection or args.regular_games_protection  or args.so_list or len(args.anti_so_theft1) == 0 or len(args.anti_so_theft2) == 0):
        log.error('you should specify a protection modle')
        wk_exit(check_so.WK_PARA_ERROR, 'para error')


    if args.strick_game_protection:
        args.file_verify = args.anti_debug = True
    try:
        inst = ApkPacker(args)
        inst.do_protection()
    except NotSupportEngineException as e:
        wk_exit(check_so.WK_NOT_SUPPORT_ENGINE, str(e))
    except InputNotZIpException as e:
        wk_exit(check_so.WK_INPUT_NOT_ZIP_ERROR, str(e))
    except Theft2WithJarException as e:
        wk_exit(check_so.WK_PARA_ERROR, str(e))
    except ServerException as e:
        wk_exit(check_so.WK_SERVER_ERROR, str(e))
    except UpdateZipException as e:
        wk_exit(check_so.WK_UPDATE_PAKCAGE_ERROR, str(e))
    except NetWorkException as e:
        wk_exit(check_so.WK_NETWORK_ERROR, str(e))
    except AleadyPortectException as e:
        wk_exit(check_so.WK_ALREADY_PROTECTED_ERROR, str(e))
    except Exception as e:
        log.exception('protection failed other error : %s' % str(e))
        wk_exit(check_so.WK_OTHER_ERROR, str(e))
    finally:
        log.info('cleaning..')
        inst.do_finishing()

    log.info('protection successfully done')
    sys.exit(0)

