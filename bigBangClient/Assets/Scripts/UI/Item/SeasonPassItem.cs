using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using GameConfig.Config;
using Babu;
using static BigBang.ClassicManager;
using Utils.GameItem;

namespace BigBang.UI
{
    public enum SeasonPassItemState
    {
        Unknow,
        CanGet,
        Finish,
        Lock
    }

    public class SeasonPassItem : MonoBehaviour
    {
        [SerializeField] private Image freeDarkBgImage = null;
        [SerializeField] private Image freeLightBgImage = null;
        [SerializeField] private InventoryItem inventoryItemFree = null;
        [SerializeField] private BabuButton freeGetButton = null;
        [SerializeField] private Image payDarkBgImage = null;
        [SerializeField] private Image payLightBgImage = null;
        [SerializeField] private InventoryItem inventoryItemPass = null;
        [SerializeField] private InventoryItem inventoryItemPassSpecial = null;
        [SerializeField] private BabuButton payGetButton = null;
        [SerializeField] private Image normalProgressFgImage = null;
        [SerializeField] private Image normalProgressBgImage = null;
        [SerializeField] private Image firstProgressFgImage = null;
        [SerializeField] private Image firstProgressBgImage = null;
        [SerializeField] private Image midDarkImage = null;
        [SerializeField] private Image midLightImage = null;
        [SerializeField] private Image imgEnable1 = null;
        [SerializeField] private Image imgEnable2 = null;
        [SerializeField] private bool bigRewards = false;

        public ActivityBattlePassRewardConfig config;

        private void OnEnable()
        {
            freeGetButton.OnClick += OnClickFreeGetButton;
            payGetButton.OnClick += OnClickPayGetButton;
        }

        private void OnDisable()
        {
            freeGetButton.OnClick -= OnClickFreeGetButton;
            payGetButton.OnClick -= OnClickPayGetButton;
        }

        public int index = -1;
        bool isFirst = false;
        bool isEnd = false;
        public void SetData(ActivityBattlePassRewardConfig config, int index)
        {
            EventManager.Instance.Dispatch(EventID.OnSeasonPassItemSetData, this);
            this.config = config;
            this.index = index;
            SetLevelTxt();
            SetInventoryItem(inventoryItemFree, config.Rewards1);
            SetInventoryItem(inventoryItemPass, config.Rewards2);
            SetInventoryItem(inventoryItemPassSpecial, config.RewardsStep);
            isEnd = index == -1;
            isFirst = index == 0;
            normalProgressFgImage.gameObject.SetActive(!isEnd && !isFirst);
            normalProgressBgImage.gameObject.SetActive(!isEnd && !isFirst);
            firstProgressFgImage.gameObject.SetActive(!isEnd && isFirst);
            firstProgressBgImage.gameObject.SetActive(!isEnd && isFirst);
        }

        [SerializeField] private TMP_Text midLevelNumText = null;
        [SerializeField] private List<TMP_Text> midLevelNumShadowList = null;
        private void SetLevelTxt()
        {
            midLevelNumText.text = (config.Id % 100).ToString();
            foreach (var midLevelNumShadow in midLevelNumShadowList)
            {
                midLevelNumShadow.text = midLevelNumText.text;
            }
        }
        private void SetInventoryItem(InventoryItem inventoryItem, string rewardStr)
        {
            if (string.IsNullOrEmpty(rewardStr))
            {
                inventoryItem.gameObject.SetActive(false);
            }
            else
            {
                inventoryItem.SetData(GameItemUtils.CreateGameItem(rewardStr));
                inventoryItem.gameObject.SetActive(true);
            }
        }

        private void SetInventoryGetState(InventoryItem inventoryItem, bool hasGet)
        {
            inventoryItem.SetBlack(hasGet);
            inventoryItem.transform.GetComponentAtPath<RectTransform>("Views/RewardGetImage").gameObject.SetActive(hasGet);
        }

        private void OnClickFreeGetButton(BabuButton sender)
        {
            SendGetReward(true);
        }
        private void OnClickPayGetButton(BabuButton sender)
        {
            SendGetReward(false);
        }
        private void SendGetReward(bool isFree)
        {
            SeasonPassItem seasonPassItem = this;
            int sendId = seasonPassItem.config.Id * 10 + (isFree ? 1 : 2);
            ActivityController.Instance.GetRewards(seasonPassItem.config.ActivityId, sendId, () =>
            {
                activityData.payData.AddReceive(sendId);
                List<GameItem> gameItemList = new();
                if (isFree)
                {
                    if (string.IsNullOrEmpty(seasonPassItem.config.Rewards1) == false) gameItemList.Add(GameItemUtils.CreateGameItem(seasonPassItem.config.Rewards1));
                }
                else
                {
                    if (string.IsNullOrEmpty(seasonPassItem.config.Rewards2) == false) gameItemList.Add(GameItemUtils.CreateGameItem(seasonPassItem.config.Rewards2));
                    if (string.IsNullOrEmpty(seasonPassItem.config.RewardsStep) == false) gameItemList.Add(GameItemUtils.CreateGameItem(seasonPassItem.config.RewardsStep));
                }
                var properties = new InventoryObtainedUIProperties(gameItemList, () =>
                {
                    if (isLockByPurchase && isFree && string.IsNullOrEmpty(seasonPassItem.config.RewardsStep) == false)
                    {
                        UIController.Instance.OpenWindow<SeasonPassPreviewUI>(new SeasonPassPreviewUIProperties(this.activityData));
                    }
                });
                EventManager.Instance.Dispatch(EventID.OnSeasonPassItemGetReward, this);
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            });
        }

        bool isLockByLevel = true;
        bool freeHasRecieve = true;
        bool payHasRecieve = true;
        bool isLockByPurchase = true;
        public bool freeCanGet = false;
        public bool payCanGet = false;
        float progress = 0.0f;
        bool freeHasGoods = true;
        bool payHasGoods = true;
        private ActivityData activityData;
        public void RefreshState(ActivityData activityData)
        {
            this.activityData = activityData;
            int taskPoint = activityData.payData.TaskPoint;
            isLockByPurchase = !activityData.payData.hasBuy;
            isLockByLevel = taskPoint < config.Option;
            freeHasGoods = string.IsNullOrEmpty(config.Rewards1) == false;
            freeHasRecieve = activityData.payData.HasReceive(config.Id * 10 + 1);
            freeCanGet = !isLockByLevel && !freeHasRecieve && freeHasGoods;
            freeGetButton.gameObject.SetActive(freeCanGet);
            freeDarkBgImage.gameObject.SetActive(isLockByLevel);
            freeLightBgImage.gameObject.SetActive(!isLockByLevel);
            payHasGoods = string.IsNullOrEmpty(config.Rewards2) == false || string.IsNullOrEmpty(config.RewardsStep) == false;
            payHasRecieve = activityData.payData.HasReceive(config.Id * 10 + 2);
            payCanGet = !isLockByLevel && !payHasRecieve && !isLockByPurchase && payHasGoods;
            payGetButton.gameObject.SetActive(payCanGet);
            payDarkBgImage.gameObject.SetActive(isLockByLevel || isLockByPurchase);
            payLightBgImage.gameObject.SetActive(!isLockByLevel && !isLockByPurchase);
            midDarkImage.gameObject.SetActive(isLockByLevel);
            midLightImage.gameObject.SetActive(!isLockByLevel);
            SetInventoryGetState(inventoryItemFree, freeHasRecieve);
            SetInventoryGetState(inventoryItemPass, payHasRecieve);
            SetInventoryGetState(inventoryItemPassSpecial, payHasRecieve);

            imgEnable1.gameObject.SetActive(!bigRewards && isLockByLevel);
            imgEnable2.gameObject.SetActive(!bigRewards && (isLockByPurchase || isLockByLevel));

            if (!isEnd)
            {
                if (taskPoint < config.Exp)
                {
                    progress = 0;
                }
                else
                {
                    progress = (taskPoint - config.Exp) / (float)(config.Option - config.Exp);
                }
                if (!isFirst)
                {
                    normalProgressFgImage.fillAmount = progress;
                }
                else
                {
                    firstProgressFgImage.fillAmount = progress;
                }
            }
        }
        [SerializeField] public RectTransform payFingerPos = null;
        [SerializeField] public RectTransform freeFingerPos = null;

    }
}
