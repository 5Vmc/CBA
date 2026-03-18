using Babu;
using BigBang.Animation;
using CBA;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class FirstPayPad : MonoBehaviour, IActivity
    {
        [SerializeField] private SkeletonGraphic spine;
        [SerializeField] private TMP_Text txtDesc;
        [SerializeField] private TMP_Text txtInfo;
        // [SerializeField] private List<InventoryBaseItem> skillList;
        [SerializeField] private List<InventoryItem> inventoryItem;
        [SerializeField] private List<Image> starList;
        [SerializeField] private BabuToggleGroup priceToggleGroup;
        [SerializeField] private GameObject infoObj;
        [SerializeField] private BabuButton btnPay;
        [SerializeField] private RectTransform _layout;
        [SerializeField] private List<Image> reddotList;
        [SerializeField] private Image reddotBtn;
        [SerializeField] private Image imgReward;
        [SerializeField] private Image imggetcard;
        [SerializeField] private BabuButton helpButton = null;
        [SerializeField] private Image hasGetImage = null;
        [SerializeField] private Image imgFirst = null;
        [SerializeField] private TMP_Text txtFirstPay = null;




        public ActivityClientType activityType = ActivityClientType.FirstPay;
        private int selectedIndex = 0;
        private List<ActivityPayRewardConfig> RewardsConfigList;
        private Timer timer;
        private ActivityData data;
        /// <summary>
        /// 0 充值， 1领取
        /// </summary>
        private int state;

        protected void Awake()
        {

        }

        Sequence oneSecondSeq;
        protected void OnEnable()
        {
            priceToggleGroup.OnValueChanged += OnToggleChanged;
            btnPay.OnClick += doPay;
            EventManager.Instance.Register(EventID.RefreshWindow, OnServerPushRefresh);
            helpButton.OnClick += OnClickHelpButton;

            dospine();
        }
        protected void OnDisable()
        {
            priceToggleGroup.OnValueChanged -= OnToggleChanged;
            btnPay.OnClick -= doPay;
            timer?.Cancel();
            EventManager.Instance.Unregister(EventID.RefreshWindow, OnServerPushRefresh);
            helpButton.OnClick -= OnClickHelpButton;
        }

        private void OnClickHelpButton(BabuButton obj)
        {
            int cardId = RewardsConfigList[0].Rewards.Split(':')[1].ToInt();
            var cfg = Configs.CardModel.GetConfig(cardId);
            if (cfg == null)
            {
                Debug.LogError("FirstPayPad , OnClickHelpButton , cfg == null , cardId = " + cardId);
                return;
            }
            string[] descs = RewardsConfigList[selectedIndex].Desc.Split("|");
            var starConfig = Configs.CardUpgrade.GetConfig(int.Parse(descs[1]));
            int star = 0;
            if (starConfig != null) star = starConfig.Star;
            UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(cardId, star));
            // UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(cfg));
        }

        private void doPay(BabuButton obj)
        {
            if (state == 1)
            {
                ActivityPayRewardConfig activityPayRewardConfig = RewardsConfigList[selectedIndex];
                ActivityController.Instance.GetRewards(data.cfg.Id, activityPayRewardConfig.Id, () =>
                {
                    data.payData.AddReceive(activityPayRewardConfig.Id);
                    List<GameItem> canGetGameItemList = GameItemUtils.CreateGameItems(activityPayRewardConfig.Rewards).ToList();
                    var properties = new InventoryObtainedUIProperties(canGetGameItemList);
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                });
            }
            else
            {
                TriggerManager.Instance.JumpPanel(TriggerModuleType.Shop_diamond);
            }
        }



        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            oldToggle?.GetComponent<StatusControl>().SetStatus(false);
            newToggle?.GetComponent<StatusControl>().SetStatus(true);
            selectedIndex = priceToggleGroup.EnableIndex;

            refreshData(selectedIndex);
        }

        private void OnServerPushRefresh(object[] objects)
        {
            if ((int)objects[0] != data.cfg.Id) return;
            SwitchToBestTab();
            refreshRedDot();
        }

        private void refreshData(int _selectIndex = 0)
        {
            string[] descs = RewardsConfigList[_selectIndex].Desc.Split("|");
            //txtName.text = descs[0];
            txtDesc.text = descs[2];
            var starConfigId = int.Parse(descs[1]);
            state = data.payData.TotalPay >= RewardsConfigList[_selectIndex].Option ? 1 : 2;

            //切换首充状态
            if (data.payData.HasReceive(RewardsConfigList[0].Id))
            {
                priceToggleGroup.gameObject.SetActive(true);
                imgFirst.gameObject.SetActive(false);
                imgReward.gameObject.SetActive(true);
            }
            else
            {
                priceToggleGroup.gameObject.SetActive(false);
                imgReward.gameObject.SetActive(false);
                imgFirst.gameObject.SetActive(true);
            }


            if (data.payData.HasReceive(RewardsConfigList[_selectIndex].Id))
            {
                state = 0;
            }
            if (state == 0)
            {
                hasGetImage.gameObject.SetActive(true);
                btnPay.gameObject.SetActive(false);
                txtInfo.gameObject.SetActive(false);
                reddotBtn.gameObject.SetActive(false);

            }
            else if (state == 1)
            {
                hasGetImage.gameObject.SetActive(false);
                btnPay.gameObject.SetActive(true);
                btnPay.GetComponentInChildren<TMP_Text>().text = "领  取";
                txtInfo.gameObject.SetActive(false);
                reddotBtn.gameObject.SetActive(true);
            }
            else
            {
                hasGetImage.gameObject.SetActive(false);
                btnPay.gameObject.SetActive(true);

                if (data.payData.TotalPay >= RewardsConfigList[0].Option)
                {
                    btnPay.GetComponentInChildren<TMP_Text>().text = "充  值";
                }
                else
                {
                    btnPay.GetComponentInChildren<TMP_Text>().text = "任意充值获得";
                }


                txtInfo.text = string.Format("再充值{0}元可领取", RewardsConfigList[_selectIndex].Option - data.payData.TotalPay);
                txtInfo.gameObject.SetActive(true);
                reddotBtn.gameObject.SetActive(false);
            }
            // if (starConfigId > 0) setSkillIcons(starConfigId);

            if (_selectIndex == 0)
            {
                SetRewards(null);
                imggetcard.gameObject.SetActive(true);
            }
            else
            {
                SetRewards(RewardsConfigList[_selectIndex].Rewards);
                imggetcard.gameObject.SetActive(false);
            }

            switch (_selectIndex)
            {
                case 0:
                    imgReward.rectTransform.anchoredPosition = new Vector3(-173f, 340f);
                    break;
                case 1:
                    imgReward.rectTransform.anchoredPosition = new Vector3(-90f, 340f);
                    break;
                case 2:
                    imgReward.rectTransform.anchoredPosition = new Vector3(83f, 340f);
                    break;
                case 3:
                    imgReward.rectTransform.anchoredPosition = new Vector3(160f, 340f);
                    break;
            }


            setStar(starConfigId);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_layout);
        }

        private void refreshRedDot()
        {
            var anyRed = false;
            for (var index = 0; index < RewardsConfigList.Count; index++)
            {
                var isRed = RewardsConfigList[index].Option <= data.payData.TotalPay && !data.payData.HasReceive(RewardsConfigList[index].Id);
                reddotList[index].gameObject.SetActive(isRed);
                anyRed |= isRed;
            }

            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + RewardsConfigList[0].ActivityId);
            node.AddValue(anyRed ? 1 : -1);
        }

        private async void setStar(int starConfigId)
        {
            var starConfig = Configs.CardUpgrade.GetConfig(starConfigId);
            if (starConfig != null)
            {
                int colorfulStarCount = starConfig.Star - 5;
                for (var index = 0; index < 5; index++)
                {
                    if (index > starConfig.Star - 1)
                    {
                        starList[index].gameObject.SetActive(false);
                    }
                    else
                    {
                        starList[index].gameObject.SetActive(true);
                        if (index + 1 <= colorfulStarCount)
                            starList[index].sprite = await SpriteProxy.GetColorfulStar();
                        else
                            starList[index].sprite = await SpriteProxy.GetYellowStar();
                    }
                }
            }
            else
            {
                for (var index = 0; index < 5; index++)
                {
                    starList[index].gameObject.SetActive(false);
                }
            }

        }

        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Transform layoutTrans;
        private void SetRewards(string rewardStr)
        {
            List<GameItem> gameItemList = new();
            if (!string.IsNullOrWhiteSpace(rewardStr))
            {
                gameItemList = GameItemUtils.CreateGameItems(rewardStr).ToList();
            }
            while (layoutTrans.childCount < gameItemList.Count) Instantiate(itemPrefab, layoutTrans);
            for (int i = 0; i < layoutTrans.childCount; i++)
            {
                if (i < gameItemList.Count)
                {
                    var reward = gameItemList[i];
                    var child = layoutTrans.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.transform.localScale = Vector3.one * 0.93f;

                    InventoryItem item = child.GetComponent<InventoryItem>();
                    item.SetData(reward);
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        private int GetSkillFireSection(GiftSkillConfig _cfg)
        {
            if (_cfg.Fire > 0)
            {
                if (_cfg.When == FActionTimeType.OnBattle)
                {
                    return 1;
                }
                else if (_cfg.When == FActionTimeType.OnSection)
                {
                    return _cfg.Wparam2;
                }
            }
            return 0;
        }

        private void dospine()
        {
            spine.Initialize(true);
            spine.AnimationState.SetAnimation(0, "play", false);

            timer = Timer.Register(this.gameObject, 1f, () => { infoObj.DOFade(1, 0.5f); });
        }

        public void LoadActivity(ActivityData _data)
        {
            RewardsConfigList = Configs.ActivityPayReward.GetConfigList().FindAll(p => p.ActivityId == _data.cfg.Id);
            infoObj.SetAlpha(0f);
            data = _data;

            SwitchToBestTab();
            refreshRedDot();
        }

        private void SwitchToBestTab()
        {
            int needIndex = -1;
            for (var index = 0; index < RewardsConfigList.Count; index++)//先选可领取的
            {
                var isRed = RewardsConfigList[index].Option <= data.payData.TotalPay && !data.payData.HasReceive(RewardsConfigList[index].Id);
                if (isRed)
                {
                    needIndex = index;
                    break;
                }
            }
            if (needIndex == -1)//再选可以充值的
            {
                for (var index = 0; index < RewardsConfigList.Count; index++)
                {
                    var isCanCharge = RewardsConfigList[index].Option > data.payData.TotalPay;
                    if (isCanCharge)
                    {
                        needIndex = index;
                        break;
                    }
                }
            }
            if (needIndex == -1) needIndex = RewardsConfigList.Count - 1;//没有就最后一个

            priceToggleGroup.Switch(needIndex);

        }


    }
}