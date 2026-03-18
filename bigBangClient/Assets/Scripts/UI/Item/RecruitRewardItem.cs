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
    public class RecruitRewardItemData
    {
        public int ShopItemId;
        /// <summary>
        /// 领取状态，-1:已经领取， 0:未达到条件。 1:可领取
        /// </summary>
        public int Status;

        public OptionRewardsConfig cfg;
        public List<GoodsConfig> goodCfgList;
        public List<GameItem> gameItemList;
        public RecruitRewardItemData(OptionRewardsConfig _cfg)
        {
            cfg = _cfg;
            gameItemList = GameItemUtils.CreateGameItems(cfg.Rewards).ToList();
            goodCfgList = new List<GoodsConfig>();
            foreach (GameItem item in gameItemList)
                goodCfgList.Add(Configs.Goods.GetConfig(item.Id));
        }
    }

    public class RecruitRewardItem : MonoBehaviour
    {
        [SerializeField] private InventoryItem inv_Item;
        [SerializeField] private TMP_Text txtItemName;
        [SerializeField] private TMP_Text txtItemDesc;
        [SerializeField] private TMP_Text txtItemLimit;
        [SerializeField] private BabuButton costBtn;
        [SerializeField] private TMP_Text price;


        public RecruitRewardItemData data;

        private void OnEnable()
        {
            costBtn.OnClick += OnCost;
        }

        private void OnDisable()
        {
            costBtn.OnClick -= OnCost;
        }

        public async void SetData(RecruitRewardItemData _data)
        {
            //这里只有1个道具
            this.data = _data;

            inv_Item.SetData(_data.gameItemList[0]);
            txtItemName.text = _data.gameItemList[0].GetName();
            txtItemDesc.text = _data.gameItemList[0].GetDescription();
            txtItemName.color = CBAColorUtil.Instance.GetColor(_data.gameItemList[0].GetQuality());
            if (_data.Status == 0)
            {
                price.text = "领 取";
                txtItemLimit.text = "招募" + Player.CardManager.RecruitController.GetPool(_data.cfg.Pool).TotalCount + "/" + _data.cfg.Option.ToString() + "次后可领取";
                costBtn.interactable = false;
                costBtn.image.sprite = await SpriteProxy.YellowBtnDisable;
            }
            else if (_data.Status == -1){
                costBtn.interactable = false;
                txtItemLimit.text = "";
                price.text = "已领取";
                costBtn.image.sprite = await SpriteProxy.YellowBtnDisable;
            }
            else
            {
                costBtn.interactable = true;
                txtItemLimit.text = "";
                price.text = "领 取";
                costBtn.image.sprite = await SpriteProxy.YellowBtnEnable;
            }

        }

        private void OnCost(BabuButton sender)
        {
            Player.CardManager.RecruitController.GetRecruitRewards(data.cfg);
        }
    }
}