using System;
using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Linq;
using Coffee.UIEffects;
using GameConfig.Config;
using Babu.Config;
using Babu;
using static BigBang.SpriteNames;

namespace BigBang.UI
{
    [System.Serializable]
    public class CardDetailProperties : WindowProperties
    {
        public PlayerCard Card { get; set; } = null;
        public bool IsSelf { get; set; } = true;

        public CardDetailProperties(PlayerCard card, bool isSelf = true)
        {
            Card = card;
            IsSelf = isSelf;
        }

        /// <summary>
        /// 查看空白卡牌信息，展示为获得的卡牌
        /// </summary>
        public CardDetailProperties(int cardId, int star = 0)
        {
            Card = PlayerCard.GetEmptyCard(cardId);
            Card.Star = star;
            IsSelf = false;
        }
    }
    public class CardDetailUI : AWindowController<CardDetailProperties>
    {
        //球员已学特技面板
        [SerializeField] private CardLearnedPad cardLearnedPad;
        //球员擅长地图按钮
        [SerializeField] private Button goodAtMapBtn;
        // 返回按钮
        [SerializeField] private Button closeBtn;
        // 升星按钮
        [SerializeField] private Button upgradeStarBtn;
        [SerializeField] private Button upgradeQualityBtn;
        [SerializeField] private GameObject maxPanel;
        [SerializeField] private Image smallCardLowQ;
        [SerializeField] private Image smallCardL2H;
        [SerializeField] private Image smallCardHighQ;
        // 球员姓名
        [SerializeField] private TMP_Text nameText;
        // 设置球员身高
        [SerializeField] private TMP_Text heightText;
        // 设置球员体重
        [SerializeField] private TMP_Text weightText;
        // 战力
        [SerializeField] private TMP_Text fightText;
        // 体能
        [SerializeField] private TMP_Text energyText;
        // 健康
        [SerializeField] private TMP_Text healthText;
        // 禁赛
        [SerializeField] private TMP_Text banText;
        // 球员位置
        [SerializeField] private TMP_Text positionText;
        // 状态
        [SerializeField] private Image stateImg;
        //球员背景
        [SerializeField] private Image cardBackImage;
        // 星级
        [SerializeField] private List<GameObject> stars;
        // 训练名称
        [SerializeField] private CardTrainInfoItem cardTrainInfoItem;
        [SerializeField] private CardDetailUIAnim anim;
        [SerializeField] private ScrollRect cardDertailScroll;

        //球员号码
        [SerializeField] private Button NumberBtn;
        [SerializeField] private TMP_Text NumberText;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject StarPanel;
        [SerializeField] private Button energyButton;
        [SerializeField] private Button medicalButton;
        [SerializeField] private Button stateButton;
        // 球员头像
        [SerializeField] private Image playerImg1;
        [SerializeField] private GameObject playerImgMask1;
        [SerializeField] private Image playerImg2;
        [SerializeField] private GameObject playerImgMask2;
        [SerializeField] private List<InventoryBaseItem> giftSkillList;
        //闪光动画
        [SerializeField] private UIShiny shiny;

        private static CardDetailUI _inst;
        public static CardDetailUI Instance
        {
            get
            {
                return _inst;
            }
            private set { }
        }
        protected override void Awake()
        {
            base.Awake();
            _inst = this;
        }
        protected override void AddListeners()
        {
            goodAtMapBtn.onClick.AddListener(OnGoodAtMap);
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.RefreshCardRecoverProperties, RefreshCardRecoverProperties);
        }
        protected override void RemoveListeners()
        {
            goodAtMapBtn.onClick.RemoveListener(OnGoodAtMap);
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.RefreshCardRecoverProperties, RefreshCardRecoverProperties);
        }
        private void OnGoodAtMap()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.OpenWindow<PlayerPosHelpUI>(new PlayerPosHelpProperties(Properties.Card));
        }
        private void OnClose()
        {
            closeBtn.GetComponent<ButtonAnim>().PlayBack(() => UIController.Instance.CloseWindow<CardDetailUI>(), playAudio: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            });
        }
        private void OnClickUpgradeQuality()
        {
            CardUpgradeType updateType = CardUpgradeType.UpgradeQuality;

            upgradeStarBtn.GetComponent<ButtonAnim>().Play(() => UIController.Instance.ShowPanel<CardUpgradeUI>(new CardUpgradeUIProperties(updateType, Properties.Card)),
            playAudio: false, audioCallback: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            });
        }
        private void OnClickUpgradeStar()
        {
            CardUpgradeType updateType = CardUpgradeType.UpgradeStar;

            upgradeStarBtn.GetComponent<ButtonAnim>().Play(() => UIController.Instance.ShowPanel<CardUpgradeUI>(new CardUpgradeUIProperties(updateType, Properties.Card)),
            playAudio: false, audioCallback: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            });
        }
        //球员号码
        private void OnChangeNumber()
        {
            UIController.Instance.OpenWindow<ChangeNumberUI>(new ChangeNumberUIProperties(Properties.Card, NumberText));
        }

        private void OnClickEnergyButton()
        {
            UIController.Instance.OpenWindow<PlayerRecoverUI>(new PlayerRecoverUIProperties(Properties.Card, PlayerRecoverType.RecoverEnergy));
        }
        private void OnClickMedicalButton()
        {
            UIController.Instance.OpenWindow<PlayerRecoverUI>(new PlayerRecoverUIProperties(Properties.Card, PlayerRecoverType.RecoverMedical));
        }

        private void OnClickStateButton()
        {
            UIController.Instance.OpenWindow<PlayerRecoverUI>(new PlayerRecoverUIProperties(Properties.Card, PlayerRecoverType.Coach2State));
        }

        /**
        更新球员恢复状态
        */
        public void RefreshCardRecoverProperties(object[] _)
        {
            // 设置球员体能
            energyText.text = $"{Properties.Card.TotalEnergyRatio.ToString("f2")} %";
            // 设置球员状态
            SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[(int)Properties.Card.Status], (s) => { stateImg.sprite = s; });
            // 设置球员伤病状态
            LangID healthLangID = new LangID[] { 0, LangID.HealthText, LangID.MinorInjuryText, LangID.SeriousInjury }[(int)Properties.Card.InjuryType];
            healthText.text = Lang.Get(healthLangID);
        }

        [SerializeField] private RectTransform playerInfoPanel = null;
        [SerializeField] private RectTransform statusPanel = null;
        [SerializeField] private RectTransform powerPanel = null;
        [SerializeField] private RectTransform composePanel = null;
        [SerializeField] private RectTransform giftPanel = null;
        [SerializeField] private RectTransform skillPanel = null;

        [SerializeField] private RectTransform peakYearPanel = null;
        [SerializeField] private TMP_Text peakNameText = null;
        [SerializeField] private TMP_Text peakYearText = null;
        [SerializeField] private PeakImage peakImage = null;
        protected override async void OnPropertiesSet()
        {
            // 设置球员姓名
            bool isPeak = PlayerCard.IsPeak(Properties.Card.Config);
            nameText.gameObject.SetActive(!isPeak);
            peakYearPanel.gameObject.SetActive(isPeak);
            if (!isPeak)
            {
                nameText.text = PlayerCard.GetFullName(Properties.Card.Config);
                nameText.color = Properties.Card.NameColor;
            }
            else
            {
                peakImage.SetData(Properties.Card);
                peakNameText.text = Properties.Card.Config.Name;
                peakYearText.text = Properties.Card.Config.PeakYear;
                peakNameText.color = Properties.Card.NameColor;
            }
            // 设置球员身高
            heightText.text = $"{Properties.Card.Config.Height.ToString()} {Lang.Get(LangID.CmTxt)}";
            // 设置球员体重
            weightText.text = $"{Properties.Card.Config.Weight.ToString()} {Lang.Get(LangID.KgTxt)}";
            // 设置球员体能
            energyText.text = $"{Properties.Card.TotalEnergyRatio.ToString("f2")} %";
            // 设置战力
            fightText.text = (Properties.Card.ServerStrength > 0) ? Properties.Card.ServerStrength.ToString() : Properties.Card.FightPoint.ToString();
            // 设置球员位置
            positionText.text = Configs.SeparatedPosition.GetConfig(Properties.Card.Config.AdaptPosition[0]).Name;
            // 设置球员状态
            SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[(int)Properties.Card.Status], (s) => { stateImg.sprite = s; });
            // 设置球员头像
            SetPlayerHead(Properties.Card.Config.Quality, Properties.Card.Config.Portrait);
            //设置球员背景
            cardBackImage.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.SquareBack, Properties.Card.Quality);
            // 设置球员伤病状态
            LangID healthLangID = new LangID[] { LangID.HealthText, LangID.HealthText, LangID.MinorInjuryText, LangID.SeriousInjury }[(int)Properties.Card.InjuryType];
            healthText.text = Lang.Get(healthLangID);
            // 🔴设置球员禁赛状态
            banText.text = "无"; //string.Empty;
            //设置球员号码
            NumberText.text = Properties.Card.PlayerCardNumber.ToString();
            if (Properties.Card.Star > 5)
            {
                int showStar = Properties.Card.Star - 5;
                // 设置星级
                for (int i = 0; i < stars.Count; i++)
                {
                    stars[i].SetActive(true);
                    if (i + 1 <= showStar)
                        stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetColorfulStar();
                    else
                        stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetYellowStar();
                }
            }
            else
            {
                for (int i = 0; i < stars.Count; i++)
                {
                    stars[i].SetActive(i + 1 <= Properties.Card.Star);
                    stars[i].GetComponent<Image>().sprite = await SpriteProxy.GetYellowStar();
                }
            }
            cardTrainInfoItem.SetDataShow(Properties.Card);
            anim.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS);
            //设置已学习技能
            cardLearnedPad.isSelf = Properties.IsSelf;
            cardLearnedPad.SetData(Properties.Card, Properties.Card.SkillDic.ToList().Where(card =>
            {
                return card.Value.Level > 0;
            }).ToList());
            //设置升级星星还是品质
            if (Properties.Card.IsStarAndQualityMax() == false)
            {
                maxPanel.SetActive(false);
                if (Properties.Card.CouldUpgradeStarInThisQuality() || Properties.IsSelf == false)
                {
                    upgradeQualityBtn.gameObject.SetActive(false);
                }
                else
                {
                    upgradeQualityBtn.gameObject.SetActive(true);
                }

                StarPanel.SetActive(true);
            }
            else
            {
                upgradeQualityBtn.gameObject.SetActive(false);
                upgradeStarBtn.gameObject.SetActive(false);
                maxPanel.SetActive(true);
                StarPanel.SetActive(false);

                smallCardLowQ.sprite = await SpriteProxy.GetQualityAdvanceTagInTag(SpriteNames.Card.SmallQualityCard, Properties.Card.Config.Quality);
                smallCardL2H.sprite = await SpriteProxy.GetQualityAdvanceTagInTag(SpriteNames.Card.SmallQualityL2H, Properties.Card.Config.Quality);
                smallCardHighQ.sprite = await SpriteProxy.GetQualityAdvanceTagInTag(SpriteNames.Card.SmallQualityCard, Properties.Card.Quality);
            }
            if (shiny != null)
            {
                shiny.enabled = Properties.Card.Quality >= QualityType.Orange;
                shiny.Play();
            }
            if (!Properties.Card.isEmptyCard) SetCardEquip();
            SetCardGiftSkill();
            cardDertailScroll.ScroolToTop(0);

            composePanel.gameObject.SetActive(!Properties.Card.isEmptyCard);
            // skillPanel.gameObject.SetActive(!Properties.Card.isEmptyCard);//技能面板没有的话太空了
            statusPanel.gameObject.SetActive(Properties.IsSelf);
        }

        [SerializeField] private List<cardEquipItem> equipList = new();
        private void SetCardEquip()
        {
            List<int> showLevels = new();
            foreach (int equipLevel in Properties.Card.EquipLevels)
            {
                if (Properties.Card.EquipGrade + 1 < equipLevel)
                {
                    showLevels.Add(equipLevel);
                }
                else
                {
                    showLevels.Add(equipLevel - 1);
                }
            }

            List<JerseyUpgradeConfig> list = Properties.Card.GetEquipLevelsConfig(showLevels);
            for (int index = 0; index < 4; index++)
            {
                if (showLevels[index] <= 0)
                {
                    equipList[index].gameObject.SetActive(false);
                }
                else
                {
                    equipList[index].gameObject.SetActive(true);
                    JerseyUpgradeConfig equipConfig = list[index];
                    if (equipConfig == null)
                    {
                        equipList[index].SetData(Properties.Card, index, equipConfig);
                    }
                    else
                    {
                        equipList[index].SetCardDetailData(equipConfig, Properties.Card);
                    }
                }
            }
        }

        private async void SetCardGiftSkill()
        {
            //天赋技能
            var giftSkillTemplateIdList = Configs.CardModel.GetConfig(Properties.Card.CardId).GiftIds.ToList();
            var cfg = Configs.CardUpgrade.GetConfigList().FirstOrDefault(p => p.CardId == Properties.Card.CardId && p.Star == Properties.Card.Star && p.Quality == Properties.Card.Quality);
            var skillLvLst = cfg != null ? cfg.Sklv : new Dictionary<int, int> { { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 1 } };
            for (var index = 0; index < 4; index++)
            {
                if (index >= giftSkillTemplateIdList.Count)
                {
                    giftSkillList[index].gameObject.SetActive(false);
                }
                else
                {
                    giftSkillList[index].gameObject.SetActive(true);

                    var _skId = giftSkillTemplateIdList[index] + (skillLvLst[index + 1] - 1) * 10;
                    var skillActived = Properties.Card.ActivedGiftSkillCount >= index + 1;
                    var _skCfg = Configs.GiftSkill.GetConfig(_skId);

                    var sp = await SpriteProxy.GetGiftSkillImg(_skCfg);
                    var fireSection = PlayerCard.GetSkillFireSection(_skCfg);
                    giftSkillList[index].SetData(_skCfg.Name, _skCfg.Desc, sp, _skCfg.Sklv, skillActived, false, true, _skCfg.Fire > 0, fireSection);
                    giftSkillList[index].SetText("Lv." + skillLvLst[index + 1].ToString());
                }
            }
        }

        private async void SetPlayerHead(int quality, int portrait)
        {
            bool isYellow = quality >= 4;
            playerImgMask1.gameObject.SetActive(!isYellow);
            playerImgMask2.gameObject.SetActive(isYellow);
            if (isYellow == false)
            {
                playerImg1.sprite = await SpriteProxy.GetPlayerPortrait(portrait);
            }
            else
            {
                playerImg2.sprite = await SpriteProxy.GetPlayerPortraitYellow(portrait);
            }
        }

        public CardLearnedPad skillPad
        {
            get { return cardLearnedPad; }
            private set { }
        }
    }
}
