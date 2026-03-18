
import os, sys, json, time
from ctypes import *
from binascii import unhexlify
import zipfile
import shutil 

WK_SUCCESS = 0
WK_NOT_PROTECTED = 1

WK_OTHER_ERROR = 31
WK_INPUT_NOT_EXIST = 32
WK_ALREADY_PROTECTED_ERROR = 33
WK_PARA_ERROR = 34
WK_NOT_SUPPORT_ENGINE = 35
WK_INPUT_NOT_ZIP_ERROR = 36
WK_UPDATE_PAKCAGE_ERROR = 37
WK_SERVER_ERROR = 38
WK_NETWORK_ERROR = 39


class ELFFlags(object):
    ELFCLASS32  = 0x01
    ELFCLASS64  = 0x02
    EI_CLASS    = 0x04
    EI_DATA     = 0x05
    ELFDATA2LSB = 0x01
    ELFDATA2MSB = 0x02
    EM_386      = 0x03
    EM_X86_64   = 0x3e
    EM_ARM      = 0x28
    EM_MIPS     = 0x08
    EM_SPARCv8p = 0x12
    EM_PowerPC  = 0x14
    EM_ARM64    = 0xb7

class Elf32_Ehdr_LSB(LittleEndianStructure):
    _fields_ =  [
                    ("e_ident",         c_ubyte * 16),
                    ("e_type",          c_ushort),
                    ("e_machine",       c_ushort),
                    ("e_version",       c_uint),
                    ("e_entry",         c_uint),
                    ("e_phoff",         c_uint),
                    ("e_shoff",         c_uint),
                    ("e_flags",         c_uint),
                    ("e_ehsize",        c_ushort),
                    ("e_phentsize",     c_ushort),
                    ("e_phnum",         c_ushort),
                    ("e_shentsize",     c_ushort),
                    ("e_shnum",         c_ushort),
                    ("e_shstrndx",      c_ushort)
                ]

class Elf64_Ehdr_LSB(LittleEndianStructure):
    _fields_ =  [
                    ("e_ident",         c_ubyte * 16),
                    ("e_type",          c_ushort),
                    ("e_machine",       c_ushort),
                    ("e_version",       c_uint),
                    ("e_entry",         c_ulonglong),
                    ("e_phoff",         c_ulonglong),
                    ("e_shoff",         c_ulonglong),
                    ("e_flags",         c_uint),
                    ("e_ehsize",        c_ushort),
                    ("e_phentsize",     c_ushort),
                    ("e_phnum",         c_ushort),
                    ("e_shentsize",     c_ushort),
                    ("e_shnum",         c_ushort),
                    ("e_shstrndx",      c_ushort)
                ]

class Elf32_Shdr_LSB(LittleEndianStructure):
    _fields_ =  [
                    ("sh_name",         c_uint),
                    ("sh_type",         c_uint),
                    ("sh_flags",        c_uint),
                    ("sh_addr",         c_uint),
                    ("sh_offset",       c_uint),
                    ("sh_size",         c_uint),
                    ("sh_link",         c_uint),
                    ("sh_info",         c_uint),
                    ("sh_addralign",    c_uint),
                    ("sh_entsize",      c_uint)
                ]

class Elf64_Shdr_LSB(LittleEndianStructure):
    _fields_ =  [
                    ("sh_name",         c_uint),
                    ("sh_type",         c_uint),
                    ("sh_flags",        c_ulonglong),
                    ("sh_addr",         c_ulonglong),
                    ("sh_offset",       c_ulonglong),
                    ("sh_size",         c_ulonglong),
                    ("sh_link",         c_uint),
                    ("sh_info",         c_uint),
                    ("sh_addralign",    c_ulonglong),
                    ("sh_entsize",      c_ulonglong)
                ]

class ELF(object):
    def __init__(self, binary):
        self.__binary    = bytearray(binary)
        self.__ElfHeader = None
        self.__shdr_l    = []

        self.__setHeaderElf()
        self.__setShdr()

    """ Parse ELF header """
    def __setHeaderElf(self):
        e_ident = self.__binary[:15]

        ei_class = e_ident[ELFFlags.EI_CLASS]
        ei_data  = e_ident[ELFFlags.EI_DATA]

        if ei_class != ELFFlags.ELFCLASS32 and ei_class != ELFFlags.ELFCLASS64:
            raise Exception("[Error] ELF.__setHeaderElf() - Bad Arch size")

        if ei_data != ELFFlags.ELFDATA2LSB and ei_data != ELFFlags.ELFDATA2MSB:
            raise Exception("[Error] ELF.__setHeaderElf() - Bad architecture endian")

        if ei_class == ELFFlags.ELFCLASS32:
            if  ei_data == ELFFlags.ELFDATA2LSB: self.__ElfHeader = Elf32_Ehdr_LSB.from_buffer_copy(self.__binary)
            else:
                raise Exception("[Error] not support MSB")
        elif ei_class == ELFFlags.ELFCLASS64:
            if   ei_data == ELFFlags.ELFDATA2LSB: self.__ElfHeader = Elf64_Ehdr_LSB.from_buffer_copy(self.__binary)
            else:
                raise Exception("[Error] not support MSB")

    """ Parse Section header """
    def __setShdr(self):
        shdr_num = self.__ElfHeader.e_shnum
        base = self.__binary[self.__ElfHeader.e_shoff:]
        shdr_l = []

        e_ident = self.__binary[:15]
        ei_data = e_ident[ELFFlags.EI_DATA]

        for i in range(shdr_num):
            if self.__ElfHeader.e_ident[ELFFlags.EI_CLASS] == ELFFlags.ELFCLASS32:
                if   ei_data == ELFFlags.ELFDATA2LSB: shdr = Elf32_Shdr_LSB.from_buffer_copy(base)
                else:
                    raise Exception("[Error] not support MSB")
            elif self.__ElfHeader.e_ident[ELFFlags.EI_CLASS] == ELFFlags.ELFCLASS64:
                if   ei_data == ELFFlags.ELFDATA2LSB: shdr = Elf64_Shdr_LSB.from_buffer_copy(base)
                else:
                    raise Exception("[Error] not support MSB")

            self.__shdr_l.append(shdr)
            base = base[self.__ElfHeader.e_shentsize:]

        # setup name from the strings table
        if self.__ElfHeader.e_shstrndx != 0:
            string_table = bytes(self.__binary[(self.__shdr_l[self.__ElfHeader.e_shstrndx].sh_offset):])
            for i in range(shdr_num):
                self.__shdr_l[i].str_name = string_table[self.__shdr_l[i].sh_name:].split(b'\x00')[0].decode('utf8')

    def getShdr(self):
        return self.__shdr_l

class Binary(object):
    def __init__(self, file_path):
        self.binary_file_path = file_path
        self.__binary = None
        self.offset = 0
        try:
            with open(self.binary_file_path, 'rb') as fd:
                __rawBinary = fd.read()
        except:
            raise(Exception("[Error] Can't open the binary or binary not found[{0}]".format(self.binary_file_path)))
        if __rawBinary[:4] == unhexlify(b"7f454c46"):
            self.__binary = ELF(__rawBinary)
        else:
            raise(Exception('not support binary file'))

    def getBinary(self):
        return self.__binary

class ZipInfo():
    def __init__(self, path):
        self.filepath = path
        self.mono_1_0 = False
        self.mono_2_0 = False
        self.il2cpp = False
        self.neox = False
        self.ue4 =  False
        self.wings = False
        self.cocos2dx_lua = False
        self.cocos2dx_cxx = False
        self.cocos2dx_js  = False

        with zipfile.ZipFile(self.filepath, 'r') as apk_zip:
            for item in apk_zip.infolist():
                if item.filename.startswith('lib'):
                    if 'libmono.so' in item.filename:
                        self.mono_1_0 = True
                    elif 'libmonobdwgc-2.0.so' in item.filename:
                        self.mono_2_0 = True
                    elif 'libil2cpp.so' in item.filename:
                        self.il2cpp = True
                    elif 'libclient.so' in item.filename:
                        self.neox = True 
                    elif 'libUE4.so' in item.filename:
                        self.ue4 = True 
                    elif 'libFancy3D.so' in item.filename:
                        self.wings = True
                    elif 'libcocos2dlua.so' in item.filename:
                        self.cocos2dx_lua = True
                    elif 'libMyGame.so' in item.filename:
                        self.cocos2dx_cxx = True
                    elif 'libcocos2djs.so' in item.filename:
                        self.cocos2dx_js = True

    def is_neox(self):
        return self.neox
   
    def is_mono(self):
        return self.mono_1_0 or self.mono_2_0

    def is_mono_1_0(self):
        return self.mono_1_0

    def is_mono_2_0(self):
        return self.mono_2_0

    def is_il2cpp(self):
        return self.il2cpp

    def is_ue4(self):
        return self.ue4

    def is_wings(self):
        return self.wings

    def is_cocos2dx_lua(self):
        return self.cocos2dx_lua
    
    def is_cocos2dx_js(self):
        return self.cocos2dx_js

    def is_cocos2dx_cxx(self):
        return self.cocos2dx_cxx

def is_so_already_protected(file_path):
    binary = Binary(file_path)
    shdr_list = binary.getBinary().getShdr() 
    for shdr in shdr_list:
        if ('.bdeg' == shdr.str_name):
            print('already protected for %s' % binary.binary_file_path)
            return 0 
    print('no protection for %s' % binary.binary_file_path)
    return WK_NOT_PROTECTED

def is_already_protected(apk_path, target_so):
    work_dir = 'tmp_workspace_for_check_' + str(int(time.time()*1000 * 10))
    ret = WK_NOT_PROTECTED
    if (not zipfile.is_zipfile(apk_path)):
        return is_so_already_protected(apk_path)
    else:
        if (not os.path.exists(work_dir)):
            os.makedirs(work_dir)
        with zipfile.ZipFile(apk_path, 'r') as apk_zip:
            for item in apk_zip.infolist():
                if (item.filename.startswith('lib')): 
                    if (any(x in item.filename for x in target_so)):
                        apk_zip.extract(item, work_dir)
                        ret = is_so_already_protected(os.path.join(work_dir, item.filename))
                        if ret == WK_NOT_PROTECTED:
                            break
        shutil.rmtree(work_dir, ignore_errors = True)
        return ret 
