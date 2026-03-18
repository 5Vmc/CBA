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
using Protocol;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;
using System.Linq;
using System;
using DG.Tweening;

namespace BigBang.UI
{
    public enum EquipRouteItemState
    {
        Unknow,
        Open,
        Select,
        Lock
    }

    public class EquipRouteItem : MonoBehaviour
    {
        [SerializeField] private List<ClassicMapLevelTabItem> starList = new();
        [SerializeField] private TMP_Text clubNameText;
        [SerializeField] private TMP_Text challengeTipText;
        [SerializeField] private TMP_Text txtbtnTen;
        [SerializeField] private TMP_Text txtbtnOne;


        [SerializeField] private BabuButton btnOnce;
        [SerializeField] private BabuButton btnTen;
        [SerializeField] private BabuButton btnChallenge;
        [SerializeField] private ScrollRect scroll;

        public PassData data;
        public EquipRouteItemState classicTeamItemState = EquipRouteItemState.Unknow;


        private ChallengeClubConfig config;
        private int leftTimes;
        private int itemid;

        private void OnEnable()
        {
            btnOnce.OnClick += FastBattle;
            btnTen.OnClick += FastBattleTen;
            btnChallenge.OnClick += JumpPanel;
        }

        private void OnDisable()
        {
            btnOnce.OnClick -= FastBattle;
            btnTen.OnClick -= FastBattleTen;
            btnChallenge.OnClick -= JumpPanel;
        }

        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private HorizontalAdapter rewardLayout;

        [SerializeField] private RectTransform costOnce = null;
        [SerializeField] private TMP_Text energyCostValueOnce = null;
        [SerializeField] private RectTransform costTen = null;
        [SerializeField] private TMP_Text energyCostValueTen = null;

        [SerializeField] private Color energyCostValueNormal;
        [SerializeField] private Color energyCostValueCanNotUse;

        public async void SetData(PassData data, int itemid = 0)
        {
            this.data = data;
            config = Configs.ChallengeClub.GetConfig(data.Id);
            int stars = data.Stars.Sum();
            for (int i = 0; i < 3; i++)
            {
                starList[i].SetLight(i < stars);
            }

            clubNameText.text = config.Name;

            leftTimes = config.Count - data.ChallengeTimes;
            if (leftTimes > 0)
            {
                challengeTipText.text = "今日还可挑战<color=#3ee764>{0}</color>次".SafeFormat(leftTimes);
            }
            else
            {
                challengeTipText.text = "今日还可挑战<color=#d60b0b>0</color>次";
            }

            RefreshEnergy();

            txtbtnTen.text = string.Format("挑战{0}次", Math.Min(10, leftTimes));
            txtbtnOne.text = string.Format("挑战{0}次", Math.Min(1, leftTimes));
            SetRewards(config.Reward, rewardLayout, "", itemid);

            if (stars == 3 && leftTimes > 0)
            {
                btnOnce.gameObject.SetActive(true);
                btnTen.gameObject.SetActive(true);
                costOnce.gameObject.SetActive(true);
                costTen.gameObject.SetActive(true);
                btnChallenge.gameObject.SetActive(false);
                btnOnce.image.sprite = await SpriteProxy.YellowSmallBtnEnable;
                btnTen.image.sprite = await SpriteProxy.YellowSmallBtnEnable;
            }
            else
            {
                if (stars < 3)
                {
                    btnChallenge.gameObject.SetActive(true);
                    btnOnce.gameObject.SetActive(false);
                    btnTen.gameObject.SetActive(false);
                    costOnce.gameObject.SetActive(false);
                    costTen.gameObject.SetActive(false);
                }
                else
                {
                    btnChallenge.gameObject.SetActive(false);
                    btnOnce.gameObject.SetActive(true);
                    btnTen.gameObject.SetActive(true);
                    costOnce.gameObject.SetActive(true);
                    costTen.gameObject.SetActive(true);
                }
                btnOnce.image.sprite = await SpriteProxy.YellowSmallBtnDisable;
                btnTen.image.sprite = await SpriteProxy.YellowSmallBtnDisable;
            }
        }
        public void RefreshEnergy()
        {
            energyCostValueOnce.text = "-{0}".SafeFormat(GameConst.BattleEnergy);
            energyCostValueOnce.color = Player.PackageManager.Energy >= GameConst.BattleEnergy ? energyCostValueNormal : energyCostValueCanNotUse;
            energyCostValueTen.text = "-{0}".SafeFormat(GameConst.BattleEnergy * 10);
            energyCostValueTen.color = Player.PackageManager.Energy >= GameConst.BattleEnergy * 10 ? energyCostValueNormal : energyCostValueCanNotUse;
        }

        private void SetRewards(string rewardStr, HorizontalAdapter layout, string rewardStr2 = null, int _itemid = 0)
        {
            itemid = _itemid;
            Transform layoutTrans = layout.transform;
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(rewardStr).ToList();
            //显示不下，第1个是玩家经验, 第2个是球员经验，就不显示了。

            if (string.IsNullOrWhiteSpace(rewardStr2) == false)
            {
                List<GameItem> gameItemList2 = GameItemUtils.CreateGameItems(rewardStr2).ToList();
                gameItemList.AddRange(gameItemList2);
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
                    item.canShowTip = true;
                    item.SetGameItemViews(reward);
                    item.SetGameItemData(reward);

                    if (_itemid != 0 && _itemid == reward.Id)
                    {
                        item.ShowSelectBorder();
                    }
                    else
                    {
                        item.HidSelectBorder();
                    }

                    //item.SetImageAndCount(reward.GetIcon(), reward.CountString());
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
            layout.Calculate();
            scroll.horizontalNormalizedPosition = 1;
        }

        private void FastBattle(BabuButton obj)
        {
            FastChallenge(1);
        }

        private void FastBattleTen(BabuButton obj)
        {
            //todo 挑战10次
            FastChallenge(leftTimes < 10 ? leftTimes : 10);
        }

        private void JumpPanel(BabuButton obj)
        {
            //todo: 这里关闭了当前面板，因为回退的时候需要刷新本面板，没时间搞；
            UIController.Instance.CloseWindow<EquipRouteUI>();
            UIController.Instance.HidePanel<CardUpUI>();
            UIController.Instance.HidePanel<CardUI>();
            ClassicManager.Instance.RouteToCountryPanel(config.Country, config.Id, itemid);
        }

        private void FastChallenge(int times)
        {
            if (leftTimes <= 0)
            {
                Tips.PopTips("今日挑战次数耗尽");
                return;
            }

            ClassicManager.Instance.FastChallenge(data.Id, times, true, (challengeTimes) =>
            {
                SetData(data);
                EventManager.Instance.Dispatch(EventID.RefreshWindow, 0);
            });
        }
    }
}
