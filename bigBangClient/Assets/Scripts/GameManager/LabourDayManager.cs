using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace BigBang
{
    /// <summary>
    /// 全明星的管理类
    /// </summary>
    public class LabourDayManager : BabuSingleton<LabourDayManager>
    {

        // private bool isInited = false;
        // public void InitOnce(bool forceInit = true)
        // {

        //     if (isInited && !forceInit) return;
        //     isInited = true;

        // }

        public int serverOrder = 0;
        public int mapIndex = 0;
        public int mapInnerIndex = 0;
        public void Unpack(SignActivityModuleNotify signActivityModuleNotify)
        {
            serverOrder = signActivityModuleNotify.TravelOrder;
            // serverOrder = 145;
            ResetDataByServerOrder();
        }

        private void ResetDataByServerOrder()
        {
            serverOrder = Utility.KeepInRange(serverOrder, 0, 150);
            mapIndex = serverOrder / 30;
            mapInnerIndex = serverOrder % 30;
            if (serverOrder == 150)
            {
                mapIndex = 4;
                mapInnerIndex = 30;
            }
        }

        public List<int> diceNumList = new List<int>();
        public void RollDice(int count, Action callback)
        {
            int oldServerOrder = serverOrder;
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.LabourDayHome);
            NetworkManager.Instance.ThrowTravelDice(activityData.cfg.Id, count, (ThrowTravelDiceResponse throwTravelDiceResponse) =>
            {
                diceNumList = throwTravelDiceResponse.NumList.ToList();
                int allCount = diceNumList.Sum();
                serverOrder += allCount;
                ResetDataByServerOrder();
                callback?.Invoke();
            });

            // diceNumList.Clear();
            // for (int i = 0; i < count; i++)
            // {
            //     diceNumList.Add(Utility.GetRandomInt(1, 6));
            // }
            // serverOrder += diceNumList.Sum();
            // ResetDataByServerOrder();
            // callback?.Invoke();
        }

        public bool IsGetAllReward
        {
            get
            {
                return serverOrder >= 150;
            }
        }
    }
}