using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using Utils.GameItem;
using Protocol;
using System.Linq;
using Babu;
using GameItem = Utils.GameItem.GameItem;
using GameConfig;
using GameConfig.Config;

namespace BigBang.UI
{

    public class HundredGuessUIProperties : WindowProperties
    {
        public bool isOnlyShowExchange = false;

        public HundredGuessUIProperties(bool isOnlyShowExchange)
        {
            this.isOnlyShowExchange = isOnlyShowExchange;
        }
    }

    public class HundredGuessUI : AWindowController<HundredGuessUIProperties>
    {
        #region 初始化
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private List<GameObject> panelList = new();
        [SerializeField] private TMP_Text titleText = null;
        [SerializeField] private List<string> titleList = new();

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            toggleGroup.OnValueChanged += OnToggleChanged;
            itemButton.OnClick += OnItemClick;
            EventManager.Instance.Register(EventID.OnRefreshGoods, OnHundredAfterSupport);
            EventManager.Instance.Register(EventID.ClassicShopUIItemBuy, OnBuyItem);
            EventManager.Instance.Register(EventID.OnHundredNeedRefreshGuess, OnHundredNeedRefreshGuess);
            EventManager.Instance.Register(EventID.OnHundredNeedCloseGuess, OnHundredNeedCloseGuess);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            toggleGroup.OnValueChanged -= OnToggleChanged;
            itemButton.OnClick -= OnItemClick;
            EventManager.Instance.Unregister(EventID.OnRefreshGoods, OnHundredAfterSupport);
            EventManager.Instance.Unregister(EventID.ClassicShopUIItemBuy, OnBuyItem);
            EventManager.Instance.Unregister(EventID.OnHundredNeedRefreshGuess, OnHundredNeedRefreshGuess);
            EventManager.Instance.Unregister(EventID.OnHundredNeedCloseGuess, OnHundredNeedCloseGuess);
        }


        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            int selectIndex = toggleGroup.EnableIndex;
            for (int i = 0; i < panelList.Count; i++)
            {
                panelList[i].SetActive(i == selectIndex);
                titleText.text = titleList[selectIndex];
            }
            switch (selectIndex)
            {
                case 0:
                    RefreshExchange();
                    hundredGuessExchangeAdapter.PlayAnim();
                    break;
                case 1:
                    RefreshHistory();
                    hundredGuessHistoryAdapter.PlayAnim();
                    break;
                case 2:
                    RefreshSupport();
                    hundredGuessSupportAdapter.PlayAnim();
                    break;
            }
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            delayRefreshTimer?.Cancel();
            RefreshUI();
        }

        [SerializeField] private HundredGuessUIAnim hundredGuessUIAnim = null;

        private UnityTimer.Timer delayRefreshTimer = null;
        private void OnHundredNeedRefreshGuess(object[] _)
        {
            delayRefreshTimer?.Cancel();
            delayRefreshTimer = UnityTimer.Timer.Register(this.gameObject, 1.0f, () =>
            {
                RefreshUI(false);
            });
        }
        private void OnHundredNeedCloseGuess(object[] _)
        {
            UIController.Instance.CloseWindow<HundredGuessUI>();
        }
        private void RefreshUI(bool isEnter = true)
        {
            for (int i = 0; i < panelList.Count; i++)
            {
                panelList[i].SetActive(false);
            }
            if (Properties.isOnlyShowExchange)
            {
                titleText.text = titleList[0];
                (hundredGuessExchangeAdapter.transform as RectTransform).SetBottom(-90f);
                RefreshWhistleCount();
                hundredGuessUIAnim.Init();
                toggleGroup.Switch(0);
                hundredGuessUIAnim.PlayEnter(false);
            }
            else
            {
                if (isEnter)
                {
                    titleText.text = titleList[2];
                }
                (hundredGuessExchangeAdapter.transform as RectTransform).SetBottom(0f);
                RefreshWhistleCount();
                hundredGuessUIAnim.Init();
                HundredManager.Instance.GetSupportServerInfo(() =>
                {
                    if (HundredManager.Instance.guessCourseInfo.Stage != (int)HundredProgress.Fight2 && HundredManager.Instance.guessCourseInfo.Stage != (int)HundredProgress.Fight3)
                    {
                        UIController.Instance.CloseWindow<HundredGuessUI>();
                        return;
                    }
                    CheckRewards();
                    if (isEnter)
                    {
                        AutoSelectPage();
                    }
                    else
                    {
                        toggleGroup.Switch(toggleGroup.EnableIndex);
                    }
                    hundredGuessUIAnim.PlayEnter(true);
                });
            }
        }
        private void CheckRewards()
        {
            if (HundredManager.Instance.guessSupportInfo.FreeSupportGoods <= 0 && HundredManager.Instance.guessSupportInfo.WinSupportGoods <= 0) return;
            List<GameItem> data = new();
            if (HundredManager.Instance.guessSupportInfo.FreeSupportGoods > 0)
            {
                GameItem gameItemFree = GameItemUtils.CreateGameItem(GameItemType.Goods, GoodsId.HundredGuessWhistle, HundredManager.Instance.guessSupportInfo.FreeSupportGoods);
                data.Add(gameItemFree);
            }
            if (HundredManager.Instance.guessSupportInfo.WinSupportGoods > 0)
            {
                GameItem gameItemWin = GameItemUtils.CreateGameItem(GameItemType.Goods, GoodsId.HundredGuessWhistle, HundredManager.Instance.guessSupportInfo.WinSupportGoods);
                data.Add(gameItemWin);
            }
            if (HundredManager.Instance.nowCourse != null) HundredManager.Instance.nowCourse.HasSupportReward = false;
            HundredManager.Instance.guessSupportInfo.FreeSupportGoods = 0;
            HundredManager.Instance.guessSupportInfo.WinSupportGoods = 0;
            var properties = new InventoryObtainedUIProperties(data);
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            HundredManager.Instance.CheckHundredRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }
        private void AutoSelectPage()
        {
            bool isSupportEmpty = IsSuppertEmpty();
            if (!isSupportEmpty)
            {
                toggleGroup.Switch(2);
                return;
            }
            bool isHistoryEmpty = IsHistoryEmpty();
            if (!isHistoryEmpty)
            {
                toggleGroup.Switch(1);
                return;
            }
            toggleGroup.Switch(0);
        }
        private bool IsSuppertEmpty()
        {
            bool isEmpty = HundredManager.Instance.guessCourseInfo.LeagueCourseItemList.FirstOrDefault(IsCourseInSupportList) == null;
            return isEmpty;
        }
        private bool IsCourseInSupportList(LeagueCourseItemData leagueCourseItemData)
        {
            if (leagueCourseItemData == null) return false;
            if (leagueCourseItemData.AwayTeam == null) return false;
            if (leagueCourseItemData.HomeTeam == null) return false;
            if (leagueCourseItemData.HomeGoal > 0) return false;
            if (leagueCourseItemData.AwayGoal > 0) return false;
            if (leagueCourseItemData.AwayTeam.TeamId == Player.GbId) return false;
            if (leagueCourseItemData.HomeTeam.TeamId == Player.GbId) return false;
            if (leagueCourseItemData.Time < Utils.DataConvUtil.ServerTime + 30 * 60) return false;//比赛开始30分钟前开始不能应援
            return true;
        }
        private bool IsHistoryEmpty()
        {
            return HundredManager.Instance.guessSupportInfo.SupportPlayoffCourses.Count <= 0 && HundredManager.Instance.guessSupportInfo.SupportChampionCourses.Count <= 0;
        }

        private void OnClose()
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);

            UIController.Instance.CloseWindow<HundredGuessUI>();
        }

        #endregion

        #region 右上角哨子

        [SerializeField] private BabuButton itemButton = null;
        [SerializeField] private TMP_Text itemCountText = null;

        private void OnItemClick(BabuButton _)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Goods, GoodsId.HundredGuessWhistle, Player.PackageManager.GetGoodsNumber(GoodsId.HundredGuessWhistle));
            itemtipsUIProperties.SetPos(itemButton.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
        private void RefreshWhistleCount()
        {
            int whistleCount = Player.PackageManager.GetGoodsNumber(GoodsId.HundredGuessWhistle);
            itemCountText.text = whistleCount.ToString();
        }
        private void OnHundredAfterSupport(object[] _)
        {
            RefreshWhistleCount();
        }

        #endregion

        #region 应援

        private void RefreshSupport()
        {
            RereshState();
            RefreshSupportList();
        }

        [SerializeField] private TMP_Text guessStateText = null;
        private void RereshState()
        {
            HundredProgress hundredProgress = (HundredProgress)HundredManager.Instance.guessCourseInfo.Stage;
            switch (hundredProgress)
            {
                case HundredProgress.Fight2:
                    {
                        string str = HundredManager.Instance.GetRoundMatchTitle(true, HundredManager.Instance.GetNowMaxRound(HundredManager.Instance.guessCourseInfo));
                        if (string.IsNullOrWhiteSpace(str) == false)
                        {
                            guessStateText.text = "当前赛程:淘汰赛" + str;
                        }
                        else
                        {
                            guessStateText.text = "";
                        }
                    }
                    break;
                case HundredProgress.Fight3:
                    {
                        string str = HundredManager.Instance.GetRoundMatchTitle(false, HundredManager.Instance.GetNowMaxRound(HundredManager.Instance.guessCourseInfo));
                        if (string.IsNullOrWhiteSpace(str) == false)
                        {
                            guessStateText.text = "当前赛程:冠军赛" + str;
                        }
                        else
                        {
                            guessStateText.text = "";
                        }
                    }
                    break;
                default:
                    guessStateText.text = "";
                    break;
            }
        }

        [SerializeField] private RectTransform guessEmptyPanel = null;
        [SerializeField] private HundredGuessSupportAdapter hundredGuessSupportAdapter = null;
        private void RefreshSupportList()
        {
            guessEmptyPanel.gameObject.SetActive(false);
            hundredGuessSupportAdapter.gameObject.SetActive(false);
            bool isEmpty = false;
            HundredProgress hundredProgress = (HundredProgress)HundredManager.Instance.guessCourseInfo.Stage;
            List<LeagueCourseItemData> leagueCourseItemDataList = null;
            leagueCourseItemDataList = HundredManager.Instance.guessCourseInfo.LeagueCourseItemList.Where(IsCourseInSupportList).ToList();
            isEmpty = leagueCourseItemDataList.Count <= 0;
            if (isEmpty)
            {
                guessEmptyPanel.gameObject.SetActive(true);
                return;
            }
            hundredGuessSupportAdapter.gameObject.SetActive(true);
            hundredGuessSupportAdapter.SetData(leagueCourseItemDataList);
        }

        #endregion

        #region 应援记录

        [SerializeField] private RectTransform historyEmptyPanel = null;
        [SerializeField] private HundredGuessHistoryAdapter hundredGuessHistoryAdapter = null;
        private void RefreshHistory()
        {
            historyEmptyPanel.gameObject.SetActive(false);
            hundredGuessHistoryAdapter.gameObject.SetActive(false);
            List<LeagueCourseItemData> LeagueCourseItemDataList = new();
            LeagueCourseItemDataList.AddRange(HundredManager.Instance.guessSupportInfo.SupportChampionCourses.Reverse());
            LeagueCourseItemDataList.AddRange(HundredManager.Instance.guessSupportInfo.SupportPlayoffCourses.Reverse());
            bool isEmpty = LeagueCourseItemDataList.Count <= 0;
            if (isEmpty)
            {
                historyEmptyPanel.gameObject.SetActive(true);
                return;
            }
            hundredGuessHistoryAdapter.gameObject.SetActive(true);
            hundredGuessHistoryAdapter.SetData(LeagueCourseItemDataList);
        }

        #endregion

        #region 兑换

        [SerializeField] private HundredGuessExchangeAdapter hundredGuessExchangeAdapter;
        private void RefreshExchange()
        {
            List<ShopItemData> shopItems = new List<ShopItemData>();
            var itemConfigs = Configs.GameItemShop.GetConfigList();
            foreach (var cfg in itemConfigs)
            {
                shopItems.Add(new ShopItemData(cfg));
            }
            hundredGuessExchangeAdapter.SetData(shopItems, 11);
        }
        private void OnBuyItem(object[] args)
        {
            GameItemShopConfig gameItemShopConfig = Configs.GameItemShop.GetConfig((int)args[0]);
            GameItem getGameItem = GameItemUtils.CreateGameItem(gameItemShopConfig.Item);
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(getGameItem));
            RefreshExchange();
        }

        #endregion
    }
}