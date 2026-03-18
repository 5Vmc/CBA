using Babu;
using Babu.BigNumber;
using Babu.Client.Fsm;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    [System.Serializable]
    public class TrainUIPanelProperties : PanelProperties
    {
        public TrainUI.SubUIID FirstUI;

        public TrainUIPanelProperties(TrainUI.SubUIID uiid)
        {
            FirstUI = uiid;
        }
    }

    public class TrainUI : APanelController<TrainUIPanelProperties>
    {

        [SerializeField] private TrainUIComponent com;
        [SerializeField] private List<Transform> RedDotList;

        public static RectTransform SpeedDiamond;
        public static TMP_Text ExpText;

        public enum SubUIID
        {
            Regular,
            Strength,
            BigBang,
            Invite
        }

        protected override void Awake()
        {
            base.Awake();
        }

        private bool isFirst = true;
        private enum TrainUIPad
        {
            None,
            Regular,
            Strength,
            BigBang,
            Invite
        }

        private TrainUIPad currentPad;

        protected override void AddListeners()
        {
            com.toggleGroup.OnValueChanged += OnToggleValueChanged;
            com.SpeedBtn.onClick.AddListener(OnSpeed);
            EventManager.Instance.Register(EventID.OnExpChanged, OnExpChanged);
            EventManager.Instance.Register(EventID.OnStrenthChanged, OnStrenthChanged);
            EventManager.Instance.Register(EventID.RefreshBigBangUIRedDot, RefreshUIRedDot);
        }

        private void RefreshUIRedDot(object[] args = null)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/Regular");
            node?.IsRed(RedDotList[0].transform);
            node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/Strength");
            node?.IsRed(RedDotList[1].transform);
            node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/BigBang");
            node?.IsRed(RedDotList[2].transform);
            node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/InviteMatch");
            node?.IsRed(RedDotList[3].transform);
        }

        protected override void RemoveListeners()
        {
            com.toggleGroup.OnValueChanged -= OnToggleValueChanged;
            com.SpeedBtn.onClick.RemoveListener(OnSpeed);
            EventManager.Instance.Unregister(EventID.OnExpChanged, OnExpChanged);
            EventManager.Instance.Unregister(EventID.OnStrenthChanged, OnStrenthChanged);
            EventManager.Instance.Unregister(EventID.RefreshBigBangUIRedDot, RefreshUIRedDot);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            // 设置球员衣服
            PlayerJersey.SetAllJersey();
            isFirst = true;
            // 显示离线收益
            Player.TrainManager.ShowOfflineExp();
            //设置数据
            UpdateExpShow();
            //设置倍率指针方向和图片
            SetSpeedTransform();
            SpeedDiamond = com.SpeedDiamond;
            ExpText = com.ExpText;
            BigBreakthroughUIAnim.TargetPosition = RectTransformUtility.WorldToScreenPoint(UIController.Instance.GetCamera(), com.SpeedDiamond.position);
            //TrainUI动画初始化
            com.TrainAnim.Init();

            if (Properties.FirstUI == SubUIID.Regular)
            {
                com.TrainAnim.ShowSpeedBtn();
            }
            EventManager.Instance.Dispatch(EventID.OnExpChanged);
            AudioManager.Instance.PlayMusic(AudioNames.BGM_TRAINING);

            com.TrainAnim.PlayEnter();

            if (com.toggleGroup.EnableToggle == null) com.toggleGroup.EnableToggle = com.toggleGroup.GetComponentInChildren<BabuToggle>();
            com.toggleGroup.Switch((int)Properties.FirstUI);

            RefreshUIRedDot();
        }

        protected override void WhileHiding()
        {
            if (com.padState.ContainsKey(2))
            {
                com.padState[2].GetComponent<BigBangPad>().Stop();
            }
        }

        private void OnExpChanged(object[] args)
        {
            UpdateExpShow();
            Player.TrainManager.CheckRedDot();
            RefreshUIRedDot();
        }

        public void OnStrenthChanged(object[] args)
        {
            RefreshUIRedDot();
        }

        private BigNumber lastValue = null;

        private void UpdateExpShow()
        {
            if (lastValue == null)
            {
                lastValue = Player.TrainManager.Exp.Clone();
            }
            com.SpeedText.text = Player.TrainManager.GetInComeShowString();
            BigNumber targetValue = Player.TrainManager.Exp.Clone();
            //DOTween.To(value => com.ExpText.text = (lastValue + value * (targetValue - lastValue)).ToFormatString(), 0, 1, 0.1f).OnComplete(() =>
            //{
            com.ExpText.text = Player.TrainManager.Exp.ToFormatString();
            //    lastValue = Player.TrainManager.Exp.Clone();
            //});
        }

        //倍率切换
        private void OnSpeed()
        {
            Babu.EventManager.Instance.Dispatch(EventID.OnSpeedChange);
            Player.TrainManager.ChangeUpLevelType();
            //设置倍率指针方向和图片
            SetSpeedTransform();
            switch (Player.TrainManager.UpLevelType)
            {
                case TrainUpLevelType.UpgradeOne:
                    AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_1);
                    break;
                case TrainUpLevelType.UpgradeTen:
                    AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_2);
                    break;
                case TrainUpLevelType.UpgradeHundred:
                    AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_3);
                    break;
                case TrainUpLevelType.UpgradeMAX:
                    AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_4);
                    break;
            }
        }

        //设置倍率指针方向和图片
        private void SetSpeedTransform()
        {
            var angles = new int[] { -90, -180, 90, 0 };
            //设置倍率图片
            SpriteManager.GetSprite(AtlasNames.TrainUI, SpriteNames.TrainUI.SpeedSwitch[(int)Player.TrainManager.UpLevelType], (s) => { com.SpeedBtn.image.sprite = s; });
            //播放倍率按钮切换动画
            com.TrainAnim.PlaySpeedBtnSwitch(angles[(int)Player.TrainManager.UpLevelType]);
        }

        private void OnToggleValueChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);

            oldToggle?.GetComponent<StatusControl>().SetStatus(false);
            newToggle?.GetComponent<StatusControl>().SetStatus(true);
            HideAllPad();

            var key = oldToggle.GetComponent<ActivityToggle>();
            var key1 = newToggle.GetComponent<ActivityToggle>();

            initPad();

            if (newToggle.name == "BabuToggle0")
            {
                com.TrainAnim.ShowSpeedBtn();
            }
            if (oldToggle.name == "BabuToggle0" && newToggle.name != "BabuToggle0")
            {
                com.TrainAnim.HidSpeedBtn();
            }
        }

        private void HideAllPad()
        {
            foreach (var pad in com.padState.Values)
            {
                pad.SetActive(false);
            }
        }

        private async void initPad()
        {
            int padIndex = com.toggleGroup.EnableIndex;
            if (!com.padState.ContainsKey(padIndex))
            {
                var padtask = await CBAUtils.GetPrefab(com.padPathList[padIndex], com.padContainer.transform);
                com.padState.Add(padIndex, padtask);
            }

            com.padState[padIndex].SetActive(true);
            switch (padIndex)
            {
                case 0: OnRegular(); break;
                case 1: OnStrengthen(); break;
                case 3: OnInviteMatch(); break;
            }
        }

        private void OnRegular()
        {
            var pad = com.padState[com.toggleGroup.EnableIndex].GetComponent<RegularTrainPad>();
            //常规训练动画初始化
            pad.InitAnim();
            // 视图移动到顶部
            pad.ScrollToTop();
            com.SpeedBtn.enabled = true;
            //播放显示倍率按钮动画  
            com.TrainAnim.ShowSpeedBtn();
            //设置常规训练数据
            pad.SetData();
            // //播放进度条动画 //com.RegularTrainPad.PlayAnim();内部已经调用
            // com.RegularTrainPad.PlayProgressAnim();
            if (isFirst)
            {
                //初始化未解锁状态动画
                pad.InitUnlockAnim();
                //播放常规训练动画
                pad.PlayAnim();
                isFirst = false;
            }
            else
            {
                //常规训练动画初始化
                pad.InitAnim();
                //播放常规训练动画
                pad.PlayAnim();
            }
        }

        private void OnStrengthen()
        {
            var pad = com.padState[com.toggleGroup.EnableIndex].GetComponent<StrengthenTrainPad>();
            //实例化强化训练预制体
            pad.InstantiateItem();
            //设置强化训练数据
            pad.SetData();
            //播放动画
            pad.PlayAnim();
        }

        private void OnInviteMatch()
        {

            if (!Player.TrainManager.InviteMatchController.IsUnlock)
            {
                Tips.PopTips("解锁篮板训练后开放该功能。");
                return;
            }
            var pad = com.padState[com.toggleGroup.EnableIndex].GetComponent<InviteMatchPad>();
            pad.SetData();
            pad.PlayAnim();
            //InviteMatchItem.toResetBtn?.Invoke();
        }

        //private void OnClose()
        //{
        //    closeBtn.GetComponent<ButtonAnim>().PlayBack(() =>
        //    {

        //        FsmManager.Instance.ChangeToState<StateHome>(new StateCommonUserData()
        //        {
        //            OpenUIAction = async () =>
        //            {
        //                UIController.Instance.HidePanel<TrainUI>();
        //                //await UIController.Instance.ShowPanel<HomeUI>();
        //            }
        //        });

        //    });
        //}
    }
}