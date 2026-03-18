using System;
using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

public class RedEnvlopeNoticeItem : MonoBehaviour
{
    [SerializeField] private RectTransform redEnvlopeNoticeItem = null;
    [SerializeField] private RectTransform noticeBarNormal = null;
    [SerializeField] private HorizontalLayoutGroup normalNoticeLayout = null;
    [SerializeField] private TMP_Text normalContentText = null;
    [SerializeField] private RectTransform noticeBarFirst = null;
    [SerializeField] private HorizontalLayoutGroup firstNoticeLayout = null;
    [SerializeField] private TMP_Text firstContentText = null;

    bool isPlaying = true;
    UnityTimer.Timer playTimer = null;
    private void OnEnable()
    {
        isPlaying = true;
        sequence?.Kill();
        sequence = null;
        redEnvlopeNoticeItem.SetLocalScaleY(0);

        playTimer = UnityTimer.Timer.Register(this.gameObject, 0.5f, PlayNext);

        EventManager.Instance.Register(EventID.OnAfterReceiveRedEnvlopeNotify, OnAfterReceiveRedEnvlopeNotify);
    }

    private void OnDisable()
    {
        sequence?.Kill();
        sequence = null;
        redEnvlopeNoticeItem.SetLocalScaleY(0);
        playTimer?.Cancel();
        playTimer = null;

        EventManager.Instance.Unregister(EventID.OnAfterReceiveRedEnvlopeNotify, OnAfterReceiveRedEnvlopeNotify);
    }

    private void OnAfterReceiveRedEnvlopeNotify(object[] args)
    {
        if (!isPlaying) PlayNext();
    }

    private readonly List<string> firstRankStrList = new()
    {
        "{0}祝福大家：事业腾飞，家庭和睦，身体健康，万事如意！",
        "{0}祝福大家：财源广进，福气满满，好运连连，幸福安康！",
        "{0}祝福大家：笑口常开，心想事成，步步高升，梦想成真！",
        "{0}祝福大家：事业有成，家庭美满，幸福安康，福禄寿喜！",
        "{0}祝福大家：鸿运当头，喜气洋洋，吉祥如意，万事胜意！",
    };
    private readonly List<string> otherRankStrList = new()
    {
        "{0}祝福大家：在新的一年里，快乐每一天！",
        "{0}祝福大家：在新的一年里，幸福每分每秒！",
        "{0}祝福大家：在新的一年里，顺心时时刻刻！",
        "{0}祝福大家：在新的一年里，幸运年年岁岁！",
        "{0}祝福大家：在新的一年里，每时每刻都开心快乐！",
    };
    private readonly List<string> itemStrList = new()
    {
        "天降鸿福！{0}不经意之间拆开了红包，意外获得【{1}*{2}】！大家快来沾沾福气！",
        "{0}打开红包后竟然获得【{1}*{2}】，真是让人羡慕不已！",
        "{0}随手拆了一个红包，竟然获得了【{1}*{2}】，真是羡煞旁人！",
        "{0}竟然获得了【{1}*{2}】，真是羡煞旁人！",
        "{0}红包轻轻一拆，【{1}*{2}】赫然在目，真是“鸿运当头”！",
    };
    private readonly string nameColorStr = "<color=#FCFF0E>{0}区-{1}</color>";

    private Sequence sequence = null;
    private void PlayNext()
    {
        sequence?.Kill();
        sequence = null;
        redEnvlopeNoticeItem.SetLocalScaleY(0);

        MarqueeInfo marqueeInfo = RedEnvlopeManager.Instance.GetMarqueeInfo();
        if (marqueeInfo == null)
        {
            isPlaying = false;
            return;
        }

        sequence = DOTween.Sequence();
        sequence.SetTarget(redEnvlopeNoticeItem);
        sequence.AddTo(this.gameObject);

        bool isRank = marqueeInfo.Type == 1;
        bool isNormal = (isRank && marqueeInfo.Rank > 1) || !isRank;

        noticeBarNormal.gameObject.SetActive(isNormal);
        noticeBarFirst.gameObject.SetActive(!isNormal);
        SetContent(marqueeInfo);

        HorizontalLayoutGroup layout = isNormal ? normalNoticeLayout : firstNoticeLayout;
        RectTransform layoutTrans = layout.transform as RectTransform;
        layoutTrans.SetAnchoredPositionX(300 + layoutTrans.sizeDelta.x / 2);

        sequence.Append(redEnvlopeNoticeItem.DOScaleY(1, 1).SetEase(Ease.OutBack));
        sequence.Append(layoutTrans.DOAnchorPosX(-300 - layoutTrans.sizeDelta.x / 2, 18f).SetEase(Ease.Linear));
        sequence.Append(redEnvlopeNoticeItem.DOScaleY(0, 1).SetEase(Ease.InBack));
        sequence.AppendCallback(PlayNext);
    }

    private void SetContent(MarqueeInfo marqueeInfo)
    {
        bool isRank = marqueeInfo.Type == 1;
        bool isNormal = (isRank && marqueeInfo.Rank > 1) || !isRank;
        string nameStr = nameColorStr.SafeFormat(marqueeInfo.ServerId, marqueeInfo.Name);
        if (isNormal)
        {
            if (isRank)
            {
                normalContentText.text = otherRankStrList[Utility.GetRandomInt(0, otherRankStrList.Count - 1)].SafeFormat(nameStr);
            }
            else
            {
                GameItem gameItem = GameItemUtils.UnPack(marqueeInfo.Item);
                normalContentText.text = itemStrList[Utility.GetRandomInt(0, itemStrList.Count - 1)].SafeFormat(nameStr, gameItem.GetName(), gameItem.CountString());
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(normalContentText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(normalNoticeLayout.transform as RectTransform);
        }
        else
        {
            firstContentText.text = firstRankStrList[Utility.GetRandomInt(0, firstRankStrList.Count - 1)].SafeFormat(nameStr);
            LayoutRebuilder.ForceRebuildLayoutImmediate(firstContentText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(firstNoticeLayout.transform as RectTransform);
        }
    }


}
