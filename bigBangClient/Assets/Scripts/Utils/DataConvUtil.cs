using System;
using System.Collections.Generic;
using Babu;
using Babu.SDK;
using BigBang;
using GameConfig;
using GameConfig.Config;
using UnityEngine;

namespace Utils
{
    public class DataConvUtil
    {
        /// <summary>
        /// 秒级时间戳
        /// 修正后的服务器时间
        /// 尽量使用这个，如无必要尽量避免使用客户端时间
        /// </summary>
        public static long ServerTime
        {
            get
            {
                return (TimeUtils.NowEx() + offsetTimeEx) / 1000;
            }
        }

        /// <summary>
        /// 修正后的服务器时间
        /// 尽量使用这个
        /// </summary>
        public static DateTime ServerDateTime
        {
            get
            {
                DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(Utils.DataConvUtil.ServerTimeEx).DateTime.ToLocalTime();
                return dateTime;
            }
        }

        /// <summary>
        /// 毫秒级时间戳
        /// 修正后的服务器时间
        /// 尽量使用这个，如无必要尽量避免使用客户端时间
        /// </summary>
        public static long ServerTimeEx
        {
            get
            {
                return TimeUtils.NowEx() + offsetTimeEx;
            }
        }

        /// <summary>
        /// 毫秒级时间戳的服务器客户端时间差
        /// 本地时间加上此offset后，可获得修正后的服务器时间
        /// </summary>
        public static long OffsetTimeEx
        {
            get
            {
                return offsetTimeEx;
            }
        }

        /// <summary>
        /// 秒级时间戳的服务器客户端时间差
        /// 本地时间加上此offset后，可获得修正后的服务器时间
        /// </summary>
        public static long OffsetTime
        {
            get
            {
                return offsetTimeEx / 1000;
            }
        }

        private static long offsetTimeEx;
        public static void SetServerTime(long nowServerTimeEx)
        {
            Debug.Log("nowServerTimeEx: " + nowServerTimeEx);
            Debug.Log("TimeUtils.NowEx(): " + TimeUtils.NowEx());
            offsetTimeEx = nowServerTimeEx - TimeUtils.NowEx();
            Debug.Log("offsetTimeEx: " + offsetTimeEx);
        }

        /*
        修正服务器时间显示 返回秒
        serverTime 秒
        **/
        public static int CorrectServerTime(long serverTimeSecs)
        {
            return (int)(serverTimeSecs + (offsetTimeEx / 1000));
        }
        public static void TacticsIdList2AtkDef(List<int> idList, ref string atkName, ref string defName)
        {
            atkName = "";
            defName = "";
            if (idList == null)
            {
                Debug.LogWarningFormat("TacticsIdList2AtkDef idList is null");
                idList = new() { 101, 201 };
            }
            if (idList.Count != 2)
            {
                Debug.LogWarningFormat("TacticsIdList2AtkDef idList.Count != 2 , idList.Count = {0}", idList.Count);
                idList = new() { 101, 201 };
            }
            idList.Sort((a, b) => { return b - a; });
            int atkId = idList[1];
            TacticsConfig atkTacticCfg = Configs.Tactics.GetDataDictionary()[atkId];
            if (atkTacticCfg == null)
            {
                Debug.LogWarningFormat("TacticsIdList2AtkDef atkTacticCfg is null , atkId = {0}", atkId);
                return;
            }
            atkName = atkTacticCfg.Name;
            int defId = idList[0];
            TacticsConfig defTacticCfg = Configs.Tactics.GetDataDictionary()[defId];
            if (defTacticCfg == null)
            {
                Debug.LogWarningFormat("TacticsIdList2AtkDef defTacticCfg is null , defId = {0}", defId);
                return;
            }
            defName = defTacticCfg.Name;
        }

        public static void Tactics2AtkDef(int[] idList, ref string atkName, ref string defName)
        {
            if (idList == null) return;
            Array.Sort(idList, (a, b) => { return b - a; });

            int atkId = idList[1];
            TacticsConfig atkTacticCfg = Configs.Tactics.GetDataDictionary()[atkId];
            atkName = atkTacticCfg.Name;
            int defId = idList[0];
            TacticsConfig defTacticCfg = Configs.Tactics.GetDataDictionary()[defId];
            defName = defTacticCfg.Name;
        }

        public static int GetCombatEffectiveness(Dictionary<int, int> ability, Dictionary<int, int> abilityRatio)
        {
            float sum = 0;
            for (int i = AbilityId.Shoot; i <= AbilityId.Will; i++)
            {
                sum += ability[i] * abilityRatio[i];
            }
            return Mathf.FloorToInt(sum / GameConst.ABILITY_NORMAL + 0.5f);
        }

        public static string FormatTimeLeft(int timeLeft)
        {
            int hour = (int)(timeLeft / 3600);
            int min = (int)((timeLeft - hour * 3600) / 60);
            int sec = (int)(timeLeft - hour * 3600 - min * 60);

            if (hour > 0)
                return string.Format("{0:00}:{1:00}:{2:00}", hour, min, sec);
            else
                return string.Format("{0:00}:{1:00}", min, sec);

        }

        /// <summary>
        /// timeStamp 服务器时间
        /// 服务器发来的消息中通常是算过时区后的时间戳，所以不需要再加上时区
        /// </summary>
        public static string FormatDateTime(long timeStamp, string format = "yy/MM/dd HHmm")
        {
            //timeStamp = CorrectServerTime(timeStamp);
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(timeStamp * TimeSpan.TicksPerSecond);
            DateTime dt = new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
            return dt.ToString(format);
        }

        public static PurchaseInfo NewPurchase(string productId, string productName, double price, int shopItemId)
        {
            PurchaseInfo info = new PurchaseInfo(productId, productName, price,
                    Player.ServerData.Id.ToString(), Player.ServerData.OfficialName, Player.GbId, Player.Name, Player.GbId, shopItemId);
            return info;
        }

    }
}