using Babu;
using BigBang.Animation;
using CBA;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityTimer;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class HundredHomeUISignPad : MonoBehaviour
    {

        [SerializeField] private BabuButton helpBtn = null;
        [SerializeField] private ImageFont titleImageFont = null;
        [SerializeField] private RectTransform nextTipPanel = null;
        [SerializeField] private RectTransform notEnterTipPanel = null;
        [SerializeField] private TMP_Text signTopTipText = null;
        [SerializeField] private List<HundredHomeUISignItem> hundredHomeUISignItemList = null;
        [SerializeField] private BabuButton formationButton = null;
        [SerializeField] private BabuButton signButton = null;
        [SerializeField] private TMP_Text signTimeTipText = null;
        [SerializeField] private TMP_Text signTimeHourText = null;
        [SerializeField] private TMP_Text signTimeMinuteText = null;
        [SerializeField] private TMP_Text signTimeSecondText = null;
        [SerializeField] private ScrollRect scrollView = null;
        [SerializeField] private LeftTimeComponent leftTimeComponent = null;
        [SerializeField] private TMP_Text dayTimeTipText = null;

        [SerializeField] private BabuButton hundredHomeUISignItemNew = null;

        private void Awake()
        {
            for (int i = 0; i < hundredHomeUISignItemList.Count; i++)
            {
                HundredHomeUISignItem hundredHomeUISignItem = hundredHomeUISignItemList[i];
                hundredHomeUISignItem.index = i;
                hundredHomeUISignItem.ClickCallback = OnClickSignItem;
            }
        }
        protected void OnEnable()
        {
            helpBtn.OnClick += OnClickHelpButton;
            formationButton.OnClick += OnClickFormationButton;
            signButton.OnClick += OnClickSignButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        }
        protected void OnDisable()
        {
            helpBtn.OnClick -= OnClickHelpButton;
            formationButton.OnClick -= OnClickFormationButton;
            signButton.OnClick -= OnClickSignButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
        }

        GetHundredCourseResponse serverData = null;
        public void OnShow(bool refreshData = false)
        {
            HundredManager.Instance.GetCourse(HundredManager.Instance.dropdownValue + 1, refreshData, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                this.serverData = getHundredCourseResponse;
                CheckStage();
                Debug.Log("OnShow , serverData.MyZoneId = " + serverData.MyZoneId);
                HundredManager.Instance.SetTitle(titleImageFont, serverData);
                RefreshType();
                Refresh();
            });
        }
        public void CheckStage()
        {
            switch ((HundredProgress)serverData.Stage)
            {
                case HundredProgress.Wait:
                case HundredProgress.Sign:
                case HundredProgress.Fight1:
                    break;
                default:
                    EventManager.Instance.Dispatch(EventID.OnHundredStageMismatch);
                    break;
            }
        }

        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredHelpUI>();
        }

        private void RefreshLeftTimeOneSec()
        {
            if (leftTime > 0)
            {
                leftTime--;
                if (leftTime == 0)
                {
                    signTimeTipText.text = "当前阶段结束";
                }
                RefreshTimeText();
            }
        }

        private int leftTime = 0;
        private void SetLeftTime(int leftTime)
        {
            this.leftTime = leftTime;
            RefreshTimeText();
        }
        private void RefreshTimeText()
        {
            int time = leftTime;
            int daySec = 24 * 60 * 60;
            dayTimeTipText.gameObject.SetActive(time > daySec);
            if (leftTime >= daySec)
            {
                dayTimeTipText.text = "{0}天".SafeFormat(time / daySec);
                time -= (time / daySec) * daySec;
            }
            leftTimeComponent.SetLeftTimeText(time);
        }

        private void OnClickFormationButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredFormationUI>(new HundredFormationUIProperties(HundredProgress.Sign, HundredFormationUI.HFType.Open, -1));
        }
        private void OnClickSignButton(BabuButton _)
        {
            string tipStr = "";
            if (HundredManager.Instance.IsNewStar)
            {
                if (selectIndex + 1 == 9)
                {
                    tipStr = "新星赛区集结了许多新星球队，该赛区第一名不进入总决赛，报名后赛区将无法更改，确定报名<color=#9D2A2D>新星赛区</color>？";
                }
                else
                {
                    tipStr = "此赛区高手云集，建议您前往新星赛区进行报名，报名后赛区将无法更改，确定报名<color=#9D2A2D>第{0}赛区</color>？".SafeFormat((selectIndex + 1).ToChinese());
                }
            }
            else
            {
                tipStr = "报名后赛区将无法更改，确定报名<color=#9D2A2D>第{0}赛区</color>？".SafeFormat((selectIndex + 1).ToChinese());
            }
            UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties(tipStr, () =>
            {
                Debug.Log("OnClickSignButton , selectIndex + 1 " + (selectIndex + 1));
                NetworkManager.Instance.SignUpHundred(selectIndex + 1, (Protocol.SignUpHundredResponse signUpHundredResponse) =>
                {
                    if (signUpHundredResponse.SignUpSucceed == true)
                    {
                        HundredManager.Instance.MyZoneId = selectIndex + 1;
                        AfterSign();
                    }
                    else
                    {
                        Tips.PopError("报名失败");
                        OnShow(true);
                    }
                });
            }));
        }
        private void AfterSign()
        {
            UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties("报名成功，您可以通过【布阵】调整球员上场顺序", "确定", null));
            OnShow(true);
            HundredManager.Instance.CheckHundredRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        /// <summary> 报名界面的 4 个状态 </summary>
        private enum SignType
        {
            /// <summary> 去报名 </summary>
            needSign = 0,
            /// <summary> 报名成功 </summary>
            Signed = 1,
            /// <summary> 未参赛 </summary>
            PassSign = 2,
            /// <summary> 休赛期 </summary>
            Wait = 3,
        }

        private int selectIndex = 0;//所在赛区
        SignType signType = SignType.needSign;
        private void RefreshType()
        {
            switch ((HundredProgress)serverData.Stage)
            {
                case HundredProgress.Sign:
                    {
                        if (serverData.MyZoneId == 0)
                        {
                            signType = SignType.needSign;
                        }
                        else
                        {
                            signType = SignType.Signed;
                        }
                    }
                    break;
                case HundredProgress.Wait:
                    {
                        signType = SignType.Wait;
                    }
                    break;
                default:
                    {
                        signType = SignType.PassSign;
                    }
                    break;
            }
        }

        private void Refresh()
        {
            hundredHomeUISignItemNew.gameObject.SetActive(HundredManager.Instance.IsNewStar);
            ResetAllItem();
            switch (signType)
            {
                case SignType.needSign:
                    {
                        nextTipPanel.gameObject.SetActive(false);
                        notEnterTipPanel.gameObject.SetActive(false);
                        signTopTipText.text = "请选择一个区域报名";
                        signTimeTipText.text = "报名截至倒计时";
                        signButton.gameObject.SetActive(true);
                        formationButton.gameObject.SetActive(false);
                        selectIndex = GetBestAreaIndex();
                        SetHighlightItem(selectIndex);
                        if (selectIndex > 3)
                            scrollView.ScrollToBottom(0);
                        else
                            scrollView.ScroolToTop(0);
                        SetLeftTime(serverData.StageEndTime - (int)Utils.DataConvUtil.ServerTime);
                        SetAreaNumberOfPeople();
                    }
                    break;
                case SignType.Signed:
                    {
                        nextTipPanel.gameObject.SetActive(false);
                        notEnterTipPanel.gameObject.SetActive(false);
                        signTopTipText.text = "等待入围赛开启";
                        signTimeTipText.text = "入围赛开始倒计时";
                        signButton.gameObject.SetActive(false);
                        formationButton.gameObject.SetActive(true);
                        selectIndex = serverData.MyZoneId - 1;
                        SetSignItem(selectIndex);
                        if (selectIndex > 3)
                            scrollView.ScrollToBottom(0);
                        else
                            scrollView.ScroolToTop(0);
                        SetLeftTime(serverData.StageEndTime - (int)Utils.DataConvUtil.ServerTime);
                        SetAreaNumberOfPeople();
                    }
                    break;
                case SignType.PassSign:
                    {
                        nextTipPanel.gameObject.SetActive(false);
                        notEnterTipPanel.gameObject.SetActive(true);
                        signTopTipText.text = "入围赛进行中";
                        signTimeTipText.text = "入围赛结束倒计时";
                        signButton.gameObject.SetActive(false);
                        formationButton.gameObject.SetActive(false);
                        SetAllDarkItem();
                        scrollView.ScroolToTop(0);
                        SetLeftTime(serverData.StageEndTime - (int)Utils.DataConvUtil.ServerTime);
                        SetAreaNumberOfPeople();
                    }
                    break;
                case SignType.Wait:
                    {
                        nextTipPanel.gameObject.SetActive(true);
                        notEnterTipPanel.gameObject.SetActive(false);
                        signTopTipText.text = "休赛期";
                        signTimeTipText.text = "报名开始倒计时";
                        signButton.gameObject.SetActive(false);
                        formationButton.gameObject.SetActive(false);
                        SetAllDarkItem();
                        scrollView.ScroolToTop(0);
                        SetLeftTime(serverData.StageEndTime - (int)Utils.DataConvUtil.ServerTime);
                        SetAreaNumberOfPeopleZero();
                    }
                    break;
            }
        }
        private int GetBestAreaIndex()//默认选择一个人数最少的区
        {
            int minCountZoneIndex = 0;
            int minCount = int.MaxValue;
            for (int i = 0; i < serverData.ZoneSignTeamCount.Count; i++)
            {
                if (serverData.ZoneSignTeamCount[i] < minCount)
                {
                    minCount = serverData.ZoneSignTeamCount[i];
                    minCountZoneIndex = i;
                }
            }
            return minCountZoneIndex;
        }

        private void ResetAllItem()
        {
            for (int i = 0; i < hundredHomeUISignItemList.Count; i++)
            {
                HundredHomeUISignItem hundredHomeUISignItem = hundredHomeUISignItemList[i];
                hundredHomeUISignItem.lightImage.gameObject.SetActive(false);
                hundredHomeUISignItem.darkImage.gameObject.SetActive(false);
                hundredHomeUISignItem.signSuccessImage.gameObject.SetActive(false);
                hundredHomeUISignItem.countText.text = 0.ToString();
                LayoutRebuilder.ForceRebuildLayoutImmediate(hundredHomeUISignItem.countText.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(hundredHomeUISignItem.countText.transform.parent as RectTransform);
            }
        }
        private void SetHighlightItem(int index)
        {
            selectIndex = index;
            for (int i = 0; i < hundredHomeUISignItemList.Count; i++)
            {
                HundredHomeUISignItem hundredHomeUISignItem = hundredHomeUISignItemList[i];
                hundredHomeUISignItem.lightImage.gameObject.SetActive(i == index);
            }
        }
        private void SetSignItem(int index)
        {
            for (int i = 0; i < hundredHomeUISignItemList.Count; i++)
            {
                HundredHomeUISignItem hundredHomeUISignItem = hundredHomeUISignItemList[i];
                hundredHomeUISignItem.darkImage.gameObject.SetActive(true);
                hundredHomeUISignItem.signSuccessImage.gameObject.SetActive(i == index);
            }
        }
        private void SetAllDarkItem()
        {
            for (int i = 0; i < hundredHomeUISignItemList.Count; i++)
            {
                HundredHomeUISignItem hundredHomeUISignItem = hundredHomeUISignItemList[i];
                hundredHomeUISignItem.darkImage.gameObject.SetActive(true);
            }
        }
        private void OnClickSignItem(int clickIndex)
        {
            switch (signType)
            {
                case SignType.needSign:
                    {
                        SetHighlightItem(clickIndex);
                    }
                    break;
                case SignType.Signed:
                    {
                        if(selectIndex + 1 == 9)
                        {
                            Tips.PopTips("您已报名新星赛区，请等待比赛开启");
                        }
                        else
                        {
                            Tips.PopTips("您已报名第{0}赛区，请等待比赛开启".SafeFormat((selectIndex + 1).ToChinese()));
                        }
                    }
                    break;
                case SignType.PassSign:
                    {
                        Tips.PopTips("您未参加本届比赛");
                    }
                    break;
                case SignType.Wait:
                    {
                        Tips.PopTips("当前处于休赛期");
                    }
                    break;
            }
        }
        private void SetAreaNumberOfPeopleZero()
        {
            for (int i = 0; i < hundredHomeUISignItemList.Count; i++)
            {
                HundredHomeUISignItem hundredHomeUISignItem = hundredHomeUISignItemList[i];
                hundredHomeUISignItem.countText.text = 0.ToString();
                LayoutRebuilder.ForceRebuildLayoutImmediate(hundredHomeUISignItem.countText.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(hundredHomeUISignItem.countText.transform.parent as RectTransform);
            }
        }
        private void SetAreaNumberOfPeople()//设置每个区域多少人
        {
            for (int i = 0; i < hundredHomeUISignItemList.Count; i++)
            {
                HundredHomeUISignItem hundredHomeUISignItem = hundredHomeUISignItemList[i];
                hundredHomeUISignItem.countText.text = serverData.ZoneSignTeamCount[i].ToString();
                LayoutRebuilder.ForceRebuildLayoutImmediate(hundredHomeUISignItem.countText.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(hundredHomeUISignItem.countText.transform.parent as RectTransform);
            }
        }
    }
}