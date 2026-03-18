using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using Protocol;
using System.Linq;
using TMPro;
using System;
using static BigBang.BattleManager;
using Utils;
using System.Collections.Generic;
using BigBang.Animation;

namespace BigBang.UI
{
    public class LeagueHistoryUI : APanelController
    {
        #region 初始化

        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private LeagueHistoryUIAnim anim = null;
        [SerializeField] private RectTransform emptyPanel = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            emptyPanel.gameObject.SetActive(false);
            anim.InitTopBar();
            anim.PlayEnterTopBar();
            anim.InitAdapter();
            OnHistorySelect();
        }

        private void OnClose(BabuButton _)
        {
            UIController.Instance.HidePanel<LeagueHistoryUI>();
        }

        #endregion

        #region  历史列表

        [SerializeField] private LeagueHistoryAdapter adapter;
        private void OnHistorySelect()
        {
            NetworkManager.Instance.GetLeagueHistory((GetLeagueHistoryResponse response) =>
            {
                List<LeagueHistoryData> dataList = response.LeagueHistoryDataList.ToList();
                dataList.Reverse();
                if (dataList.Count > 0)
                {
                    adapter.SetData(dataList);
                    anim.InitAdapter();
                    anim.PlayEnterAdapter(true);
                }
                emptyPanel.gameObject.SetActive(dataList.Count <= 0);
            });
        }

        #endregion

    }
}