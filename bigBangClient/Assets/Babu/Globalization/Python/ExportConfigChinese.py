import csv
import os
import traceback
import re

CONFIG_CHINESE_EXPORT_FILE = "Assets/Resources/Config/config_chinese.txt"
CONFIG_PATH = "Assets/Resources/Config/"

IGNORE_CONFIGS = [
    'cfg_globalization.csv',
    'cfg_support_lang.csv'
]
IGNORE_COLS = [
    ('cfg_lang.csv', 'desc')
]


CHINESE_REGEX = re.compile(u'[\u4e00-\u9fa5]+')

def get_file(root_path,all_files=[]):
	files = os.listdir(root_path)
	for file in files:
		if not os.path.isdir(root_path + "/" + file):
			all_files.append(root_path + "/" + file)
		else:
			get_file((root_path+"/"+file),all_files)
	return all_files

def isChinese(word):
    return CHINESE_REGEX.search(word)


def getFileName(file_path):
    pos = str.rindex(file_path, '/')
    if pos != -1:
        return file_path[pos+1:]
    return file_path

def isIgnoreConfig(file_path):
    for config in IGNORE_CONFIGS:
        file_name = getFileName(file_path)
        if config == file_name:
            return True
    return False

def isIgnoreCol(file_name, col_name):
    for row in IGNORE_COLS:
        if row[0] == file_name and row[1] == col_name:
            return True
    return False

def fetchChinese(file_path):
    f = open(file_path, "r", encoding='utf-8')
    reader = csv.reader(f)
    rows = []
    for row in reader:
        rows.append(row)

    header = rows[0]
    file_name = getFileName(file_path)
    chinese = []
    for index in range(1, len(rows)):
        row = rows[index]
        for i in range(0, len(row)):
            if (isIgnoreCol(file_name, header[i]) == False) and isChinese(row[i]):
                chinese.append(row[i].strip())
    return chinese

def output(chinese):
    f = open(CONFIG_CHINESE_EXPORT_FILE, "w", encoding='utf-8')
    for i in range(0, len(chinese)):
        if i == 0:
            f.write(chinese[i])
        else:
            f.write("\n" + chinese[i])
        
try:
    #file_list = [CONFIG_PATH + i for i in os.listdir(CONFIG_PATH)]
    file_list = get_file(CONFIG_PATH)
    chinese = []
    for file_path in file_list:
        if file_path.endswith(".csv") and not isIgnoreConfig(file_path):
            chinese.extend(fetchChinese(file_path))
    output(chinese)
        
except Exception as e:
    print(e)
    traceback.print_exc()
    exit(1)
