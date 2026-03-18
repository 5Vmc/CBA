using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.SDK;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using UnityEngine;
using Utils;

public enum ETimeGiftType
{
    None = 0,

    NormalGift = 1,
    /// <summary>
    /// 关卡礼包
    /// </summary>
    GiftMap = 5001,
    /// <summary>
    /// 球员礼包
    /// </summary>
    GiftCard = 5002,
    /// <summary>
    /// 成长礼包
    /// </summary>
    GiftLevel = 5003
}


public class TimeGiftController : Singleton<TimeGiftController>
{
    public bool HasGift
    {
        get
        {
            if (data == null || data.Count == 0) return false;
            foreach (var list in data.Values)
            {
                if (list.Count > 0) return true;
            }
            return false;
        }
    }
    private Dictionary<int, List<GiftItemData>> data = new Dictionary<int, List<GiftItemData>>();
    public Dictionary<int, List<GiftItemData>> Data
    {
        get
        {
            CheckGiftItemTime();
            return data;
        }
    }
    public bool HasTimeGift
    {
        get
        {
            if (data == null || data.Count == 0) return false;
            if (Data.Count == 0) return false;
            return true;
        }
    }

    public void CheckGiftItemTime()
    {
        if (data == null || data.Count == 0) return;
        foreach (var list in data.Values)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].EndTime < Utils.DataConvUtil.ServerTime)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
        }

        List<int> removeList = new List<int>();
        foreach (var item in data)
        {
            if (item.Value.Count == 0)
            {
                removeList.Add(item.Key);
            }
        }
        foreach (var item in removeList)
        {
            data.Remove(item);
        }
    }

    /// <summary>
    /// 被通知的时候弹窗
    /// </summary>
    /// <param name="_serverid"></param>
    /// <param name="_giftid"></param>
    /// <param name="_activityid"></param>
    private void OnNewGiftItem(int _giftid, int _activityid, int endTime, bool newWindowTag)
    {
        var actCfg = Configs.Activity.GetConfig(_activityid);
        var cfg = Configs.GiftShop.GetConfig(_giftid);
        GiftItemData _newgift;
        if (cfg == null) return;
        if (!data.ContainsKey(actCfg.Id))
        {
            _newgift = new GiftItemData(_giftid, (ETimeGiftType)actCfg.Id, endTime);
            data.Add(actCfg.Id, new() { _newgift });
            OpenNewGiftWin(_newgift, newWindowTag);
        }
        else if (!data[actCfg.Id].Exists(p => p.cfg.Id == _giftid))
        {
            _newgift = new GiftItemData(_giftid, (ETimeGiftType)actCfg.Id, endTime);
            data[actCfg.Id].Add(_newgift);
            OpenNewGiftWin(_newgift, newWindowTag);
        }
        else
        {
            //数据结构里不会变化礼包时间、礼包ID，所以这种情况不用做处理。
        }
    }

    private void OpenNewGiftWin(GiftItemData _newgift, bool newWindowTag = true)
    {
        //Debug.Log("OpenNewGiftWin , !Player.InBattleAni = " + !Player.InBattleAni + "  , !GuideManager.InForceGuide = " + !GuideManager.InForceGuide + " , newWindowTag = " + newWindowTag);
        if (/*!Player.InBattleAni && !GuideManager.InForceGuide &&*/ newWindowTag)
        {
            var properties = new TimeGiftUIProperties(_newgift);
            properties.MagicTargetTrans = Player.TimeGiftTrans;
            UIController.Instance.OpenWindow<TimeGiftUI>(properties, false);
        }
    }

    public void Update(RepeatedField<ActivityPayTriggerInfo> payTriggerList, int newGiftId, bool forceUpdateAll)
    {
        //Debug.Log("newGiftId = " + newGiftId);
        var newWindowTag = true;
        if (forceUpdateAll)
        {
            //启动第一次推才经历这个过程，否则客户端都会弹窗。
            newWindowTag = false;
        }
        else
        {
            data.Clear();
        }

        //装在服务端发来的giftCount
        foreach (var actInfo in payTriggerList)
        {
            foreach (var giftInfo in actInfo.Gifts)
            {
                var newWindow = newWindowTag && newGiftId == giftInfo.GiftId;
                OnNewGiftItem(giftInfo.GiftId, actInfo.ActivityId, giftInfo.EndTime, newWindow);
            }
        }
    }

    /// <summary>
    /// 删除指定的限时礼包
    /// </summary>
    /// <param name="itemid"></param>
    public void RemoveItemByItemId(int itemid)
    {
        foreach (var actInfo in data.Values)
        {
            foreach (var giftInfo in actInfo)
            {
                if (giftInfo.cfg.Id == itemid)
                {
                    actInfo.Remove(giftInfo);
                    return;
                }
            }
        }
    }
}
