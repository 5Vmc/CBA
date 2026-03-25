using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using GameConfig;
using Utils.GameItem;
using Google.Protobuf.Collections;
using System.Linq;
using Babu.Config;
using BigBang.Animation;
using TMPro;
using UnityEngine.UI;
using Utils;
using GameConfig.Config;

namespace BigBang.UI
{
    //[System.Serializable]
    //public class SevenDaysLoginUIProperties : WindowProperties
    //{
    //    public int ActivityId;
    //    public EActivityType ActivityType;
    //    public int SkinId;
    //    public SevenDaysLoginUIProperties(int activityId = ActivityID.SevenDaysLogin)
    //    {
    //        ActivityId = activityId;
    //        ActivityType = (EActivityType)((int)activityId / 1000);
    //        SkinId = ActivityType == EActivityType.Sign7Day ? 1 : 2;
    //    }
    //}

    public class SevenDaysLoginUI : MonoBehaviour, IActivityClient
    {
        //领取奖励按钮
        [SerializeField] private BabuButton obtainBtn;
        [SerializeField] private TMP_Text obtainTxt;
        [SerializeField] private TMP_Text titleTxt;
        [SerializeField] private List<SevenDayRewardItem> itemList;
        [SerializeField] private Sprite enableSprite;
        [SerializeField] private Sprite disableSprite;
        [SerializeField] private Image BackgroundImg;

        [SerializeField] private Color disableTextColor = new(); //1b2a34
        [SerializeField] private Color enableTextColor = new(); //(148 / 255f, 100 / 255f, 8 / 255f, 1);

        [SerializeField] private Color enableBtnColor = new(); //ffffff
        [SerializeField] private Color disableBtnCommonColor = new(); //暗蓝
        [SerializeField] private Color disableBtnFestivalColor = new(); //暗红

        private List<GameItem> obtainList = new List<GameItem>();

        [SerializeField] public SevenDaysLoginUIAnim Anim;

        private void OnEnable()
        {
            obtainBtn.OnClick += OnReceive;
        }
        private void OnDisable()
        {
            obtainBtn.OnClick -= OnReceive;
        }

        private ActivityConfig activityConfig;
        private ActivityClientType activityType;
        private int SkinId;
        public void LoadActivityClient(ActivityConfig activityConfig)
        {
            activityType = (ActivityClientType)activityConfig.ClientType;
            SkinId = activityType == ActivityClientType.Sign7Day ? 1 : 2;

            setImage();

            if (activityType == ActivityClientType.Sign7Day)
            {
                titleTxt.text = "七日签到";
                var list = itemList.Zip(Configs.SevenDayLoginReward.GetConfigList(), (view, cfg) => (view, cfg));
                int index = 0;
                foreach (var (view, cfg) in list)
                {
                    index++;
                    var gameItem = GameItemUtils.CreateGameItem(cfg.Content);
                    // 第2个和最后7个特殊处理
                    view.SetData(gameItem, activityType, activityConfig.Id, index, index == 7 || index == 2);
                }
            }
            else
            {
                titleTxt.text = "节日签到";
                var list = itemList.Zip(Configs.FestivalLogin.GetConfigList().FindAll(p => p.ActivityId == activityConfig.Id), (view, cfg) => (view, cfg));
                int index = 0;
                foreach (var (view, cfg) in list)
                {
                    index++;
                    var gameItem = GameItemUtils.CreateGameItem(cfg.Content);
                    // 第2个和最后7个特殊处理
                    view.SetData(gameItem, activityType, activityConfig.Id, index, index == 7 || index == 2);
                }
            }
            RefreshReward(false);
            Anim.PlayEnter();
        }

        private async void setImage()
        {
            BackgroundImg.sprite = await SpriteProxy.GetFestivalImg(SkinId, "img_691");
        }

        //private void OnClose(BabuButton sender)
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_BACKBG);
        //    Anim.PlayExit(() =>
        //    {
        //        UIController.Instance.CloseWindow<SevenDaysLoginUI>();
        //    });
        //}

        private void OnReceive(BabuButton sender)
        {
            if (obtainable == false)
            {
                Tips.PopError("今日奖励已领取，请明日再来");
                return;
            }
            if (activityType == ActivityClientType.Sign7Day)
            {
                NetworkManager.Instance.ReceiveSevenDayReward(true, response =>
                {
                    if (response.ReceiveSucceed)
                    {
                        ShowPad(response.ReceiveList.ToList());
                    }
                    RefreshState(response.ReceiveList.ToList());
                });
            }
            else
            {
                var data = ActivityController.Instance.OnlineActivityDic[activityConfig.Id];
                var rewardConfig = Configs.FestivalLogin.GetConfigList().FirstOrDefault(p =>
                    p.Option <= data.payData.TotalPay && !data.payData.HasReceive(p.Id)
                );
                if (rewardConfig == null) return;
                ActivityController.Instance.GetRewards(activityConfig.Id, rewardConfig.Id, () =>
                {
                    data.payData.AddReceive(rewardConfig.Id);
                    var receivedList = new List<int>() { rewardConfig.Id };
                    ShowPad(receivedList);
                    RefreshState(receivedList);
                });
            }
        }

        private void ShowPad1(List<int> receiveList)
        {
            obtainList.Clear();
            Player.ActivityManager.SetIsSevenSignRedDot(SkinId != 1);

            foreach (var item in receiveList)
            {
                var cfg = Configs.FestivalLogin.GetConfig(item);
                obtainList.Add(GameItemUtils.CreateGameItem(cfg.Content));
            }
            var properties = new InventoryObtainedUIProperties(obtainList, () =>
            {
                RefreshReward(true);
                if (obtainList.Exists(item => item.Type == GameItemType.Card))
                {
                    GameManager.Instance.TrigIosShopReview();
                }
            });
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
        }

        private void ShowPad(List<int> receiveList)
        {
            obtainList.Clear();
            Player.ActivityManager.SetIsSevenSignRedDot(SkinId != 1);
            foreach (var item in receiveList)
            {
                if (SkinId == 1)
                {
                    var cfg = Configs.SevenDayLoginReward;
                    obtainList.Add(GameItemUtils.CreateGameItem(cfg.GetConfig(item).Content));

                }
                else
                {
                    var cfg = Configs.FestivalLogin;
                    obtainList.Add(GameItemUtils.CreateGameItem(cfg.GetConfig(item).Content));
                }
            }
            var properties = new InventoryObtainedUIProperties(obtainList, () =>
            {
                RefreshReward(true);
                if (obtainList.Exists(item => item.Type == GameItemType.Card))
                {
                    GameManager.Instance.TrigIosShopReview();
                }
            });
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
        }



        //刷新所有物品的数据和状态
        private void RefreshState(List<int> refreshStateList)
        {
            if (activityType == ActivityClientType.Sign7Day)
            {
                foreach (var reward in refreshStateList)
                {
                    foreach (var item in Player.ActivityManager.SevenRewardList)
                    {
                        if (item.GetID() == reward)
                        {
                            item.SetValue(reward, (int)RewardStates.RECEIVED);
                        }
                    }
                }
            }
            else
            {

            }
        }

        //刷新所有物品的数据和状态
        private void RefreshReward(bool anim)
        {
            if (activityType == ActivityClientType.Sign7Day)
            {
                RefreshAsCommonSevenDay();
            }
            else if (activityType == ActivityClientType.NationalDayLogin)
            {
                RefreshAsFestivalSevenDay();
            }
        }

        private void RefreshAsCommonSevenDay()
        {
            var list = itemList.Zip(Player.ActivityManager.SevenRewardList, (view, state) => (view, state));
            foreach (var (view, state) in list)
            {
                if (state.GetState() == (int)RewardStates.RECEIVED)
                {
                    view.SetAsCompleted();
                    view.IsCompleted = true;

                }
                else if (state.GetState() == (int)RewardStates.COLLECT)
                {
                    view.SetAsObtainable();
                    view.IsCompleted = false;
                }
                else
                {
                    view.SetAsNormal();
                    view.IsCompleted = false;
                }
            }
            // 是否可领取
            obtainable = Player.ActivityManager.SevenRewardList.Any(item => item.GetState() == (int)RewardStates.COLLECT);
            //obtainBtn.interactable = obtainable;
            obtainBtn.image.sprite = obtainable ? enableSprite : disableSprite;
            obtainBtn.image.color = obtainable ? enableBtnColor : disableBtnCommonColor;
            obtainTxt.color = obtainable ? enableTextColor : disableTextColor;
        }

        private bool obtainable = false;
        private void RefreshAsFestivalSevenDay()
        {
            var data = ActivityController.Instance.OnlineActivityDic[activityConfig.Id];
            var list = itemList.Zip(Configs.FestivalLogin.GetConfigList().FindAll(p => p.ActivityId == data.cfg.Id), (view, state) => (view, state));

            obtainable = false;
            foreach (var (view, state) in list)
            {
                if (data.payData.HasReceive(state.Id))
                {
                    view.SetAsCompleted();
                    view.IsCompleted = true;

                }
                else if (data.payData.TotalPay >= state.Option)
                {
                    view.SetAsObtainable();
                    view.IsCompleted = false;
                    //任意可领但是还没有领
                    obtainable = true;
                }
                else
                {
                    view.SetAsNormal();
                    view.IsCompleted = false;
                }
            }
            // 是否可领取
            //obtainBtn.interactable = obtainable;
            obtainBtn.image.sprite = obtainable ? enableSprite : disableSprite;
            obtainBtn.image.color = obtainable ? enableBtnColor : disableBtnFestivalColor;
            obtainTxt.color = obtainable ? enableTextColor : disableTextColor;
        }
    }
}
