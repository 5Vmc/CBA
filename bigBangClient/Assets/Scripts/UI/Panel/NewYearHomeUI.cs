using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class NewYearHomeUI : APanelController
    {
        #region 初始化
        public enum SubUIID
        {
            Challenge = 0,
            Task = 1,
            Gift = 2,
        }

        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClose;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClose;
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        }

        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuToggleGroup bottomToggleGroup = null;
        [SerializeField] private NewYearChallengePad newYearChallengePad = null;
        [SerializeField] private NewYearTaskPad newYearTaskPad = null;
        [SerializeField] private NewYearGiftPad newYearGiftPad = null;

        SubUIID subUIID = SubUIID.Challenge;
        int leftRefreshTime = -1;
        protected override void OnPropertiesSet()
        {
            subUIID = SubUIID.Challenge;
            bottomToggleGroup.Switch((int)subUIID);
            RefreshRedDot(null);
        }
        private void RefreshLeftTimeOneSec()
        {
            if (leftRefreshTime < 0) return;
            leftRefreshTime--;
            if (leftRefreshTime == 0)
            {
                RefreshUI();
            }
        }
        private void RefreshUI()
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);
            ShowPad(subUIID);
        }

        [SerializeField] private Image newYearRedDotChallenge = null;
        [SerializeField] private Image newYearRedDotTask = null;
        [SerializeField] private Image newYearRedDotGift = null;
        private void RefreshRedDot(object[] _)
        {
            RedDotNode nodeChallenge = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYear, "/Challenge");
            if (newYearRedDotChallenge.IsDestroyed() == false) nodeChallenge.IsRed(newYearRedDotChallenge.transform);
            RedDotNode nodeTask = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYear, "/Task");
            if (newYearRedDotTask.IsDestroyed() == false) nodeTask.IsRed(newYearRedDotTask.transform);
            RedDotNode nodeGift = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYear, "/Gift");
            if (newYearRedDotGift.IsDestroyed() == false) nodeGift.IsRed(newYearRedDotGift.transform);
        }
        #endregion

        #region 关闭界面
        private void OnClose(BabuButton _)
        {
            UIController.Instance.HidePanel<NewYearHomeUI>();
        }
        #endregion

        #region 切换页签

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            ShowPad((SubUIID)selectedIndex);
        }
        private void ShowPad(SubUIID padIndex)
        {
            HideAllPad();
            switch (padIndex)
            {
                case SubUIID.Challenge: OnShowChallenge(); break;
                case SubUIID.Task: OnShowTask(); break;
                case SubUIID.Gift: OnShowGift(); break;
            }
        }
        private void HideAllPad()
        {
            newYearChallengePad.gameObject.SetActive(false);
            newYearTaskPad.gameObject.SetActive(false);
            newYearGiftPad.gameObject.SetActive(false);
        }

        private void OnShowChallenge()
        {
            newYearChallengePad.gameObject.SetActive(true);
            newYearChallengePad.OnShow();
        }
        private void OnShowTask()
        {
            newYearTaskPad.gameObject.SetActive(true);
            newYearTaskPad.OnShow();
        }
        private void OnShowGift()
        {
            newYearGiftPad.gameObject.SetActive(true);
            newYearGiftPad.OnShow();
        }

        #endregion
    }
}
