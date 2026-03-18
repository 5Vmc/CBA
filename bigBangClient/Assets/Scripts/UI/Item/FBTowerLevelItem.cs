using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;
using GameConfig.Config;
using Utils;
using Babu;
using GameConfig;

namespace BigBang.UI
{
    public class FBTowerLevelItem : MonoBehaviour
    {

        [SerializeField] private BabuButton towerLevelButton = null;
        [SerializeField] private TMP_Text normalLightLevelText = null;
        [SerializeField] private TMP_Text normalDarkLevelText = null;

        [SerializeField] private RectTransform normalLightPanel = null;
        [SerializeField] private SkeletonGraphic normalCircleSpineSkeletonGraphic = null;
        [SerializeField] private Image starBgImage = null;
        [SerializeField] private RectTransform normalDarkPanel = null;
        [SerializeField] private RectTransform buffLightPanel = null;
        [SerializeField] private SkeletonGraphic buffCircleSpineSkeletonGraphic = null;
        [SerializeField] private RectTransform buffDarkPanel = null;
        [SerializeField] private Image arrowImage = null;

        [SerializeField] private List<Image> starImageList = new();

        [HideInInspector] public int index = -1;

        void OnEnable()
        {
            towerLevelButton.OnClick += OnClickTowerLevelButton;
        }
        void OnDisable()
        {
            towerLevelButton.OnClick -= OnClickTowerLevelButton;
        }
        public TowerLevelData towerLevelData = null;
        public void RefreshShow(TowerLevelData towerLevelData)
        {
            this.towerLevelData = towerLevelData;
            Refresh();
        }

        private void Refresh()
        {
            bool isLight = towerLevelData.towerOpenState == TowerOpenState.Pass || towerLevelData.towerOpenState == TowerOpenState.Now;
            arrowImage.gameObject.SetActive(towerLevelData.towerOpenState == TowerOpenState.Now);
            normalLightPanel.gameObject.SetActive(isLight && towerLevelData.towerTypeState == TowerTypeState.Normal);
            normalDarkPanel.gameObject.SetActive(!isLight && towerLevelData.towerTypeState == TowerTypeState.Normal);
            buffLightPanel.gameObject.SetActive(isLight && towerLevelData.towerTypeState == TowerTypeState.Buff);
            buffDarkPanel.gameObject.SetActive(!isLight && towerLevelData.towerTypeState == TowerTypeState.Buff);
            starBgImage.gameObject.SetActive(towerLevelData.towerTypeState == TowerTypeState.Normal && towerLevelData.towerOpenState == TowerOpenState.Pass);
            normalCircleSpineSkeletonGraphic.gameObject.SetActive(towerLevelData.towerTypeState == TowerTypeState.Normal && towerLevelData.towerOpenState == TowerOpenState.Now);
            buffCircleSpineSkeletonGraphic.gameObject.SetActive(towerLevelData.towerTypeState == TowerTypeState.Buff && towerLevelData.towerOpenState == TowerOpenState.Now);

            if (towerLevelData.towerTypeState == TowerTypeState.Normal && towerLevelData.towerOpenState == TowerOpenState.Pass)
            {
                int starCount = 0;
                if (towerLevelData.passData != null && towerLevelData.passData.Stars.Count > 0)
                {
                    for (int i = 0; i < towerLevelData.passData.Stars.Count; i++)
                    {
                        starCount += towerLevelData.passData.Stars[i];
                    }
                }
                for (int i = 0; i < starImageList.Count; i++)
                {
                    starImageList[i].gameObject.SetActive(i < starCount);
                }
            }

            if (towerLevelData.towerTypeState == TowerTypeState.Normal)
            {
                if (isLight)
                {
                    normalLightLevelText.text = (index + 1).ToString();
                }
                else
                {
                    normalDarkLevelText.text = (index + 1).ToString();
                }
            }
        }

        private void OnClickTowerLevelButton(BabuButton _)
        {
            var cfg = Configs.Tower.GetConfig(FBTowerController.Instance.FBData.currentDungeonId);
            if (cfg.Lv > Player.Level)
            {
                Tips.PopTips(cfg.Lv + "级可挑战");
                return;
            }
            if (towerLevelData.towerOpenState == TowerOpenState.Lock)
            {
                Tips.PopTips("请先通关前置关卡");
                return;
            }
            if (towerLevelData.towerOpenState == TowerOpenState.Pass)
            {
                Tips.PopTips("该关卡已通关");
                return;
            }
            if (FBTowerController.Instance.FBData.failCount >= FBTowerController.MaxDailyFailCount)
            {
                Tips.PopTips("失败次数已达上限({0}/{1})".SafeFormat(FBTowerController.Instance.FBData.failCount, FBTowerController.MaxDailyFailCount));
                return;
            }
            EventManager.Instance.Dispatch(EventID.OnClickFBTowerLevelItem, this);
        }


    }
}

