using System.Linq;
using System.Threading.Tasks;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class ItemtipsUIProperties : WindowProperties
    {
        public string Name;
        public int Count;
        public Task<Sprite> Icon;
        public string Desc;
        public GameItemType Type;
        public int GoodsType;
        public Task<Sprite> Quality;
        public Color Color;
        public CardModelConfig Prop;

        public GameItem Item;

        public ItemtipsUIProperties(CardModelConfig card)
        {
            Count = 1;
            Type = GameItemType.Card;

            int goodsId = 0;
            GoodsConfig cfg = Configs.Goods.GetConfigList().FirstOrDefault(item => item.Param2 == card.Id);
            if (cfg == null)
            {
                Debug.LogWarningFormat("ItemtipsUIProperties , ItemtipsUIProperties , GoodsConfig is null , cardId = {0}", card.Id);
            }
            else
            {
                goodsId = cfg.Id;
                Item = GameItemUtils.CreateGameItem(GameItemType.Card, goodsId, Count);
            }

            Quality = SpriteProxy.GetInvetoryQuality(card.Quality);
            Color = CBAColorUtil.Instance.GetColor(card.Quality);
            Icon = SpriteProxy.GetPropCard(card.Id);
            Name = PlayerCard.GetFullName(card);
            //球员的话 加属性描述
            Prop = card;
        }


        public ItemtipsUIProperties(int type, int ID, int count)
        {
            if ((GameItemType)type == GameItemType.Card)
            {
                var cfg = Configs.CardModel.GetConfig(ID);
                Prop = cfg;
            }

            Item = GameItemUtils.CreateGameItem((GameItemType)type, ID, count);
            Type = (GameItemType)type;
            Count = count;

            Name = Item.GetName();
            Icon = Item.GetIcon();
            Desc = Item.GetDescription();
            Quality = SpriteProxy.GetInvetoryQuality(Item.GetQuality());
            Color = CBAColorUtil.Instance.GetColor(Item.GetQuality());
        }

        public bool useCuntomPos = false;
        public Transform positionTransform;
        public Vector3 positionOffset;
        public void SetPos(Transform positionTransform, Vector3 positionOffset)
        {
            this.useCuntomPos = true;
            this.positionTransform = positionTransform;
            this.positionOffset = positionOffset;
        }

        public ItemtipsUIProperties(GameItem gameItem)
        {
            if (gameItem.Type == GameItemType.Card)
            {
                var cfg = Configs.CardModel.GetConfig(gameItem.Id);
                Prop = cfg;
            }

            Item = gameItem;
            Count = gameItem.Count;
            Type = (GameItemType)gameItem.Type;

            Name = Item.GetName();
            Icon = Item.GetIcon();
            Desc = Item.GetDescription();
            Quality = SpriteProxy.GetInvetoryQuality(Item.GetQuality());
            Color = CBAColorUtil.Instance.GetColor(Item.GetQuality());
        }
    }

    public class ItemtipsUI : AWindowController<ItemtipsUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private InventoryItem item;
        [SerializeField] private Image clockImg;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text recycleText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private GameObject prop;

        public ItemtipsUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            descText.gameObject.SetActive(true);
            prop.gameObject.SetActive(false);
            // 设置图标和数量
            item.SetImageAndCount(await Properties.Icon, Properties.Count.ToString());
            item.canShowTip = false;
            // 我拥有的数量
            countText.text = Lang.Get(LangID.HasCount) + Properties.Item.GetPlayerCount();
            // 设置名称
            nameText.text = Properties.Name;
            // 设置名称颜色
            nameText.color = Properties.Color;
            // 设置描述
            descText.text = Properties.Desc;
            // 背景颜色    
            backgroundImg.sprite = await Properties.Quality;
            // 设置回收信息
            if (Properties.Type == GameItemType.Goods)
            {
                if (Properties.GoodsType == (int)GoodsType.Pieces)
                {
                    recycleText.gameObject.SetActive(true);
                    recycleText.text = Lang.Get(LangID.RecycleToStarText);
                }
                else
                {
                    recycleText.gameObject.SetActive(true);
                    recycleText.text = Lang.Get(LangID.RecycleToEruoText);
                }
            }
            else
            {
                recycleText.gameObject.SetActive(false);
            }
            //设置道具类型
            if (Properties.Type == GameItemType.Resource)
            {
                typeText.text = Lang.Get(LangID.ResourcesDesc);
            }
            else if (Properties.Type == GameItemType.Card)
            {
                typeText.text = Lang.Get(LangID.Guys);
                //加属性描述
                descText.gameObject.SetActive(false);
                prop.gameObject.SetActive(true);
                for (int i = 0; i < 10; i++)
                {
                    prop.transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<TMP_Text>().text = Configs.CardAbility.GetConfig(i + 1).Name;
                    if (Properties.Prop == null)
                    {
                        prop.transform.GetChild(0).GetChild(i).GetChild(3).GetComponent<TMP_Text>().text = "0";
                    }
                    else
                    {
                        prop.transform.GetChild(0).GetChild(i).GetChild(3).GetComponent<TMP_Text>().text = Properties.Prop.Ability[i + 1].ToString();
                    }
                }
                if (Properties.Prop != null) typeText.text = PlayerCard.GetAdaptPositionAbbreviation(Properties.Prop);

                countText.text = Player.CardManager.GetCard(Properties.Prop.Id) == null ? "未拥有" : "已拥有";

                recycleText.gameObject.SetActive(true);
                recycleText.text = Lang.Get(LangID.PropRecycleTip);
                if (Properties.Prop == null)
                {
                    Debug.LogWarning("ItemTipUI , Properties.Type == GameItemType.Card , Properties.Prop == null");
                }
            }
            else if (Properties.Type == GameItemType.Goods)
            {
                if (Properties.GoodsType == 1)
                {
                    typeText.text = Lang.Get(LangID.NormalProp);
                }
                else if (Properties.GoodsType == 2)
                {
                    typeText.text = Lang.Get(LangID.BoxProp);
                }
                else if (Properties.GoodsType == 3)
                {
                    typeText.text = Lang.Get(LangID.SplinterProp);
                }
                else if (Properties.GoodsType == 4)
                {
                    typeText.text = Lang.Get(LangID.MaterialProp);
                }
                else
                {
                    typeText.text = "物品";
                }
            }
            else if (Properties.Type == GameItemType.Honour)
            {
                typeText.text = "荣誉";
            }
            else if (Properties.Type == GameItemType.None)
            {
                typeText.text = "";
            }
            else
            {
                typeText.text = "";
            }

            clockImg.gameObject.SetActive(false);
            timeText.gameObject.SetActive(false);

            CheckSetPos();

            Anim.PlayEnter();
        }

        private readonly int maxX = 360 - 210;
        private readonly int minX = -360 + 210;
        [SerializeField] private RectTransform panel = null;
        private void CheckSetPos()
        {
            if (Properties.useCuntomPos == false)
            {
                panel.pivot = new(0.5f, 0.5f);
                panel.localPosition = new Vector3(0, 150f, 0);
                return;
            }

            panel.pivot = new(0.5f, 1.0f);
            Vector3 localPosition = Vector3.zero;
            if (Properties.positionTransform != null)
            {
                localPosition = Utils.Utility.ConvertLocalPosition(Properties.positionTransform, Vector3.zero, panel.parent);
            }
            localPosition += Properties.positionOffset;

            if (localPosition.x > maxX) localPosition.x = maxX;
            if (localPosition.x < minX) localPosition.x = minX;

            panel.localPosition = localPosition;
        }

        private void OnClose()
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<ItemtipsUI>();
        }
    }
}