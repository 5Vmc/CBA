using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using BigBang;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class Utility
    {

        /// <summary>
        /// 英文首字母大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToFirstUpper(this string str)
        {
            if (string.IsNullOrWhiteSpace(str) == true)
            {
                return "";
            }
            if (str.Length == 1)
            {
                return str.ToUpper();
            }
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
        /// <summary>
        /// 英文首字母小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToFirstLower(this string str)
        {
            if (string.IsNullOrWhiteSpace(str) == true)
            {
                return "";
            }
            if (str.Length == 1)
            {
                return str.ToLower();
            }
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }

        /// <summary>
        /// 格式化文字，防止报错
        /// </summary>
        /// <param name="format">格式文字</param>
        /// <param name="args">参数列表</param>
        /// <returns></returns>
        public static string SafeFormat(this string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format) == true)
            {
                return "";
            }

            if (args == null || args.Length == 0)
            {
                return format;
            }

            try
            {
                return string.Format(format, args);
            }
            catch
            {
                UnityEngine.Debug.LogWarningFormat("Utility , SafeFormat , Format error , format = {0} , args.count = {1}", format, args.Length);
                return "";
            }
        }

        /// <summary>
        /// 格式化文字，防止报错
        /// 出现错误时不报警告
        /// </summary>
        /// <param name="format">格式文字</param>
        /// <param name="args">参数列表</param>
        /// <returns></returns>
        public static string SafeFormatNoWarn(this string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format) == true)
            {
                return "";
            }

            if (args == null || args.Length == 0)
            {
                return format;
            }

            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 使用摄像机将屏幕坐标转为世界坐标
        /// </summary>
        public static Vector3 ConvertScreenPositionToWorldPosition(RectTransform rect, Vector2 screenPoint, Camera cam)
        {
            Vector3 worldPoint = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, cam, out worldPoint);
            return worldPoint;
        }
        /// <summary>
        /// 使用摄像机将世界坐标转为屏幕坐标
        /// </summary>
        public static Vector3 ConvertWorldPositionToScreenPosition(Vector3 worldPoint, Camera cam)
        {
            Vector3 screenPoint = Vector3.zero;
            screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPoint);
            return screenPoint;
        }


        /// <summary>
        /// 使用摄像机将屏幕坐标转换成rect下的本地坐标
        /// </summary>
        public static Vector3 ConvertScreenPositionToLocalPosition(RectTransform rect, Vector2 screenPoint, Camera cam)
        {
            Vector2 localPoint = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out localPoint);
            return localPoint;
        }
        /// <summary>
        /// 使用摄像机将rect下的本地坐标转换成屏幕坐标
        /// </summary>
        public static Vector3 ConvertLocalPositionToScreenPosition(RectTransform rect, Vector3 localPoint, Camera cam)
        {
            Vector3 worldPoint = ConvertLocalPositionToWorldPosition(rect, localPoint);
            Vector3 screenPoint = ConvertWorldPositionToScreenPosition(worldPoint, cam);
            return screenPoint;
        }

        /// <summary>
        /// 将transform下的本地坐标转换成targetTransform下的本地坐标
        /// </summary>
        public static Vector3 ConvertLocalPosition(Transform transform, Vector3 localPosition, Transform targetTransform)
        {
            return targetTransform.InverseTransformPoint(transform.TransformPoint(localPosition));
        }

        /// <summary>
        /// 将transform下的本地坐标转换成世界坐标
        /// </summary>
        public static Vector3 ConvertLocalPositionToWorldPosition(Transform transform, Vector3 localPosition)
        {
            return transform.TransformPoint(localPosition);
        }

        /// <summary>
        /// Shuffle random lists
        /// </summary>
        public static void ShuffleList<E>(List<E> list)
        {
            System.Random r = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                E value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Get color value as hex value
        /// </summary>
        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", ""); //in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", ""); //in case the string is formatted #FFFFFF
            byte a = 255; //assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return new Color32(r, g, b, a);
        }

        public static string ColorToHex(Color color)
        {
            Color32 c = color;
            return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
        }

        /// <summary>
        /// int -> 1,000,000 conversion string
        /// </summary>
        public static string ChangeThousandsSeparator(int myScore)
        {
            return string.Format("{0:n0}", myScore);
        }


        /// <summary>
        /// Find and import sprite by name
        /// </summary>
        public static Sprite GetItemSprite(Sprite[] sprites, string name)
        {
            foreach (Sprite sprite in sprites)
            {
                if (sprite.name == name)
                {
                    return sprite;
                }
            }

            return null;
        }



        /// <summary>
        /// Internet status check
        /// </summary>
        public static bool CheckInternet
        {
            get
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    //no internet
                    return false;
                }
                else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                {
                    //3g4g5g
                    return true;
                }
                else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
                {
                    //wifi
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// get random int inclusive min and inclusive max
        /// </summary>
        /// <param name="min">minInclusive</param>
        /// <param name="max">maxInclusive</param>
        public static int GetRandomInt(int min, int max)
        {
            return UnityEngine.Random.Range(min, max + 1);
        }
        /// <summary>
        /// get random float inclusive min and inclusive max
        /// </summary>
        /// <param name="min">minInclusive</param>
        /// <param name="max">maxInclusive</param>
        public static float GetRandomFloat(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        /// <summary>
        /// get random bool
        /// </summary>
        public static bool GetRandomBool()
        {
            return GetRandomInt(0, 1) == 0;
        }

        /// <summary>
        /// 确保target在min到max之间
        /// </summary>
        public static int KeepInRange(int target, int min, int max)
        {
            if (target < min) return min;
            if (target > max) return max;
            return target;
        }
        /// <summary>
        /// 确保target在min到max之间
        /// </summary>
        public static float KeepInRange(float target, float min, float max)
        {
            if (target < min) return min;
            if (target > max) return max;
            return target;
        }

        /**
         * 将秒转为时分秒的格式
         */
        public static string TimeToHMS(int second, bool dotSeparator = true)
        {
            if (second < 0) return "";
            int hours = second / 3600;
            int minutes = (second - hours * 3600) / 60;
            int seconds = second - hours * 3600 - minutes * 60;
            StringBuilder sb = new StringBuilder();
            if (hours > 0)
            {
                sb.Append(hours);
                if (dotSeparator)
                {
                    sb.Append(":");
                }
                else
                {
                    sb.Append("h ");
                }
            }
            if (hours > 0 || minutes > 0)
            {
                if (minutes < 10)
                {
                    sb.Append(0);
                }
                sb.Append(minutes);
                if (dotSeparator)
                {
                    sb.Append(":");
                }
                else
                {
                    sb.Append("m ");
                }
            }
            if (seconds < 10)
            {
                sb.Append(0);
            }
            sb.Append(seconds);
            if (!dotSeparator)
            {
                sb.Append("s");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 打乱一个List
        /// </summary>
        /// <typeparam name="T">list的类型</typeparam>
        /// <param name="list">list</param>
        public static void RandomSortSelf<T>(this List<T> list)
        {
            int indexI = 0;
            int indexRandom = 0;
            for (int i = 0; i < list.Count; i++)
            {
                indexI = i;
                indexRandom = UnityEngine.Random.Range(0, list.Count);
                T swap = list[indexI];
                list[indexI] = list[indexRandom];
                list[indexRandom] = swap;
            }
        }

        /// <summary>
        /// 获得现在是今年的第几周
        /// </summary>
        /// <returns>周数</returns>
        public static int GetWeekOfYear()
        {
            GregorianCalendar gregorianCalendar = new();
            return gregorianCalendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        /// <summary>
        /// 获得现在是本周的第几天
        /// </summary>
        /// <returns>第几天，周一到周日分别是0-6</returns>
        public static int GetDayOfWeek()
        {
            GregorianCalendar gregorianCalendar = new();
            DayOfWeek dayOfWeek = gregorianCalendar.GetDayOfWeek(DateTime.Now);
            int dayOfWeekInt = (int)dayOfWeek;
            dayOfWeekInt -= 1;
            if (dayOfWeekInt == -1) dayOfWeekInt = 6;
            return dayOfWeekInt;
        }

        /// <summary>
        /// 将字符类型存的数字列表转换为对象
        /// </summary>
        /// <param name="str">“1,2,3,4”</param>
        /// <returns>new List<int>(1,2,3,4)</returns>
        public static List<int> getIntListFormString(string str)
        {
            List<int> intList = new();
            if (string.IsNullOrWhiteSpace(str)) return intList;
            string[] strArr = str.Split(',');
            int intNum = 0;
            for (int i = 0; i < strArr.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(strArr[i])) continue;
                if (int.TryParse(strArr[i], out intNum))
                {
                    intList.Add(intNum);
                }
                else
                {
                    Debug.LogWarning("getIntListFormString , int.TryParse error , strArr[i] = " + strArr[i]);
                    continue;
                }
            }
            return intList;
        }

        /// <summary>
        /// 设置gameObject节点及其所有子节点的layer
        /// </summary>
        /// <param name="gameObject">要设置的根节点（此节点也会更改）</param>
        /// <param name="layer">新的layer</param>
        public static void SetLayerInThisAndAllChild(this GameObject gameObject, int layer)
        {
            foreach (Transform tran in gameObject.GetComponentsInChildren<Transform>(true))//遍历当前物体及其所有子物体，看现象为深度优先遍历
            {
                tran.gameObject.layer = layer;//更改物体的Layer层
            }
        }
        /// <summary>
        /// 设置gameObject节点及其所有子节点的Tag
        /// </summary>
        /// <param name="gameObject">要设置的根节点（此节点也会更改）</param>
        /// <param name="layer">新的layer</param>
        public static void SetTagInThisAndAllChild(this GameObject gameObject, string tag)
        {
            foreach (Transform tran in gameObject.GetComponentsInChildren<Transform>(true))//遍历当前物体及其所有子物体，看现象为深度优先遍历
            {
                tran.gameObject.tag = tag;//更改物体的Tag层
            }
        }
        /// <summary>
        /// 设置gameObject节点及其所有子节点的Tag
        /// </summary>
        /// <param name="gameObject">要设置的根节点（此节点也会更改）</param>
        /// <param name="layer">新的layer</param>
        public static void SetCullingMaskInThisAndAllChild(this GameObject gameObject, int layer)
        {
            foreach (Camera cam in gameObject.GetComponentsInChildren<Camera>(true))//遍历当前物体及其所有子物体，看现象为深度优先遍历
            {
                cam.cullingMask = 1 << layer;//更改摄像机显示哪层
            }
            foreach (Light light in gameObject.GetComponentsInChildren<Light>(true))//遍历当前物体及其所有子物体，看现象为深度优先遍历
            {
                light.cullingMask = 1 << layer;//更改光照影响哪层
            }
        }

        /// <summary>
        /// 线性插值
        /// Unity自带的插值函数限定了t在0到1区间，使用此函数允许突破范围
        /// </summary>
        /// <param name="from">t=0时的值</param>
        /// <param name="to">t=1时的值</param>
        /// <param name="t">0-1对应from-to</param>
        /// <returns>插值结果</returns>
        public static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * t;
        }
        /// <summary>
        /// 获取Lerp用的T值
        /// 16:9为0，21:9为1，可能会超过0和1
        /// 请尽量使用适配屏幕后的方法：UIFrame.GetFixScreenLerpT()
        /// </summary>
        [Obsolete]
        public static float GetScreenLerpT()
        {
            float height = 0;
            float width = 0;
            if (!Application.isPlaying)
            {
                height = screenSize.y;
                width = screenSize.x;
            }
            else
            {
                height = Screen.height;
                width = Screen.width;
            }
            float hw169 = 16.0f / 9.0f;
            float hw219 = 21.0f / 9.0f;
            float hwScreen = height / width;
            float screenT = (hwScreen - hw169) / (hw219 - hw169);
            return screenT;
        }

        static int mSizeFrame = -1;
        static System.Reflection.MethodInfo s_GetSizeOfMainGameView;
        static Vector2 mGameSize = Vector2.one;
        /// <summary>
        /// Size of the game view cannot be retrieved from Screen.width and Screen.height when the game view is hidden.
        /// </summary>
        static public Vector2 screenSize
        {
            get
            {
                int frame = Time.frameCount;

                if (mSizeFrame != frame || !Application.isPlaying)
                {
                    mSizeFrame = frame;

                    if (s_GetSizeOfMainGameView == null)
                    {
                        System.Type type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                        s_GetSizeOfMainGameView = type.GetMethod("GetSizeOfMainGameView",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    }
                    mGameSize = (Vector2)s_GetSizeOfMainGameView.Invoke(null, null);
                }
                return mGameSize;
            }
        }

        /// <summary>
        /// 将字符串转为整数
        /// </summary>
        public static int ToInt(this string str)
        {
            int value = 0;
            bool isSuccess = int.TryParse(str, out value);
            if (isSuccess == false)
            {
                Debug.LogWarning("Utility , ToInt , isSuccess == false , str = " + str);
            }
            return value;
        }

        /// <summary>
        /// 将字符串转为浮点数
        /// </summary>
        public static float ToFloat(this string str)
        {
            float value = 0f;
            bool isSuccess = float.TryParse(str, out value);
            if (isSuccess == false)
            {
                Debug.LogWarning("Utility , ToFloat , isSuccess == false , str = " + str);
            }
            return value;
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="leftTime">剩余时间，单位为秒</param>
        /// <returns>"12:34:56"格式的时间字符串，用 List 返回</returns>
        public static List<string> FormatLeftTimeWithList(int leftTime)
        {
            if (leftTime < 0) return new List<string>() { "00", "00", "00" };
            int hour = leftTime / 3600;
            string hourStr = Zerofill(hour);
            int min = leftTime / 60 % 60;
            string minStr = Zerofill(min);
            int sec = leftTime % 60;
            string secStr = Zerofill(sec);
            return new List<string>() { hourStr, minStr, secStr };
        }
        public static string Zerofill(int num)
        {
            if (num < 0) return "00";
            if (num < 10) return "0" + num.ToString();
            return num.ToString();
        }
        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="leftTime">剩余时间，单位为秒</param>
        /// <returns>"12:34:56"格式的时间字符串</returns>
        public static string FormatLeftTimeMustHasHour(int leftTime)
        {
            if (leftTime < 0) return "00:00:00";
            int hour = leftTime / 3600;
            string hourStr = Zerofill(hour);
            int min = leftTime / 60 % 60;
            string minStr = Zerofill(min);
            int sec = leftTime % 60;
            string secStr = Zerofill(sec);
            return $"{hourStr}:{minStr}:{secStr}";
        }

        /// <summary>
        /// 将汉字转换为汉字
        /// 如 12345 转 “一万二千三百四十五”
        /// </summary>
        /// <param name="num">要转换的数字，最大支持万亿，不能为负数或小数</param>
        public static string ToChinese(this int num)
        {
            string x = num.ToString();
            //数字转换为中文后的数组
            string[] P_array_num = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            //为数字位数建立一个位数组  
            string[] P_array_digit = new string[] { "", "十", "百", "千" };
            //为数字单位建立一个单位数组  
            string[] P_array_units = new string[] { "", "万", "亿", "万亿" };
            string P_str_returnValue = ""; //返回值  
            int finger = 0; //字符位置指针  
            int P_int_m = x.Length % 4; //取模  
            int P_int_k = 0;
            if (P_int_m > 0)
                P_int_k = x.Length / 4 + 1;
            else
                P_int_k = x.Length / 4;
            //外层循环,四位一组,每组最后加上单位: ",万亿,",",亿,",",万,"  
            for (int i = P_int_k; i > 0; i--)
            {
                int P_int_L = 4;
                if (i == P_int_k && P_int_m != 0)
                    P_int_L = P_int_m;
                //得到一组四位数  
                string four = x.Substring(finger, P_int_L);
                int P_int_l = four.Length;
                //内层循环在该组中的每一位数上循环  
                for (int j = 0; j < P_int_l; j++)
                {
                    //处理组中的每一位数加上所在的位  
                    int n = Convert.ToInt32(four.Substring(j, 1));
                    if (n == 0)
                    {
                        if (j < P_int_l - 1 && Convert.ToInt32(four.Substring(j + 1, 1)) > 0 && !P_str_returnValue.EndsWith(P_array_num[n]))
                            P_str_returnValue += P_array_num[n];
                    }
                    else
                    {
                        if (!(n == 1 && (P_str_returnValue.EndsWith(P_array_num[0]) | P_str_returnValue.Length == 0) && j == P_int_l - 2))
                            P_str_returnValue += P_array_num[n];
                        P_str_returnValue += P_array_digit[P_int_l - j - 1];
                    }
                }
                finger += P_int_L;
                //每组最后加上一个单位:",万,",",亿," 等  
                if (i < P_int_k) //如果不是最高位的一组  
                {
                    if (Convert.ToInt32(four) != 0)
                        //如果所有4位不全是0则加上单位",万,",",亿,"等  
                        P_str_returnValue += P_array_units[i - 1];
                }
                else
                {
                    //处理最高位的一组,最后必须加上单位  
                    P_str_returnValue += P_array_units[i - 1];
                }
            }
            return P_str_returnValue;
        }

        /// <summary>
        /// 比较两个 List 内的元素是否一致
        /// 值类型比较值
        /// 引用类型比较地址
        /// 都为空也是一致
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="listA">第一个 List</param>
        /// <param name="listB">第二个 List</param>
        /// <returns>是否一致</returns>
        public static bool IsListSame<T>(List<T> listA, List<T> listB)
        {
            if (listA == null && listB == null) return true;
            if (listA == null || listB == null) return false;
            if (listA.Count != listB.Count) return false;
            for (int i = 0; i < listA.Count; i++)
            {
                if (Equals(listA[i], listB[i]) == false) return false;
            }
            return true;
        }
        /// <summary>
        /// 比较两个 List 内的元素是否一致
        /// 值类型比较值
        /// 引用类型比较地址
        /// 都为空也是一致
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="listA">第一个 List</param>
        /// <param name="listB">第二个 List</param>
        /// <returns>是否一致</returns>
        public static bool IsSame<T>(this List<T> listA, List<T> listB)
        {
            return IsListSame(listA, listB);
        }

        /// <summary>
        /// 将图片置灰
        /// 使用Materials/UI/GraySprite材质置灰
        /// 使用 null 解除置灰
        /// </summary>
        /// <param name="image">要置灰的图片</param>
        /// <param name="isGray">是否置灰</param>
        public static void SetGray(this Image image, bool isGray)
        {
            UIEffect uiEffect = image.GetComponent<UIEffect>();
            if (uiEffect == null)
            {
                uiEffect = image.gameObject.AddComponent<UIEffect>();
            }
            uiEffect.effectMode = EffectMode.Grayscale;
            uiEffect.effectFactor = isGray ? 1 : 0;
        }

        /// <summary>
        /// 2012/12/12 12:12:12
        /// </summary>
        public static string ToStringUseFormat1(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 20121212121212
        /// </summary>
        public static string ToStringUseFormat2(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmss");
        }

        /// <summary>
        /// 20121212
        /// </summary>
        public static string ToStringUseFormat3(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMdd");
        }

    }
}