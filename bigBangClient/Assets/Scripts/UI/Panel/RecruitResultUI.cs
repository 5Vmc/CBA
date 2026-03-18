using System.Collections.Generic;
using System.Linq;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using Google.Protobuf.Collections;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    [System.Serializable]
    public class RecruitResultProperties : PanelProperties
    {
        public List<GameItem> ResultList { get; set; }
        public int PoolId { get; set; }
        public RecruitCountType RecruitType { get; set; }
        public int CostType { get; set; }
        // public RecruitResultProperties(List<GameItem> resultList)
        // {
        //     ResultList = resultList;
        // }

        public RecruitResultProperties(int poolId, RepeatedField<Protocol.GameItem> resultList, int recruitType, int costType)
        {
            ResultList = GameItemUtils.UnPackList(resultList).ToList();
            PoolId = poolId;
            RecruitType = (RecruitCountType)recruitType;
            CostType = costType;
        }
    }

    public class RecruitResultUI : APanelController<RecruitResultProperties>
    {
        [SerializeField] private List<CardAndDebrisItem> tenCardList;

        [SerializeField] private CardAndDebrisItem onceCardInfo;

        [SerializeField] private GameObject tenCardPad;
        [SerializeField] private GameObject onceCardPad;

        [SerializeField] private Button closeBtn;
        // [SerializeField] private Button continueBtn;
        [SerializeField] private RecruitBtn continueBtn;

        public RecruitResultUIAnim Anim;

        // 招募次数类型
        // private int _recruitType = RecruitCountType.Once;
        //普通招募次数

        protected override void AddListeners()
        {
            base.AddListeners();

            closeBtn.onClick.AddListener(OnClose);
            continueBtn.SetClickAction(OnContinue);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }

        private void OnClose()
        {
            closeBtn.GetComponent<ButtonAnim>().Play(() =>
            {
                UIController.Instance.HidePanel<RecruitResultUI>();
                //UIController.Instance.ShowPanel<RecruitUI>();
                Babu.EventManager.Instance.Dispatch(EventID.InitRecruitUIModelAnim);
                // UIController.Instance.HidePanel<RecruitResultUI>();
                TrigStoreReviewByRecruit();
            });
        }

        private void OnContinue(RecruitCountType recruitCountType, RecruitCostType recruitCostType)
        {
            Player.CardManager.RecruitController.DoRecruit(Properties.PoolId, recruitCountType, recruitCostType, OnRecruitSuccess);
        }

        private void OnRecruitSuccess(RecruitResponse response)
        {
            Properties = new RecruitResultProperties(
                    response.PoolInfo.PoolId,
                    response.ResultList,
                    response.RecruitCountType,
                    response.CostType);
            RefreshUI();
        }

        [SerializeField] private RecruitResultUIGuide recruitResultUIGuide;
        protected override void OnPropertiesSet()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (Properties.ResultList.Exists(item => item.Type == GameItemType.Card && item.GetQuality() >= 4))
            {
                needTrigStoreReview = true;
            }

            var pool = Player.CardManager.RecruitController.GetPool(Properties.PoolId);
            var isActivity = pool.HasActivity();
            continueBtn.SetButtonStyle(Properties.RecruitType, isActivity);

            tenCardPad.SetActive(Properties.RecruitType == RecruitCountType.Ten);
            onceCardPad.SetActive(Properties.RecruitType == RecruitCountType.Once);
            // 抽一次
            if (Properties.RecruitType == RecruitCountType.Once)
            {
                AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUT_SHOW_UP);
                ShowGameItem(onceCardInfo, Properties.ResultList[0]);
                if (Properties.ResultList.Exists(item => item.Type == GameItemType.Goods))
                {
                    UnityTimer.Timer.Register(this.gameObject, 2.5f, () => AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUTFRAG));
                }
            }
            // 抽10次
            else
            {
                AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUT10_SHOW_UP);
                if (Properties.ResultList.Count < 10)
                {
                    Debug.LogError($"抽10次：结果抽了{Properties.ResultList.Count}");
                }
                for (int i = 0; i < Properties.ResultList.Count; i++)
                {
                    ShowGameItem(tenCardList[i], Properties.ResultList[i]);
                }
                if (Properties.ResultList.Exists(item => item.Type == GameItemType.Goods))
                {
                    UnityTimer.Timer.Register(this.gameObject, 3, () => AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUTFRAG));
                }
            }

            //刷新赠送信息
            RefreshFreebie();

            recruitResultUIGuide.CheckGuide();

            // 播放抽卡动画
            Anim.PlayEnter(Properties.RecruitType == RecruitCountType.Once, () =>
            {
                TrigStoreReviewByRecruit();
                recruitResultUIGuide.AfterRecruitAnimPlayEnd();
            });
        }

        bool needTrigStoreReview = false;
        private void TrigStoreReviewByRecruit()
        {
            if (needTrigStoreReview == false) return;
            needTrigStoreReview = false;
            bool needShowStoreReviewByRecruit = PlayerPrefs.GetInt(PlayerPrefsKeys.NeedShowStoreReviewByRecruit, 1) == 1;
            if (needShowStoreReviewByRecruit)
            {
                PlayerPrefs.SetInt(PlayerPrefsKeys.NeedShowStoreReviewByRecruit, 0);
                GameManager.Instance.TrigIosShopReview();
            }
        }

        private void ShowGameItem(CardAndDebrisItem cad, GameItem item)
        {
            // 如果是卡牌
            if (item.Type == GameItemType.Card)
            {
                var cardConfig = Configs.CardModel.GetConfig(item.Id);
                cad.SetData(cardConfig);
            }
            // 如果是碎片
            else if (item.Type == GameItemType.Goods)
            {
                var goodsConfig = Configs.Goods.GetConfig(item.Id);
                var cardConfig = Configs.CardModel.GetConfig(goodsConfig.Param2);
                // 碎片数量
                cad.SetData(cardConfig, item.Count);
            }
            else
            {
                Debug.LogError("啥都不是");
            }
        }

        #region 赠品
        [SerializeField] private InventoryItem freebieInventoryItem = null;
        [SerializeField] private TMP_Text freebieTipText = null;
        private void RefreshFreebie()
        {
            int freebieCount = 0;
            switch (Properties.RecruitType)
            {
                case RecruitCountType.Once: freebieCount = 60; break;
                case RecruitCountType.Ten: freebieCount = 600; break;
            }
            freebieInventoryItem.SetData(GoodsId.ContractFragment, freebieCount, true);
            freebieTipText.text = "额外赠送{0}".SafeFormat(freebieInventoryItem.GetGameItem().GetName());
        }
        #endregion

    }
}