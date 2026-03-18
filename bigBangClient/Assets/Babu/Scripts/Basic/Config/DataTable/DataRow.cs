using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babu.Config
{
    public class DataRow
    {
        private List<string> _fields;
        private List<string> _values;

        public void SetFields(List<string> fields)
        {
            _fields = fields;
        }

        public void SetValues(List<string> values)
        {
            _values = values;
        }

        public bool GetBool(int index)
        {
            return GetString(index) != "0";
        }

        public byte GetInt8(int index)
        {
            return byte.Parse(GetString(index));
        }

        public short GetInt16(int index)
        {
            return short.Parse(GetString(index));
        }

        public Int32 GetInt32(int index)
        {
            return Int32.Parse(GetString(index));
        }

        public Int64 GetInt64(int index)
        {
            return Int64.Parse(GetString(index));
        }

        public float GetFloat32(int index)
        {
            return float.Parse(GetString(index));
        }

        public Double GetFloat64(int index)
        {
            return Double.Parse(GetString(index));
        }

        public Double GetDouble(int index)
        {
            return Double.Parse(GetString(index));
        }

        public string GetString(int index)
        {
            return _values[index];
        }

        public bool GetBool(string field)
        {
            return GetString(field) != "0";
        }

        public byte GetInt8(string field)
        {
            return byte.Parse(GetString(field));
        }

        public short GetInt16(string field)
        {
            return short.Parse(GetString(field));
        }

        public Int32 GetInt32(string field)
        {
            return Int32.Parse(GetString(field));
        }

        public Int64 GetInt64(string field)
        {
            return Int64.Parse(GetString(field));
        }

        public float GetFloat32(string field)
        {
            return float.Parse(GetString(field));
        }

        public Double GetFloat64(string field)
        {
            return Double.Parse(GetString(field));
        }

        public Int32[] GetInt32Array(string field)
        {
            string[] ss = GetStringArray(field);
            Int32[] result = new Int32[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToInt32(ss[i]);
            }

            return result;
        }

        public Int32[] GetInt32Array(int index, string delims = "|")
        {
            string[] ss = GetStringArray(index, delims);
            Int32[] result = new Int32[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToInt32(ss[i]);
            }

            return result;
        }

        public short[] GetInt16Array(string field)
        {
            string[] ss = GetStringArray(field);
            short[] result = new short[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToInt16(ss[i]);
            }

            return result;
        }

        public byte[] GetInt8Array(string field)
        {
            string[] ss = GetStringArray(field);
            byte[] result = new byte[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToByte(ss[i]);
            }

            return result;
        }

        public float[] GetFloat32Array(int index, string delims = "|")
        {
            string[] ss = GetStringArray(index, delims);
            float[] result = new float[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToSingle(ss[i]);
            }

            return result;
        }

        public float[] GetFloat32Array(string field)
        {
            string[] ss = GetStringArray(field);
            float[] result = new float[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToSingle(ss[i]);
            }

            return result;
        }

        public string GetString(string field)
        {
            int index = _fields.IndexOf(field);
            if (index == -1)
            {
                Debug.Log("can not find filed: " + field);
                return "";
            }

            //替换换行符
            var value = _values[index].Replace(@"\n", "\n");
            value = value.Replace(@"<br>", "\n");

            return value;
        }

        public string GetKeyString(string field)
        {
            int index = _fields.IndexOf(field);
            if (index == -1)
            {
                Debug.Log("can not find filed: " + field);
                return "";
            }

            //替换换行符
            var value = _values[index];

            return value;
        }

        public string[] GetStringArray(string field, string delims = "|")
        {
            return GetString(field).Split(new string[] {delims}, StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] GetStringArray(int index, string delims = "|")
        {
            return GetString(index).Split(new string[] {delims}, StringSplitOptions.RemoveEmptyEntries);
        }

        public Dictionary<int, string> GetIntStringDic(string field)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            string[] strings = GetStringArray(field);
            foreach (var s in strings)
            {
                string[] arr = s.Split(':');
                int key = int.Parse(arr[0]);
                string value = arr[1];
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, value);
                }
                else
                {
                    //todo log
                }
            }

            return dic;
        }

        public Dictionary<int, int> GetIntIntDic(string field)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            string[] strings = GetStringArray(field);
            foreach (var s in strings)
            {
                string[] arr = s.Split(':');
                int key = int.Parse(arr[0]);
                int value = int.Parse(arr[1]);
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, value);
                }
                else
                {
                    //todo log
                }
            }

            return dic;
        }
        public Dictionary<int, float> GetIntFloatDic(string field)
        {
            Dictionary<int, float> dic = new Dictionary<int, float>();
            string[] strings = GetStringArray(field);
            foreach (var s in strings)
            {
                string[] arr = s.Split(':');
                int key = int.Parse(arr[0]);
                float value = float.Parse(arr[1]);
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, value);
                }
                else
                {
                    //todo log
                }
            }

            return dic;
        }
        public Dictionary<string, int> GetStringIntDic(string field)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            string[] strings = GetStringArray(field);
            foreach (var s in strings)
            {
                string[] arr = s.Split(':');
                string key = arr[0];
                int value = int.Parse(arr[1]);
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, value);
                }
                else
                {
                    //todo log
                }
            }

            return dic;
        }

        public T GetEnum<T>(string field)
        {
            return (T)Enum.Parse(typeof(T), GetString(field));
        }

        public BigNumber.BigNumber GetBigNumber(string field)
        {
            string[] strings = GetStringArray(field, ":");
            if (strings.Length == 1)
            {
                return new BigNumber.BigNumber(float.Parse(strings[0]));
            }
            else
            {
                return new BigNumber.BigNumber(float.Parse(strings[0]), int.Parse(strings[1]));
            }
        }
    }
}