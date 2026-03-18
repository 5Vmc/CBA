using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Linq;
using Utils.GameItem;
using GameConfig;
using TMPro;
using GameConfig.Config;
using Coffee.UIEffects;
using UnityTimer;
using Babu;

namespace BigBang.UI
{
    public class FirstChargeUI : AWindowController
    {
        [SerializeField] private Button goBtn;
        [SerializeField] private Button getBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private Image img1;
        [SerializeField] private TMP_Text countTxt1;
        [SerializeField] private Image img2;
        [SerializeField] private TMP_Text countTxt2;

        [SerializeField] private UIShiny shiny;
        // [SerializeField] private CardAndDebrisItem cardItem;

        protected override void AddListeners()
        {
            base.AddListeners();
            goBtn.onClick.AddListener(OnGo);
            getBtn.onClick.AddListener(OnGet);
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            goBtn.onClick.RemoveListener(OnGo);
            getBtn.onClick.RemoveListener(OnGet);
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            goBtn.gameObject.SetActive(Player.ShopManager.SumCost < GameConst.FIRST_CHARGE_COST);
            getBtn.gameObject.SetActive(Player.ShopManager.SumCost >= GameConst.FIRST_CHARGE_COST);
            var gameItems = GameItemUtils.CreateGameItems(Configs.FirstChargeReward.GetConfigList().First().Reward);
            // var card = gameItems.First(item => item.Type == GameItemType.Card);
            // cardItem.SetData(Configs.CardModel.GetConfig(card.Id));
            var items = gameItems.Where(item => item.Type != GameItemType.Card).ToList();
            img1.sprite = await items[0].GetIcon();
            countTxt1.text = items[0].CountString();
            img2.sprite = await items[1].GetIcon();
            countTxt2.text = items[1].CountString();

            // shiny.gameObject.SetActive(false);

            // Timer.Register(this.gameObject, 0.1f, ()=>{

            //     shiny.gameObject.SetActive(true);
            //     shiny.Play();
            // });

        }

        private void OnGo()
        {
            UIController.Instance.CloseWindow<FirstChargeUI>();
            UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Diamond));
        }

        private void OnGet()
        {
            Debug.Log("领取首充奖励");
            var rewards = GameItemUtils.CreateGameItems(Configs.FirstChargeReward.GetConfigList().First().Reward);
            var cards = rewards.Where(item => item.Type == GameItemType.Card && Player.CardManager.GetCard(item.Id) == null).ToList();
            var gameItems = rewards.Where(item => item.Type != GameItemType.Card).ToList();
            foreach (var item in rewards.Where(item => item.Type == GameItemType.Card && Player.CardManager.GetCard(item.Id) != null))
            {
                var goodsCfg = Configs.Goods.GetConfig(Player.CardManager.GetCard(item.Id).Config.PiecesId);
                gameItems.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, goodsCfg.Id, goodsCfg.Param1));
            }
            NetworkManager.Instance.GetFirstChargeReward(response =>
            {
                if (response.Succeed)
                {
                    UIController.Instance.CloseWindow<FirstChargeUI>();
                    Player.ShopManager.FirstCharge = true;

                    if (cards.Any())
                    {
                        UIController.Instance.OpenWindow<SuperCardUI>(new SuperCardUIProperties(false, () =>
                        {
                            var properties = new InventoryObtainedUIProperties(gameItems, null);
                            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                        }, cards.Select(item => Configs.CardModel.GetConfig(item.Id)).ToList<CardModelConfig>()));
                    }
                    else
                    {
                        var properties = new InventoryObtainedUIProperties(gameItems, null);
                        UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                    }

                    Babu.EventManager.Instance.Dispatch(EventID.OnGetFirstChargeRewardSucceed);
                }
                else
                {
                    Debug.Log("领取首充奖励失败 :" + response.Succeed);
                }
            });
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.CloseWindow<FirstChargeUI>();
        }
    }
}