using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameConfig.Config;
using GameConfig;
using static BigBang.AllStarManager;
using Utils.GameItem;
using System;
using Protocol;
using GameItem = Utils.GameItem.GameItem;
using Babu;

namespace BigBang.UI
{
    public class HonourGetUIProperties : WindowProperties
    {
        public AchievementData achievementData;
        public Action closeCallBack;
        public bool isFirstHonourGet = false;
        public HonourGetUIProperties(AchievementData achievementData, bool isFirstHonourGet, Action closeCallBack = null)
        {
            this.achievementData = achievementData;
            this.isFirstHonourGet = isFirstHonourGet;
            this.closeCallBack = closeCallBack;
        }
    }
    public class HonourGetUI : AWindowController<HonourGetUIProperties>
    {
        #region 初始化与监听

        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private Image cupImage = null;
        [SerializeField] private HonourGetUIAnim anim = null;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClickClose);
        }
        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClickClose);
        }
        private AchievementData achievementData = null;
        protected override void OnPropertiesSet()
        {
            achievementData = Properties.achievementData;
            RefreshUI();
            anim.PlayEnter();
            anim.PlayDark(Properties.isFirstHonourGet);
        }
        [SerializeField] private Button closeBtn = null;
        private void OnClickClose()
        {
            Player.AchievementManager.GetAchievementRewards(achievementData.ID, (resp) =>
            {
                achievementData.Received = 1;
                AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
                UIController.Instance.CloseWindow<HonourGetUI>();
                Player.AchievementManager.CheckHonourRedDot();
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                Properties.closeCallBack?.Invoke();
            });
        }
        #endregion

        #region 通用的部分

        private async void RefreshUI()
        {
            nameText.text = "{0}-{1}".SafeFormat(achievementData.Config.GroupTitle, achievementData.Config.Name);
            cupImage.sprite = await SpriteProxy.GetHonourCup(achievementData.Config.Icon);
        }

        #endregion 

        #region 动画

        #endregion

    }
}