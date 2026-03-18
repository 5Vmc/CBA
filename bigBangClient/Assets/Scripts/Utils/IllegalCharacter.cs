using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BigBang;
using UnityEngine;
using Utils;

namespace Babu
{
    public class IllegalCharacter : MonoBehaviour
    {
        static Dictionary<int, string> _illegalDic = new Dictionary<int, string>();
        static bool _isInit = false;


        public static void Init(string text)
        {
            if (Babu.Globalization.Globalizer.Instance.GetCurLanguageType() == Babu.Globalization.Globalizer.LanguageType.English)
            {
                //return;
            }

            string[] names = text.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < names.Length; ++i)
            {
                string name = names[i].ToLower();
                if (_illegalDic.ContainsKey(GetHashCode(name)))
                {
                    continue;
                }

                _illegalDic.Add(GetHashCode(name), name);
            }
        }

        /// <summary>
        /// 检查名字是否违规
        /// 返回true代表不能使用
        /// </summary>
        public static void IsNameCanNotUse(string nameStr, bool useNetWorkCheck, Action<bool> callback)
        {
            nameStr = nameStr.Trim();
            // 汉字编码
            Regex regex = new Regex("[\u4e00-\u9fa5]");
            // 长度检测
            int chinese = nameStr.ToCharArray().Count(item => regex.IsMatch(item.ToString()));
            if ((nameStr.Length - chinese + chinese * 2) > 15)
            {
                Tips.PopError(ErrorID.NameOverflow);
                callback.Invoke(true);
                return;
            }
            if (string.IsNullOrEmpty(nameStr))
            {
                Tips.PopError(ErrorID.NameEmpty);
                callback.Invoke(true);
                return;
            }
            if (useNetWorkCheck)
            {
                // 名字非法检测
                CheckStringContainIllegalCharacterLocalAndNetwork(nameStr, (bool isStringContainIllegalCharacter) =>
                {
                    if (isStringContainIllegalCharacter)
                    {
                        Tips.PopError(ErrorID.IllegalName);
                    }
                    callback.Invoke(isStringContainIllegalCharacter);
                });
            }
            else
            {
                if (IsStringContainIllegalCharacterLocal(nameStr))
                {
                    Tips.PopError(ErrorID.IllegalName);
                    callback.Invoke(true);
                }
                else
                {
                    callback.Invoke(false);
                }
            }
        }

        /// <summary> 
        /// 检查非法字符
        /// 先检查客户端本地敏感词库，过了再去服务器检查
        /// 返回true代表有敏感词
        ///  </summary>
        public static void CheckStringContainIllegalCharacterLocalAndNetwork(string str, Action<bool> callback)
        {
            if (IsStringContainIllegalCharacterLocal(str))
            {
                callback.Invoke(true);
                return;
            }
            NetworkManager.Instance.CheckStringContainIllegalCharacter(str, callback);
        }

        public static bool IsStringContainIllegalCharacterLocal(string str)
        {
            // if (Babu.Globalization.Globalizer.Instance.IsInternationalVersion())
            // {
            //     return true;
            // }

            str = str.ToLower();
            // if (!InternationalLogic.IsInternationalVersion())
            // {
            //    TTContentVerify.Result result = TTContentVerify.VerifyUsername(str);
            //    if (result == TTContentVerify.Result.Invalid)
            //    {
            //        return false;
            //    }
            // }

            if (!IsStringLegal(str))
            {
                return true;
            }

            // char[] bytes = str.ToCharArray();
            // for (int i = 0; i < bytes.Length; ++i)
            // {
            //    if (char.IsWhiteSpace(bytes[i]) || char.IsPunctuation(bytes[i])
            //        || char.IsNumber((bytes[i])) || char.IsSeparator(bytes[i])
            //        || char.IsSymbol(bytes[i]))
            //    {
            //        return false;
            //    }
            // }

            return false;
        }

        public static bool IsStringLegal(string str)
        {
            str = str.ToLower();
            if (_illegalDic.ContainsKey(GetHashCode(str)))
            {
                return false;
            }
            List<string> values = new List<string>(_illegalDic.Values);
            for (int i = 0; i < values.Count; ++i)
            {
                if (str.Contains(values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static int GetHashCode(string obj)
        {

            if (obj == null)
                return 0;
            return obj.GetHashCode();
        }
    }
}