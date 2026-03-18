# coding=utf-8

import os
import csv
import traceback

GLOBALIZATION_CONFIG_PATH = "Assets/Resources/Config/cfg_globalization.csv"
BLACK_CHINESE_CONFIG_PATH = "Assets/Resources/Config/cfg_black_chinese.txt"
CONFIG_CHINESE_EXPORT_FILE = "Assets/Resources/Config/config_chinese.txt"
UI_CHINESE_EXPORT_FILE = "Assets/Resources/Config/ui_chinese.txt"

def initConfig():
    f = open(GLOBALIZATION_CONFIG_PATH, 'w', encoding='utf-8')
    f.write('id,text_cn')
    f.close()

def readExistConfig():
    f = open(GLOBALIZATION_CONFIG_PATH, "r", encoding='utf-8')
    csv_reader = csv.reader(f)
    headers = None
    rows = []
    for row in csv_reader:
        if headers == None:
            headers = row
        else:
            rows.append(row)
    f.close()
    return len(headers), rows

def loadExportChinese(file):
    f = open(file, "r", encoding='utf-8')
    if not f:
        print("!!!!invalid file: " + file + " !!!!!")
    lines = f.readlines()
    ret = []
    for line in lines:
        str = line.strip()
        if len(str) > 0:
            ret.append(str)
    return ret

def isExist(rows, str):
    for row in rows:
        if row[1] == str:
            return True
    return False

def isBlackChinese(black_chinese, str):
    for black_line in black_chinese:
        if black_line == str:
            return True
    return False

def fillChinese(chinese, col_count, rows):
    black_file = open(BLACK_CHINESE_CONFIG_PATH, "r", encoding='utf-8')
    black_chiense = black_file.readlines()
    for i in range(0, len(black_chiense)):
        black_chiense[i] = black_chiense[i].strip()

    f = open(GLOBALIZATION_CONFIG_PATH, "a", encoding='utf-8')
    for s in chinese:
        if isExist(rows, s) == False and isBlackChinese(black_chiense, s) == False:
            id = len(rows) + 1
            f.write("\n" + str(id) + "," + s)
            if col_count > 2:
                for i in range(2, col_count):
                    f.write(',')
            rows.append([len(rows) + 1, s])
    f.close()

try:
    if os.path.exists(GLOBALIZATION_CONFIG_PATH) == False:
        initConfig()

    col_count, rows = readExistConfig()
    fillChinese(loadExportChinese(UI_CHINESE_EXPORT_FILE), col_count, rows)
    fillChinese(loadExportChinese(CONFIG_CHINESE_EXPORT_FILE), col_count, rows)
    exit(0)

except Exception as e:
    print(e)
    traceback.print_exc()
    exit(1)