using Babu;
using GameConfig;
using GameConfig.Config;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Utils {
    public class Props {
        public string PropName;
        public int PropValue;
    }

    public class CBAUtils
    {
        /// <summary>
        /// 获取属性，例如 1:2， 返回 投篮，10
        /// </summary>
        /// <param name="_value"></param>
        public static Props CreateProp(string _value) {
            Props prop = new Props();
            string[] values = _value.Split(":");
            return CreateProp(int.Parse(values[0]), int.Parse(values[1]));
        }

        public static Props CreateProp(int ability, int value)
        {
            Props prop = new Props();
            prop.PropName = Configs.CardAbility.GetConfig(ability).Name;
            prop.PropValue = value;
            return prop;
        }

        /// <summary>
        /// 获取多个属性，例如 1:2|2:10， 返回 投篮，10
        /// </summary>
        /// <param name="_value"></param>
        /// <returns></returns>
        public static List<Props> CreateProps(Dictionary<int, int> propDict) {
            List<Props> list = new List<Props>();
            foreach (int key in propDict.Keys)
            {
                list.Add(CreateProp(key, propDict[key]));
            }
            return list;
        }

        /// <summary>
        /// 返回带颜色的比较字符串
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetCompareColorStr(int num1, int num2, string separator) {
            if (num1 >= num2) {
                return string.Format("<color={0}>{1}</color>{2}{3}", CBAColorUtil.Instance.GetHexColor(CBAColor.Green), num1, "/", num2);
            } else {
                return string.Format("<color={0}>{1}</color>{2}{3}", CBAColorUtil.Instance.GetHexColor(CBAColor.Red), num1, "/", num2);
            }
        }

        /// <summary>
        /// 动态加载Prefab
        /// </summary>
        /// <param name="path"></param>
        /// <param name="prefab"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public async static Task<GameObject> GetPrefab(string path, string prefab, Transform trans) {
            return await GetPrefab(path + prefab, trans);
        }

        public async static Task<GameObject> GetPrefab(string fullpath, Transform trans)
        {
            //#if UNITY_WEGBL
            var handle = YooAssets.LoadAssetAsync<GameObject>(fullpath);
            await handle.Task;
            return handle.InstantiateSync(trans);
            //#else
            //var handle = YooAssets.LoadAssetSync<GameObject>(fullpath);
            //return handle.InstantiateSync(trans);
            //#endif

        }
    }
}

