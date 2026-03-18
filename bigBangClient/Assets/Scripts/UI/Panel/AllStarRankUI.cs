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
using Protocol;

namespace BigBang.UI
{
    public class AllStarRankUI : APanelController
    {
        #region 初始化
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
        }
        private Area area = Area.South;
        protected override void OnPropertiesSet()
        {
            area = (Area)AllStarManager.Instance.serverData.Area;
            bottomToggleGroup.Switch(area == Area.South ? 0 : 1);
        }
        #endregion

        #region 按钮回调
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.HidePanel<AllStarRankUI>();
        }
        #endregion

        #region 切换页签
        [SerializeField] private BabuToggleGroup bottomToggleGroup = null;
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            area = selectedIndex == 0 ? Area.South : Area.North;
            RefreshUI();
            AllStarManager.Instance.GetRankData(area, () =>
            {
                RefreshUI();
            });
        }
        #endregion

        #region 刷新内容
        [SerializeField] private List<AllStarTopRankItem> topRankItemList = null;
        [SerializeField] private AllStarRankAdapter allStarRankAdapter = null;
        [SerializeField] private RectTransform emptyPanel = null;
        [SerializeField] private AllStarRankItem myRankItem = null;

        private void RefreshUI()
        {
            List<AllStarRankInfo> rankInfoList = AllStarManager.Instance.GetRankInfoList(area);
            for (int i = 0; i < topRankItemList.Count; i++)
            {
                AllStarTopRankItem allStarTopRankItem = topRankItemList[i];
                AllStarRankInfo allStarRankInfo = i >= rankInfoList.Count ? null : rankInfoList[i];
                allStarTopRankItem.SetData(allStarRankInfo);
            }
            bool isEmpty = rankInfoList.Count <= 0;
            allStarRankAdapter.gameObject.SetActive(!isEmpty);
            emptyPanel.gameObject.SetActive(isEmpty);
            bool needShowSelf = !isEmpty && AllStarManager.Instance.serverData != null && AllStarManager.Instance.serverData.Area != 0 && (Area)AllStarManager.Instance.serverData.Area == area;
            myRankItem.gameObject.SetActive(needShowSelf);
            (allStarRankAdapter.transform as RectTransform).SetBottom(needShowSelf ? 246f : 146f);
            if (!isEmpty)
            {
                allStarRankAdapter.SetData(rankInfoList);
            }
            if (needShowSelf)
            {
                AllStarRankInfo allStarRankInfo = rankInfoList.FirstOrDefault(info => info.Gbid == Player.GbId);
                myRankItem.SetData(allStarRankInfo, true, -1);
            }
        }


        #endregion
    }
}