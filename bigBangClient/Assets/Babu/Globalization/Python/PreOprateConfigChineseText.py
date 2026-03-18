import csv
import os
import traceback
import re

GLOBALIZATION_CONFIG_PATH = "Assets/Resources/Config/cfg_globalization.csv"
CONFIG_CHINESE_EXPORT_FILE = "Assets/Resources/Config/config_chinese.txt"
CONFIG_PATH = "Assets/Resources/Config/"

IGNORE_CONFIGS = [
    'cfg_globalization.csv',
    'cfg_coach.csv',
    'cfg_player.csv',
    'cfg_club.csv',
    'cfg_name.csv',
    'cfg_support_lang.csv'
]
IGNORE_COLS = [
    ('cfg_lang.csv', 'desc')
]


CHINESE_REGEX = re.compile(u'[\u4e00-\u9fa5]+')

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

globalization_config_rows = []
def loadGloblizationConfig():
    f = open(GLOBALIZATION_CONFIG_PATH, "r", encoding='utf-8')
    csv_reader = csv.reader(f)
    for row in csv_reader:
        globalization_config_rows.append(row)

def getGlobalizationConfigIndex(str):
    for row in globalization_config_rows:
        if row[1] == str:
            return row[0]
    return ""


def preOprateChinese(file_path):
    f = open(file_path, "r", encoding='utf-8')
    reader = csv.reader(f)
    rows = []
    for row in reader:
        rows.append(row)
    f.close()

    header = rows[0]
    file_name = getFileName(file_path)
    has_operated = False
    for index in range(1, len(rows)):
        row = rows[index]
        for i in range(0, len(row)):
            if (isIgnoreCol(file_name, header[i]) == False) and isChinese(row[i]):
                index = getGlobalizationConfigIndex(row[i].strip())
                if index != "":
                    row[i] = '$$' + index
                    has_operated = True
    if has_operated:
        f = open(file_path, "w", encoding='utf-8')
        for row in rows:
            f.write(",".join(row) + "\n")
        f.close()

try:
    loadGloblizationConfig()

    file_list = [CONFIG_PATH + i for i in os.listdir(CONFIG_PATH)]
    for file_path in file_list:
        if file_path.endswith(".csv") and not isIgnoreConfig(file_path):
            preOprateChinese(file_path)
        
except Exception as e:
    print(e)
    traceback.print_exc()
    exit(1)
