using Babu.SDK;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig.Config;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class ShopDiamondItemData
    {
        public int ShopItemId;
        public float Cost;
        public int Obtain;
        public Task<Sprite> DiamondImg;
        public bool Discount;
        public bool Hot;
        public bool IsFirst;
        public int FirstValue;
        public string ProductID;

        public ShopDiamondItemData(DiamondShopConfig cfg)
        {
            // 设置商品ID
            ShopItemId = cfg.Id;
            // 设置花费
            Cost = cfg.Rmb;
            // 设置充值获得
            Obtain = cfg.Obtain;
            // 设置图标
            DiamondImg = SpriteProxy.GetShopItem(cfg.Id);
            // ⚠折扣
            Discount = false;
            // ⚠热门
            Hot = false;
            // 设置首充赠送
            FirstValue = cfg.Give;
            // 设置首充状态
            int count = 0;
            Player.ShopManager.SumCount.TryGetValue(cfg.Id, out count);
            IsFirst = count == 0;
            // 设置产品ID
            ProductID = cfg.ProductId;
        }
    }

    public class ShopDiamondItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text costTxt;
        [SerializeField] private TMP_Text obtainTxt;
        [SerializeField] private TMP_Text firstTxt;
        [SerializeField] private Image diamondImg;
        [SerializeField] private Image discountImg;
        [SerializeField] private Image hotImg;
        [SerializeField] private GameObject first;
        [SerializeField] private BabuButton costBtn;
        [SerializeField] private UIShiny shiny;

        private ShopDiamondItemData data;

        private void OnEnable()
        {
            costBtn.OnClick += OnCost;
        }

        private void OnDisable()
        {
            costBtn.OnClick -= OnCost;
        }

        private void OnCost(BabuButton _)
        {
            if (ServerConst.OPEN_BUY == false)
            {
                Tips.PopTips("测试期间不开放充值");
                return;
            }
            PurchaseInfo info = DataConvUtil.NewPurchase(data.ProductID, "Diamonds", data.Cost, data.ShopItemId);
#if USER_DEBUG && UNITY_EDITOR
            PurchaseUtil.TestBuyInEditor(info.ShopItemId);
            return;
#endif
            PurchaseServiceManager.Instance.Purchase(info);
        }

        public async void SetData(ShopDiamondItemData data)
        {
            this.data = data;
            costTxt.text = data.Cost.ToString();
            obtainTxt.text = data.Obtain.ToString();
            firstTxt.text = data.FirstValue.ToString();
            diamondImg.sprite = await data.DiamondImg;
            discountImg.gameObject.SetActive(data.Discount);
            hotImg.gameObject.SetActive(data.Hot);
            first.SetActive(data.IsFirst);
        }

        public void PlayPointAnim(float delay)
        {
            shiny.effectFactor = 0;
            DOTween.To(value => shiny.effectFactor = value, 0, 1, 3f);
        }
    }
}