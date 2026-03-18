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
using Utils.GameItem;
using Protocol;
using GameItem = Utils.GameItem.GameItem;
using DG.Tweening;

namespace BigBang.UI
{
    public class ChristmasTreeUI : APanelController
    {
        #region 初始化
        [SerializeField] private Image darkSnowImage = null;
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private Image stickImage = null;
        [SerializeField] private BabuButton taskButton = null;
        [SerializeField] private Image dotNodeImg = null;
        [SerializeField] private Image treeImage = null;
        [SerializeField] private List<ChristmasTreeItem> itemList = null;
        [SerializeField] private BabuButton chargeOnceButton = null;
        [SerializeField] private BabuButton chargeTenButton = null;
        [SerializeField] private TMP_Text leftTimeText = null;
        [SerializeField] private BabuButton batteryPanel = null;
        [SerializeField] private TMP_Text batteryNumText = null;
        [SerializeField] private BabuButton helpButton = null;
        [SerializeField] private BabuButton skipButton = null;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            batteryPanel.OnClick += OnClickBatteryPanel;
            taskButton.OnClick += OnClickTaskPanel;
            helpButton.OnClick += OnClickHelpButton;
            chargeOnceButton.OnClick += OnClickChargeOnceButton;
            chargeTenButton.OnClick += OnClickChargeTenButton;
            skipButton.OnClick += OnClickSkipButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
            EventManager.Instance.Register(EventID.OnRefreshGoods, RefreshBatteryCount);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        }
        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            batteryPanel.OnClick -= OnClickBatteryPanel;
            taskButton.OnClick -= OnClickTaskPanel;
            helpButton.OnClick -= OnClickHelpButton;
            chargeOnceButton.OnClick -= OnClickChargeOnceButton;
            chargeTenButton.OnClick -= OnClickChargeTenButton;
            skipButton.OnClick -= OnClickSkipButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
            EventManager.Instance.Unregister(EventID.OnRefreshGoods, RefreshBatteryCount);
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
        }
        #endregion

        #region 屏幕适配

        private void Start()
        {
            ScreenFix();
        }

        private float chargePanelY219 = 296.7f;
        private float chargePanelY169 = 200f;
        [SerializeField] private RectTransform chargeOncePanel = null;
        [SerializeField] private RectTransform chargeTenPanel = null;

        private float topPanelY219 = -249;
        private float topPanelY169 = -100f;
        [SerializeField] private RectTransform topPanel = null;
        private void ScreenFix()
        {
            float t = UIFrame.GetFixScreenLerpT();

            float chargePanelY = Mathf.Lerp(chargePanelY169, chargePanelY219, t);
            chargeOncePanel.SetAnchoredPositionY(chargePanelY);
            chargeTenPanel.SetAnchoredPositionY(chargePanelY);

            float topPanelY = Mathf.Lerp(topPanelY169, topPanelY219, t);
            topPanel.SetAnchoredPositionY(topPanelY);

            float hw1480720 = 1480f / 720f;
            float hwScreen = (float)UIFrame.height / (float)UIFrame.width;
            darkSnowImage.gameObject.SetActive(hwScreen >= hw1480720);
        }

        #endregion

        #region 按钮回调
        private void OnClose()
        {
            if (isAnimDoing) return;
            closeBtn.GetComponent<ButtonAnim>().PlayBack(() => UIController.Instance.HidePanel<ChristmasTreeUI>(), playAudio: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            });
        }
        private void OnClickBatteryPanel(BabuButton _)
        {
            if (isAnimDoing) return;
            GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, festivalBoxConfig.KeyId, 0);
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
        }
        private void OnClickTaskPanel(BabuButton _)
        {
            if (isAnimDoing) return;
            UIController.Instance.OpenWindow<ChristmasTaskUI>();
        }
        private void OnClickHelpButton(BabuButton _)
        {
            if (isAnimDoing) return;
            UIController.Instance.OpenWindow<ChristmasHelpUI>();
        }
        private void OnClickChargeOnceButton(BabuButton _)
        {
            if (isAnimDoing) return;
            Charge(1);
        }
        private void OnClickChargeTenButton(BabuButton _)
        {
            if (isAnimDoing) return;
            Charge(3);
        }
        private void Charge(int count)
        {
            GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, festivalBoxConfig.KeyId, 0);
            int playerCount = gameItem.GetPlayerCount();
            if (playerCount < count)
            {
                Tips.PopTips("{0}数量不足".SafeFormat(gameItem.GetName()));
                return;
            }
            NetworkManager.Instance.OpenFestivalBox(ActivityID.ChristmasTree, count, (OpenFestivalBoxResponse openFestivalBoxResponse) =>
            {
                if (openFestivalBoxResponse.AddList != null && openFestivalBoxResponse.AddList.Count > 0)
                {
                    PlayGetRewardAnim(GameItemUtils.UnPackList(openFestivalBoxResponse.AddList).ToList());
                }
            });
        }
        private void OnClickSkipButton(BabuButton _)
        {
            ClearGetRewardAnim();
            EndLight(gameItemListSave);
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(gameItemListSave));
        }
        #endregion

        #region 抽奖动画

        private bool isAnimDoing = false;
        private Sequence seq = null;
        List<GameItem> gameItemListSave = new();
        private void PlayGetRewardAnim(List<GameItem> gameItemList)
        {
            skipButton.gameObject.SetActive(true);
            gameItemListSave = gameItemList;
            isAnimDoing = true;
            seq = DOTween.Sequence();
            for (int i = 0; i < 50; i++)
            {
                seq.AppendInterval(Mathf.Lerp(0.15f, 0.02f, i / (float)50));
                seq.AppendCallback(RandomLight);
            }
            seq.AppendCallback(() => { AllLight(true); });
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() => { EndLight(gameItemList); });
            seq.AppendInterval(2f);
            seq.AppendCallback(() =>
            {
                UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(gameItemList));
                ClearGetRewardAnim();
            });
        }
        private void ClearGetRewardAnim()
        {
            seq?.Kill();
            seq = null;
            isAnimDoing = false;
            skipButton.gameObject.SetActive(false);
        }

        private void AllLight(bool isLight)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                ChristmasTreeItem item = itemList[i];
                item.SetLight(isLight);
            }
        }
        private void RandomLight()//树上的物品随机亮起
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_SWITCH);
            for (int i = 0; i < itemList.Count; i++)
            {
                ChristmasTreeItem item = itemList[i];
                bool isLight = Utility.GetRandomInt(1, 4) == 1;
                item.SetLight(isLight);
            }
        }
        private void EndLight(List<GameItem> gameItemList)
        {
            AudioManager.Instance.PlaySound(AudioNames.EVENT_COMMONHIT);
            for (int i = 0; i < itemList.Count; i++)
            {
                bool isLight = false;
                ChristmasTreeItem item = itemList[i];
                foreach (var gameItem in gameItemList)
                {
                    if (GameItemUtils.Equals(item.gameItem, gameItem))
                    {
                        isLight = true;
                        break;
                    }
                }
                item.SetLight(isLight, true);
            }
        }

        #endregion

        #region 牌子移动

        private Sequence stickSeq = null;
        private float stickMidRotZ = 7.9f;
        public void StartMove()
        {
            stickSeq = DOTween.Sequence();
            stickSeq.AddTo(stickImage.gameObject);
            stickSeq.Append(stickImage.transform.DOLocalRotate(new Vector3(0, 0, stickMidRotZ + 2), 5f));
            stickSeq.AppendInterval(0.1f);
            stickSeq.Append(stickImage.transform.DOLocalRotate(new Vector3(0, 0, stickMidRotZ - 4), 5f));
            stickSeq.AppendInterval(0.1f);
            stickSeq.SetLoops(-1);
        }
        public void StopMove()
        {
            stickSeq?.Kill();
            stickSeq = null;
            stickImage.transform.SetLocalRotationZ(stickMidRotZ - 4);
        }

        #endregion

        #region 界面刷新
        protected override void OnPropertiesSet()
        {
            AudioManager.Instance.PlayMusic(AudioNames.CHRISTMAS_BG);
            StopMove();
            StartMove();
            ClearGetRewardAnim();
            RefreshActivityData();
            RefreshTreeItem();
            RefreshBatteryCount(null);
            RefreshLeftTime();
            RefreshRedDot(null);
        }

        private ActivityData activityData = null;
        private FestivalBoxConfig festivalBoxConfig = null;
        private void RefreshActivityData()
        {
            if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.ChristmasTree) == false)
            {
                Debug.LogWarning("ChristmasTreeUI , RefreshActivityData , ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.ChristmasTree) == false");
                return;
            }
            activityData = ActivityController.Instance.OnlineActivityDic[ActivityID.ChristmasTree];
            festivalBoxConfig = Configs.FestivalBox.GetConfig(ActivityID.ChristmasTree);
            if (festivalBoxConfig == null)
            {
                Debug.LogError("ChristmasTreeUI , RefreshTreeItem , festivalBoxConfig == null , ActivityID.ChristmasTree = {0}".SafeFormat(ActivityID.ChristmasTree));
                return;
            }
        }

        private void RefreshTreeItem()
        {
            List<BoxConfig> boxConfigList = Configs.Box.GetConfigList().Where<BoxConfig>(p => p.BoxId == festivalBoxConfig.BoxId).ToList();
            if (boxConfigList == null || boxConfigList.Count <= 0)
            {
                Debug.LogError("ChristmasTreeUI , RefreshTreeItem , boxConfigList == null || boxConfigList.Count <= 0 , festivalBoxConfig.BoxId = {0}".SafeFormat(festivalBoxConfig.BoxId));
                return;
            }
            for (int i = 0; i < itemList.Count; i++)
            {
                BoxConfig boxConfig = boxConfigList[i];
                ChristmasTreeItem item = itemList[i];
                GameItem gameItem = GameItemUtils.CreateGameItem((GameItemType)boxConfig.RewardType, boxConfig.RewardId, boxConfig.RewardNum);
                if (gameItem == null)
                {
                    Debug.LogError("ChristmasTreeUI , RefreshTreeItem , gameItem == null , boxConfig.RewardType = {0} , boxConfig.RewardId = {1} , boxConfig.RewardNum = {2} , festivalBoxConfig.BoxId = {3} , i = {4}".SafeFormat(boxConfig.RewardId, boxConfig.RewardId, boxConfig.RewardNum, festivalBoxConfig.BoxId, i));
                }
                item.SetData(gameItem);
            }
        }
        [SerializeField] private UIShiny onceBtnUIShiny = null;
        [SerializeField] private UIShiny threeBtnUIShiny = null;
        private void RefreshBatteryCount(object[] _)
        {
            GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, festivalBoxConfig.KeyId, 0);
            int batteryCount = gameItem.GetPlayerCount();
            batteryNumText.text = batteryCount.ToString();
            onceBtnUIShiny.enabled = batteryCount >= 1;
            threeBtnUIShiny.enabled = batteryCount >= 3;
        }
        private void RefreshLeftTime()
        {
            if (activityData == null) return;
            long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
            leftTimeText.text = "剩余时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        }
        private void RefreshRedDot(object[] _)
        {
            RedDotNode TaskRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Christmas, "/" + ActivityID.ChristmasTask);
            TaskRedDotNode.IsRed(dotNodeImg.transform);
        }
        #endregion
    }
}