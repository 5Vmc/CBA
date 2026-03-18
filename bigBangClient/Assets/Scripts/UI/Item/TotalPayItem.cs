
//SpriteProxy.GetInvetoryQuality(cfg.Quality);

using Babu;
using GameConfig;
using GameConfig.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class TotalPayItemData
    {
        public bool isFree = false;
        public ActivityPayRewardConfig cfg;
        /// <summary>
        /// 2 可领取， 1不可领取，0已领取
        /// </summary>
        public int state;
        public int money;
    }

    public class TotalPayItem : MonoBehaviour
    {
        [SerializeField] private BabuButton btnGet;
        [SerializeField] private TMP_Text priceTxt;
        [SerializeField] private TMP_Text btnGetTxt;
        [SerializeField] private Image imgTitleBg;
        [SerializeField] private List<InventoryItem> rewardsList;
        [SerializeField] private Image hasGetImage = null;
        [SerializeField] private Image BgImage = null;

        private TotalPayItemData data;
        protected void OnEnable()
        {
            btnGet.OnClick += GetReward;
        }

        protected void OnDisable()
        {
            btnGet.OnClick -= GetReward;
        }


        private void GetReward(BabuButton sender)
        {
            if (data.isFree)
            {
                if (ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id))
                {
                    Tips.PopTips("该奖励已被领取，请明日再来");
                    return;
                }
                NetworkManager.Instance.ReceiveDailyGift(activityData.cfg.Id, (resp) =>
                {
                    if (resp.ReceiveSucceed == false)
                    {
                        Tips.PopTips("领取失败");
                        Debug.LogWarningFormat("TotalPayItem , GetReward , resp.ReceiveSucceed == false , activityData.cfg.Id = {0}", activityData.cfg.Id);
                        return;
                    }
                    ActivityController.Instance.dailyGiftReceivedActivityIdSet.Add(activityData.cfg.Id);
                    var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList());
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                    ActivityController.Instance.RefreshRedDot(activityData);
                    EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                });
                return;
            }
            if (data.state == 2)
            {
                ActivityController.Instance.GetRewards(data.cfg.ActivityId, data.cfg.Id, () =>
                {
                    activityData.payData.AddReceive(data.cfg.Id);
                    var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(data.cfg.Rewards).ToList());
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                });
            }
            else if (data.state == 1)
            {
                TriggerManager.Instance.JumpPanel(TriggerModuleType.Shop_diamond);
            }
        }

        private ActivityData activityData;
        public async void SetData(TotalPayItemData _data, int itemIndex, ActivityData activityData)
        {
            data = _data;
            this.activityData = activityData;
            setImage(activityData);

            if (data.isFree)
            {
                SpriteManager.GetSprite(AtlasNames.Activity, "bgdays", s => imgTitleBg.sprite = s);
                ColorUtility.TryParseHtmlString("#fdfad4", out Color color);
                priceTxt.color = color;
                priceTxt.text = activityData.cfg.DailyGiftDesc;
            }
            else
            {
                SpriteManager.GetSprite(AtlasNames.Activity, "bgblue", s => imgTitleBg.sprite = s);
                ColorUtility.TryParseHtmlString("#184448", out Color color);
                priceTxt.color = color;
                priceTxt.text = data.cfg.Desc.SafeFormat(_data.money, _data.cfg.Option);
            }


            if (_data.state == 2)
            {
                ColorUtility.TryParseHtmlString("#936508", out Color color);
                btnGetTxt.color = color;
                btnGetTxt.text = "领  取";
                btnGet.GetComponent<Image>().sprite = await SpriteProxy.YellowBtnEnable;
                btnGet.gameObject.SetActive(true);
                hasGetImage.gameObject.SetActive(false);
            }
            else if (_data.state == 1)
            {
                ColorUtility.TryParseHtmlString("#184448", out Color color);
                btnGetTxt.color = color;
                btnGetTxt.text = "充  值";
                btnGet.GetComponent<Image>().sprite = await SpriteProxy.YellowSmallBtnDisable;
                btnGet.gameObject.SetActive(true);
                hasGetImage.gameObject.SetActive(false);
            }
            else
            {
                ColorUtility.TryParseHtmlString("#b7bfc5", out Color color);
                btnGetTxt.color = color;
                btnGetTxt.text = "已领取";
                btnGet.gameObject.SetActive(false);
                hasGetImage.gameObject.SetActive(true);
            }

            List<GameItem> gameItemList = null;
            if (data.isFree)
            {
                gameItemList = GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList();
            }
            else
            {
                gameItemList = GameItemUtils.CreateGameItems(_data.cfg.Rewards).ToList();
            }
            var rewardsItemsCount = gameItemList.Count;

            for (int index = 0; index < rewardsList.Count; index++)
            {
                if (rewardsItemsCount <= index)
                {
                    rewardsList[index].gameObject.SetActive(false);
                }
                else
                {
                    rewardsList[index].gameObject.SetActive(true);
                    rewardsList[index].SetGameItemData(gameItemList[index]);
                }
            }
        }

        private async void setImage(ActivityData activityData)
        {
            BgImage.sprite = await SpriteProxy.GetFestivalImg(activityData.cfg.Param1, "totalPaybg");
            hasGetImage.sprite = await SpriteProxy.GetFestivalImg(activityData.cfg.Param1, "img_517_4");
        }
    }
}