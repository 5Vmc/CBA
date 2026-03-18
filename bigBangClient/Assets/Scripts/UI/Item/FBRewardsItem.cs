using Babu;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class FBRewardsItemData
    {
        public int ShopItemId;
        /// <summary>
        /// 领取状态，-1:已经领取， 0:未达到条件。 1:可领取
        /// </summary>
        public int Status;

        public TowerStarRewardConfig cfg;
        public FBRewardsItemData(TowerStarRewardConfig _cfg)
        {
            cfg = _cfg;
        }
    }

    public class FBRewardsItem : MonoBehaviour
    {
        [SerializeField] private List<InventoryItem> rewardsItems;
        [SerializeField] private InventoryItem prefab;
        [SerializeField] private TMP_Text NameTxt;
        [SerializeField] private TMP_Text LimitTxt;
        [SerializeField] private BabuButton costBtn;
        [SerializeField] private TMP_Text CostTxt;
        [SerializeField] private GameObject slotContainer;


        public FBRewardsItemData data;

        private void OnEnable()
        {
            costBtn.OnClick += OnCost;
        }

        private void OnDisable()
        {
            costBtn.OnClick -= OnCost;
        }

        public async void SetData(FBRewardsItemData _data)
        {
            this.data = _data;
            bindRewardsData();

            NameTxt.text = _data.cfg.Number.ToString() + "星奖励";
            if (_data.Status == 0)
            {
                CostTxt.text = "领 取";
                LimitTxt.text = "星数达到" + FBTowerController.Instance.FBData.totalStar.ToString() + "/" + _data.cfg.Number.ToString() + "可领取";
                costBtn.interactable = false;
                costBtn.image.sprite = await SpriteProxy.YellowBtnDisable;
            }
            else if (_data.Status == -1)
            {
                LimitTxt.text = "";
                costBtn.interactable = false;
                CostTxt.text = "已领取";
                costBtn.image.sprite = await SpriteProxy.YellowBtnDisable;
            }
            else
            {
                LimitTxt.text = "";
                costBtn.interactable = true;
                CostTxt.text = "领 取";
                costBtn.image.sprite = await SpriteProxy.YellowBtnEnable;
            }

        }

        /// <summary>
        /// 动态绑定奖励
        /// </summary>
        private void bindRewardsData()
        {
            var rewards = GameItemUtils.CreateGameItems(data.cfg.Reward).ToList();
            var children = slotContainer.GetComponentsInChildren<InventoryItem>();
            int slotCount = children.Length;
            int rewardCount = rewards.Count;
            int counter = System.Math.Max(slotCount, rewardCount);

            for (int index = 0; index < counter; index++)
            {
                InventoryItem item;
                if (index > slotCount - 1)
                {
                    item = Instantiate<InventoryItem>(prefab, slotContainer.transform);
                }
                else
                {
                    item = children[index];
                }

                if (index > rewardCount - 1)
                {
                    item.gameObject.SetActive(false);
                }
                else
                {
                    item.gameObject.SetActive(true);
                    item.SetGameItemData(rewards[index]);
                }
            }
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        /// <param name="sender"></param>
        private void OnCost(BabuButton sender)
        {
            EventManager.Instance.Dispatch(EventID.OnClickFBTowerGetStarReward, this);
        }
    }
}