using Babu;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class ShopTrainItemData
    {
        public int ShopItemId;
        public Task<Sprite> Icon;
        public int Cost;
        public string Desc;
        public string Count;
        public bool Black;
        public int Obtain;

        public ShopTrainItemData(TrainShopConfig cfg)
        {
            ShopItemId = cfg.Id;
            Obtain = cfg.Obtain;
            Icon = SpriteProxy.GetShopItem(cfg.Id);
            Cost = cfg.Diamond;
            Desc = cfg.Name;
            Count = $"({Player.ShopManager.GetBuyCount(cfg.Id)}/{cfg.Limit})";
            Black = Player.ShopManager.GetBuyCount(cfg.Id) >= cfg.Limit;
        }
    }

    public class ShopTrainItem : MonoBehaviour
    {
        [SerializeField] private Image IconImg;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private TMP_Text costTxt;
        [SerializeField] private TMP_Text descTxt;
        [SerializeField] private TMP_Text countTxt;
        [SerializeField] private BabuButton costBtn;
        [SerializeField] private Image blackImg;
        [SerializeField] private UIShiny shiny;

        private ShopTrainItemData data;

        private void OnEnable()
        {
            costBtn.OnClick += OnCost;
        }

        private void OnDisable()
        {
            costBtn.OnClick -= OnCost;
        }

        public async void SetData(ShopTrainItemData data)
        {
            this.data = data;
            IconImg.sprite = await data.Icon;
            SpriteManager.GetSprite(AtlasNames.Shop, SpriteNames.Shop.TrainShop.Replace("{shopItemID}", data.ShopItemId.ToString()), s => backgroundImg.sprite = s);
            costTxt.text = data.Cost.ToString();
            descTxt.text = data.Desc;
            countTxt.text = data.Count;
            blackImg.gameObject.SetActive(data.Black);
        }

        private void OnCost(BabuButton _)
        {
            int limit = Configs.TrainShop.GetConfig(data.ShopItemId).Limit;
            if (Player.TrainManager.GetUnlockCount() <= 0)
            {
                Tips.PopError(ErrorID.UnlockOneTrainItem);
                return;
            }
            if (Player.PackageManager.Diamond < data.Cost)
            {
                // 钻石不足
                Tips.PopError(ErrorID.DiamondNotEnough);
                return;
            }
            if (Player.ShopManager.GetBuyCount(data.ShopItemId) >= limit)
            {
                Tips.PopTips(Lang.Error(ErrorID.SellOutToday));
                return;
            }

            if (Player.ShopManager.isNeedAlertExchange)
            {
                UIController.Instance.OpenWindow<ConfirmBoxCheckUI>(new ConfirmBoxCheckUIProperties("花费{0}钻石购买{1}".SafeFormat(data.Cost, data.Desc), () =>
                {
                    DoCost();
                }, null, !Player.ShopManager.isNeedAlertExchange, "不再提醒", (bool isCheck) => { Player.ShopManager.isNeedAlertExchange = !isCheck; }));
            }
            else
            {
                DoCost();
            }
        }
        private void DoCost()
        {
            NetworkManager.Instance.TrainShop(data.ShopItemId, response =>
            {
                // 购买成功
                if (response.Succeed)
                {
                    Player.ShopManager.RecordBuyCount(data.ShopItemId, 0);
                    // 刷新商店界面
                    Babu.EventManager.Instance.Dispatch(EventID.OnRefreshTrainShop);
                    // 设置获得项目
                    List<GameItem> obtainList = new List<GameItem>();
                    var gameItem = GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.TrainExpMin, data.Obtain);
                    var obtainExp = Player.TrainManager.GetMinExpReward(data.Obtain);
                    gameItem.Desc = obtainExp.ToFormatString();
                    obtainList.Add(gameItem);
                    // 打开收益界面
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));
                }
            });
        }

        public void PlayPointAnim(float delay)
        {
            shiny.effectFactor = 0;
            DOTween.To(value => shiny.effectFactor = value, 0, 1, 3);
        }
    }
}