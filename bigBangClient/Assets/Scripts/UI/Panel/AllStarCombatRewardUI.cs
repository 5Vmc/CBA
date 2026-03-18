using System;
using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Linq;
using Coffee.UIEffects;
using GameConfig.Config;
using Babu.Config;
using Babu;
using static BigBang.SpriteNames;
using static BigBang.AllStarManager;

namespace BigBang.UI
{
    public class AllStarCombatRewardUI : APanelController
    {
        #region 初始化
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
        }
        #endregion

        #region 按钮回调
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.HidePanel<AllStarCombatRewardUI>();
        }
        #endregion

        #region 刷新内容
        protected override void OnPropertiesSet()
        {
            RefreshNameAndCombat();
            RefreshProgress();
            RefreshReward();
            RefreshArea();
        }

        [SerializeField] private TMP_Text clubNameText = null;
        [SerializeField] private ImageFont combatImageFont = null;
        private void RefreshNameAndCombat()
        {
            clubNameText.text = Player.Name;
            combatImageFont.text = AllStarManager.Instance.savedTotalMaxCombatInServer.ToString("N0");
        }
        [SerializeField] private List<Image> progressFgList = new();
        [SerializeField] private List<AllStarCombatStageItem> stageItemList = new();
        private void RefreshProgress()
        {
            List<AllStarRewardConfig> allStarRewardConfigList = Configs.AllStarReward.GetConfigList()
                .Where((AllStarRewardConfig allStarRewardConfig) =>
                {
                    if (allStarRewardConfig.Type != 3) return false;
                    if (allStarRewardConfig.Group != AllStarManager.Instance.group) return false;
                    return true;
                })
                .OrderBy(cfg => cfg.Option)
                .ToList();
            for (int i = 0; i < stageItemList.Count; i++)
            {
                stageItemList[i].SetData(allStarRewardConfigList[i]);
            }
            for (int i = 0; i < progressFgList.Count; i++)
            {
                int lastOption = 0;
                if (i > 0)
                {
                    lastOption = stageItemList[i - 1].allStarRewardConfig.Option;
                }
                int left = (AllStarManager.Instance.savedTotalMaxCombatInServer - lastOption);
                int right = (stageItemList[i].allStarRewardConfig.Option - lastOption);
                float fillAmount = left / (float)right;
                progressFgList[i].fillAmount = fillAmount;
            }
        }
        [SerializeField] private List<AllStarCambatRewardItem> rewardItemList = new();
        private void RefreshReward()
        {
            for (int i = 0; i < rewardItemList.Count; i++)
            {
                rewardItemList[i].SetData(stageItemList[i].allStarRewardConfig);
            }
        }
        [SerializeField] private List<Image> northImageList;
        [SerializeField] private List<Image> southImageList;
        private void RefreshArea()
        {
            foreach (var item in northImageList)
            {
                item.gameObject.SetActive(AllStarManager.Instance.serverData.Area == (int)Area.North);
            }
            foreach (var item in southImageList)
            {
                item.gameObject.SetActive(AllStarManager.Instance.serverData.Area == (int)Area.South);
            }
        }

        #endregion
    }
}