using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.Animation;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using Utils;
using deVoid.UIFramework;
using System;

public class HonourPad : MonoBehaviour
{
    public enum SubUIID
    {
        Badge = 0,
        Cup = 1,
        Souvenir = 2,
    }

    [SerializeField] private BabuToggleGroup bottomToggleGroup;
    [SerializeField] private HonourAdapter adapter;
    [SerializeField] public Image badgeRedDot;
    [SerializeField] public Image cupRedDot;
    [SerializeField] public HonourPadAnim Anim;

    private void OnEnable()
    {
        bottomToggleGroup.OnValueChanged += OnToggleChanged;
        EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);

        RefreshRedDot();
    }
    private void OnDisable()
    {
        bottomToggleGroup.OnValueChanged -= OnToggleChanged;
        EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
    }

    private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
    {
        int selectedIndex = bottomToggleGroup.EnableIndex;
        ShowPad((SubUIID)selectedIndex);
    }
    private void ShowPad(SubUIID padIndex)
    {
        switch (padIndex)
        {
            case SubUIID.Badge: SetAdapter(11); ; break;
            case SubUIID.Cup: SetAdapter(12); ; break;
            case SubUIID.Souvenir: SetAdapter(13); ; break;
        }
    }
    private void SetAdapter(int achievementType)
    {
        List<HonourLineItemData> honourLineItemDataList = new List<HonourLineItemData>();

        {
            HonourLineItemData honourLineItemData = new HonourLineItemData();
            honourLineItemData.type = HonourLineItemData.HonourLineType.Top;
            honourLineItemDataList.Add(honourLineItemData);
        }

        {
            int midLineMinCount = 3;
            int midLineCount = 0;

            List<AchievementGroupData> achievementGroupDataList = Player.AchievementManager.AchGroupData[achievementType];
            Dictionary<int, HonourGroupData> achievementGroupDic = new();
            foreach (AchievementGroupData achievementGroupData in achievementGroupDataList)
            {
                foreach (AchievementData achievementData in achievementGroupData.list)
                {
                    HonourGroupData honourGroupData = null;
                    if (achievementGroupDic.ContainsKey(achievementData.Config.ClientGroup) == false)
                    {
                        honourGroupData = new HonourGroupData();
                        honourGroupData.clientGroup = achievementData.Config.ClientGroup;
                        achievementGroupDic.Add(achievementData.Config.ClientGroup, honourGroupData);
                    }
                    else
                    {
                        honourGroupData = achievementGroupDic[achievementData.Config.ClientGroup];
                    }
                    honourGroupData.list.Add(achievementData);
                }
            }
            List<HonourGroupData> honourGroupDataList = achievementGroupDic.Values.ToList();
            honourGroupDataList = honourGroupDataList.OrderBy(item => item.clientGroup).ToList();

            for (int i = 0; i < honourGroupDataList.Count; i += 2)
            {
                HonourLineItemData honourLineItemData = new HonourLineItemData();
                honourLineItemData.type = HonourLineItemData.HonourLineType.Mid;
                HonourGroupData honourGroupData = honourGroupDataList[i];
                honourLineItemData.honourGroupDataList.Add(honourGroupData);
                if (i + 1 < honourGroupDataList.Count)
                {
                    honourGroupData = honourGroupDataList[i + 1];
                    honourLineItemData.honourGroupDataList.Add(honourGroupData);
                }
                honourLineItemDataList.Add(honourLineItemData);
                midLineCount++;
            }
            for (int i = midLineCount; i < midLineMinCount; i++)
            {
                HonourLineItemData honourLineItemData = new HonourLineItemData();
                honourLineItemData.type = HonourLineItemData.HonourLineType.Mid;
                honourLineItemDataList.Add(honourLineItemData);
            }
        }

        {
            HonourLineItemData honourLineItemData = new HonourLineItemData();
            honourLineItemData.type = HonourLineItemData.HonourLineType.Bottom;
            honourLineItemDataList.Add(honourLineItemData);
        }

        adapter.Parameters.ContentPadding = new RectOffset(0, 0, 200, Mathf.RoundToInt(Utility.Lerp(-130, 0, UIFrame.GetFixScreenLerpT())));
        adapter.SetData(honourLineItemDataList);
        adapter.SetNormalizedPosition(1);
    }


    public void RefreshRedDot(object[] args = null)
    {
        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Honour, "/Badge");
        node.IsRed(badgeRedDot.transform);
        node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Honour, "/Cup");
        node.IsRed(cupRedDot.transform);
    }

    public void OnShow()
    {
        bottomToggleGroup.Switch(0);
        Anim.PlayTopBarAnim();
        CheckNewGet();
    }
    public Queue<AchievementData> canGetAchievementDataList = new();
    public bool isFirstHonourGet = true;
    private void CheckNewGet()
    {
        canGetAchievementDataList.Clear();
        List<AchievementGroupData> achievementGroupDataList11 = Player.AchievementManager.AchGroupData[11];
        List<AchievementGroupData> achievementGroupDataList12 = Player.AchievementManager.AchGroupData[12];
        List<AchievementGroupData> achievementGroupDataList13 = Player.AchievementManager.AchGroupData[13];
        List<AchievementGroupData> achievementGroupDataList = new List<AchievementGroupData>();
        achievementGroupDataList.AddRange(achievementGroupDataList11);
        achievementGroupDataList.AddRange(achievementGroupDataList12);
        achievementGroupDataList.AddRange(achievementGroupDataList13);
        foreach (AchievementGroupData achievementGroupData in achievementGroupDataList)
        {
            foreach (AchievementData achievementData in achievementGroupData.list)
            {
                if (achievementData.Received == 1) continue;
                if (!achievementData.IsComplete) continue;
                canGetAchievementDataList.Enqueue(achievementData);
            }
        }
        canGetAchievementDataList = new Queue<AchievementData>(canGetAchievementDataList.OrderBy((AchievementData achievementData) =>
        {
            return achievementData.time;
        }));
        isFirstHonourGet = true;
        if (canGetAchievementDataList.Count > 0)
        {
            ShowNextGet();
        }
    }
    private void ShowNextGet()
    {
        if (canGetAchievementDataList.Count == 0) return;
        AchievementData achievementData = canGetAchievementDataList.Dequeue();
        UIController.Instance.OpenWindow<HonourGetUI>(new HonourGetUIProperties(achievementData, isFirstHonourGet, ShowNextGet));
        isFirstHonourGet = false;
    }

}
