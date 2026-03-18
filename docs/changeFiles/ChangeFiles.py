import os
import codecs
import chardet

def list_folders_files(pathNow):

    # print("pathNow: " + pathNow)

    # """
    # 递归处理此目录下的所有文件
    # """
    list_folders = []
    list_files = []
    for file in os.listdir(pathNow):
        file_path = os.path.join(pathNow, file)
        if os.path.isdir(file_path):
            list_folders.append(file_path)
        else:
            filenames = file.split('.')
            fileNameSplitCountCheck = (len(filenames) > 1)
            fileNameExtCheck = (filenames[len(filenames)-1] == "cs")
            if fileNameSplitCountCheck & fileNameExtCheck:
                list_files.append(file)
    
    for fileName in list_files:
        filePath = pathNow + '/' + fileName
        # print("filePath: " + filePath)
        with open(filePath, "rb") as f:
            data = f.read()
            codeType = chardet.detect(data)['encoding']
            if codeType == 'GB2312':#当前进检测的文件编码为GB2312，其它格式的自动转换可能导致中文乱码
                convert(filePath, codeType, 'UTF-8')
    for folderName in list_folders:
        list_folders_files(folderName)

def convert(file, in_enc="ANSI", out_enc="UTF-8"):
    """
    该程序用于将目录下的文件从指定格式转换到指定格式
    """
    in_enc = in_enc.upper()
    out_enc = out_enc.upper()
    try:
        print("convert [ " + file.split('\\')[-1] + " ].....From " + in_enc + " --> " + out_enc)
        f = codecs.open(file, 'r', in_enc, "ignore")
        new_content = f.read()
        codecs.open(file, 'w', out_enc).write(new_content)
    except IOError as err:
        print("I/O error: {0}".format(err))

# 将路径下面的所有文件，从原来的格式变为UTF-8的格式
if __name__ == "__main__":
    list_folders_files("/Users/droidhenmini/Documents/bigBang/bigBangClient")
    