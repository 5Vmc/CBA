using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using BigBang.Animation;
using TMPro;
using Babu;
using Utils;
using System;
using Utils.GameItem;

namespace BigBang.UI
{
    public class LeagueSessionRewardUIProperties : WindowProperties
    {
        public Action Callback = null;
        public LeagueSessionRewardUIProperties(Action callback = null)
        {
            Callback = callback;
        }
    }
    public class LeagueSessionRewardUI : AWindowController<LeagueSessionRewardUIProperties>
    {
        #region 初始化与监听
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuButton confirmBtn = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClickCloseBtn;
            confirmBtn.OnClick += OnClickCloseBtn;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClickCloseBtn;
            confirmBtn.OnClick -= OnClickCloseBtn;
        }
        #endregion

        #region 退出与保存
        private void OnClickCloseBtn(BabuButton _)
        {
            NetworkManager.Instance.GetPVPReward(CompitionID.League, (resp) =>
            {
                if (resp.ReceiveSucceed)
                {
                    var properties = new InventoryObtainedUIProperties(Player.PVPManager.tmpRewards[CompitionID.League], Properties.Callback);
                    UIController.Instance.CloseWindow<LeagueSessionRewardUI>();
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);// 打开通用收益界面
                    //清理临时数据和小红点
                    Player.PVPManager.tmpRewards[CompitionID.League] = new System.Collections.Generic.List<Utils.GameItem.GameItem>();
                    Player.PVPManager.RefreshLeagueRedDot();
                }
            });
        }
        #endregion

        #region 数据刷新与显示刷新
        [SerializeField] private TMP_Text tipText = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            tipText.text = "您在离开期间,已完成了<size=34><color=#09811A>{0}场</color></size>比赛".SafeFormat(Player.PVPManager.tmpRewards[CompitionID.League].Count);
            SetRewards();
            scrollView.enabled = false;
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                scrollView.enabled = true;
                scrollView.verticalNormalizedPosition = 1f;
            });
        }

        [SerializeField] private InventoryItem itemPrefab = null;
        [SerializeField] private Transform content;
        [SerializeField] private ScrollRect scrollView = null;
        private void SetRewards()
        {
            Transform layoutTrans = content;
            List<GameItem> gameItemList = Player.PVPManager.tmpRewards[CompitionID.League];
            while (layoutTrans.childCount < gameItemList.Count) Instantiate(itemPrefab, layoutTrans);
            for (int i = 0; i < layoutTrans.childCount; i++)
            {
                if (i < gameItemList.Count)
                {
                    var reward = gameItemList[i];
                    var child = layoutTrans.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.GetComponent<InventoryItem>().SetData(reward);
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.transform as RectTransform);
            scrollView.verticalNormalizedPosition = 1f;
        }

        #endregion

    }
}