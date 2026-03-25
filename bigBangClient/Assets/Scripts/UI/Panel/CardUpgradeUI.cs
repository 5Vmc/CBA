using UnityEngine;
using deVoid.UIFramework;
using UnityEngine.UI;
using System.Collections.Generic;
using Utils;
using TMPro;
using BigBang.Animation;
using DG.Tweening;

using Utils.GameItem;
using GameConfig;
using Babu;
using System;

namespace BigBang.UI
{
    [System.Serializable]
    public class CardUpgradeUIProperties : WindowProperties
    {
        public PlayerCard Card { get; set; }
        public CardUpgradeType UpgradeType { get; set; }

        public CardUpgradeUIProperties(CardUpgradeType updateType, PlayerCard card)
        {
            Card = card;
            UpgradeType = updateType;
        }
    }

    public enum CardUpgradeType
    {
        UpgradeStar = 1,
        UpgradeQuality = 2
    }

    public enum CardUpgradeStatus
    {
        None = -1,
        CanUpQuality = 0,
        CanUpStar = 1,

        Max = 2
    }

    public class CardUpgradeUI : AWindowController<CardUpgradeUIProperties>
    {
        [SerializeField] private CardItem cardItem4Star;
        // 战力
        [SerializeField] private TMP_Text fightText;
        // 升级增加多少战力
        [SerializeField] private TMP_Text fightAddText;
        // 返回按钮
        //[SerializeField] private Button closeBtn;
        // 升星按钮
        //[SerializeField] private Button upgradeStarBtn;
        [SerializeField] private Image upgradeStarLightBg;
        [SerializeField] private GameObject upgradeStarPanel;

        [SerializeField] private TMP_Text maxStarText;



        // 道具
        //[SerializeField] private List<PropItem> propItems;
        [SerializeField] private CardTrainInfoItem cardTrainInfoItem;

        [SerializeField] private RectTransform scrollViewRect;

        [SerializeField] public CardUpgradeUIAnim Anim;

        //升阶panel
        [SerializeField] private CardItem qualityLowCard;
        [SerializeField] private CardItem qualityHighCard;
        [SerializeField] private GameObject upgradeQualityPanel; //升阶panel end
                                                                 // 升阶按钮
                                                                 //[SerializeField] private Button upgradeQualityBtn;

        // [SerializeField] private RectTransform attrScrollView;

        [SerializeField] private CardUpgradeStatusItem[] StatusItems; //升星，升阶，满级
        [SerializeField] private RectTransform StatusTipBg;
        [SerializeField] private TMP_Text StatusTipText;

        //满级状态下的数值显示
        [SerializeField] private GameObject maxInfoPanel;
        // 设置球员身高
        [SerializeField] private TMP_Text heightText;
        //球员号码
        [SerializeField] private TMP_Text numberText;
        // 设置球员体重
        [SerializeField] private TMP_Text weightText;

        // 体能
        [SerializeField] private TMP_Text energyText;
        // 健康
        [SerializeField] private TMP_Text healthText;
        // 禁赛
        [SerializeField] private TMP_Text banText;
        // 球员位置
        [SerializeField] private TMP_Text posText;
        // 状态
        [SerializeField] private Image stateImg;

        [SerializeField] private BabuButton btnContinue;
        [SerializeField] private TMP_Text txtContinue;
        private static CardUpgradeUI _inst;
        protected override void Awake()
        {
            base.Awake();
            _inst = this;
        }

        public static CardUpgradeUI Instance
        {
            get { return _inst; }
            private set { }
        }

        protected override void AddListeners()
        {
            btnContinue.OnClick += OnClose;
        }

        protected override void RemoveListeners()
        {
            btnContinue.OnClick -= OnClose;
        }

        private void InitUI()
        {
            qualityLowCard.gameObject.SetActive(true);
            qualityHighCard.gameObject.SetActive(true);
            btnContinue.gameObject.SetActive(false);
            txtContinue.gameObject.SetActive(false);
            
            upgradeQualityPanel.SetActive(false);

            int index = 0;
            foreach (CardUpgradeStatusItem item in StatusItems)
            {
                item.InitMe((CardUpgradeStatus)index);
                item.ShowMe(false);
                index++;
            }

            maxInfoPanel.SetActive(false);
            StatusTipBg.gameObject.SetActive(false);
        }

        private void UpdateUI()
        {
            if (Properties.UpgradeType == CardUpgradeType.UpgradeStar)
            {
                upgradeStarPanel.SetActive(true);
                upgradeQualityPanel.SetActive(false);
                // 设置球员卡片
                cardItem4Star.SetDataAndHideLeftStar(Properties.Card); //SetData(Properties.Card);
                cardItem4Star.Anim.ClearNoLightStar();
            }
            else
            {
                upgradeStarPanel.SetActive(false);
                upgradeQualityPanel.SetActive(true);

                qualityLowCard.SetData(Properties.Card);
                qualityLowCard.Anim.ClearNoLightStar();

                PlayerCard highQualityCard = (PlayerCard)Properties.Card.Clone();

                highQualityCard.Quality = highQualityCard.Quality + 1;
                highQualityCard.Star = 0;
                qualityHighCard.SetData(highQualityCard, false);

                qualityHighCard.Anim.ClearNoLightStar();

            }
        }

        protected override async void OnPropertiesSet()
        {
            this.InitUI();

            UpdateData();
            UpdateUI();

            // 设置背光
            upgradeStarLightBg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Light, Properties.Card.Quality);
            // 播放动画
            fightAddText.GetComponent<LoomAnim>().PlayText(0.5f, 0.5f, 0.3f);
            // 播放动画

            Anim.SetUpgradeType(Properties.UpgradeType);
            Anim.PlayEnter();

            var len = scrollViewRect.sizeDelta.y;
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);

            if (Properties.UpgradeType == CardUpgradeType.UpgradeStar)
            {
                OnUpgradeStar();
            }
            else
            {
                OnUpgradeQuality();
            }


        }

        private string RemoveColor(string str)
        {
            while (str.Contains("</color>") || str.Contains("</Color>"))
            {
                var startIndex = str.IndexOf("<");
                var endIndex = str.IndexOf(">");
                str = str.Remove(startIndex, endIndex - startIndex + 1);
            }
            return str;
        }

        private (int, int) GetValue(string value)
        {
            try
            {
                int value1 = int.Parse(value.Split('/')[0]);
                int value2 = int.Parse(value.Split('/')[1]);
                return (value1, value2);
            }
            catch
            {
                return (0, 0);
            }
        }


        //满星的时候
        private void ShowMaxStarUI(bool isMax)
        {
            if (isMax)
            {
                maxStarText.gameObject.SetActive(true);

                scrollViewRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 500);

                //去掉星星的动画
                if (Properties.Card.IsStarAndQualityMax() == true)
                {
                    SetStatusImage(CardUpgradeStatus.Max);
                }
                else
                {
                    maxStarText.text = "玩家满星，可以进阶！";
                    SetStatusImage(CardUpgradeStatus.CanUpQuality);
                }
            }
            else
            {
                maxStarText.gameObject.SetActive(false); 
            }
        }
        private void UpdateData()
        {
            // 设置球员战力
            fightText.text = Properties.Card.FightPoint.ToString();
            // 设置升级之后加多少战力
            var effectivenessStarUpgradeAdd = 0;
            List<GameItem> props = null;
            if (Properties.UpgradeType == CardUpgradeType.UpgradeStar)
            {
                effectivenessStarUpgradeAdd = Properties.Card.GetUpgradeStarCombatEffectivenessAdd();
                props = Player.CardManager.GetUpgradeStarItems(Properties.Card.CardId);

                bool canUpdgardStar = Properties.Card.CouldUpgradeStarInThisQuality();
                this.ShowMaxStarUI(!canUpdgardStar);

                // 设置训练属性
                cardTrainInfoItem.SetDataShow(Properties.Card);
            }
            else
            {
                effectivenessStarUpgradeAdd = Properties.Card.GetUpgradeQualityCombatEffectivenessAdd();
                props = Player.CardManager.GetUpgradeQualityItems(Properties.Card.CardId);
                this.ShowMaxStarUI(false);

                // 设置训练属性
                cardTrainInfoItem.SetDataShow(Properties.Card);
            }
            fightAddText.text = $"+{effectivenessStarUpgradeAdd}";
            fightAddText.gameObject.SetActive(effectivenessStarUpgradeAdd > 0);
            // 播放动画
            fightAddText.GetComponent<LoomAnim>().PlayText(0.5f, 0.5f, 0.3f);
        }

        private void OnUpgradeStar()
        {
            Player.CardManager.CardUpgradeStar(Properties.Card.CardId, OnUpgradeStarSuccess);
        }

        private void OnUpgradeQuality()
        {
            Player.CardManager.CardUpgradeQuality(Properties.Card.CardId, OnUpgradeQualitySucess);
        }

        private void OnUpgradeStarSuccess()
        {
            cardTrainInfoItem.SetDataCmp(Properties.Card, true);
            GetComponent<CardUpgradeUIAnim>().PlayUpStar(() =>
            {
                btnContinue.gameObject.SetActive(true);
                txtContinue.gameObject.SetActive(true);
                if (Properties.Card.IsStarAndQualityMax() == true)
                {
                    SetStatusImage(CardUpgradeStatus.Max);
                    SetMaxInfo();
                }
            });
            EventManager.Instance.Dispatch(EventID.OnCardUpgradeStar);
        }



        //升阶结束
        public void OnUpgradeQualitySucess()
        {
            cardTrainInfoItem.SetDataCmp(Properties.Card, true);
            GetComponent<CardUpgradeUIAnim>().PlayUpGrade(() => {
                if (Properties.Card.IsStarAndQualityMax() == false)
                {
                    SetStatusImage(CardUpgradeStatus.CanUpStar);
                }
                else
                {
                    SetStatusImage(CardUpgradeStatus.Max);

                    SetMaxInfo();
                }

                btnContinue.gameObject.SetActive(true);
                txtContinue.gameObject.SetActive(true);
            });
            EventManager.Instance.Dispatch(EventID.OnCardUpgradeStar);
        }

        private void SetStatusImage(CardUpgradeStatus status)
        {
            Debug.Log(">>>>>>>>> SetStatusImage=" + status);
            foreach (CardUpgradeStatusItem statusItem in this.StatusItems)
            {
                if (status == statusItem.Status)
                {
                    statusItem.ShowMe(true);
                    StatusTipBg.gameObject.SetActive(true);
                    StatusTipBg.localScale = new Vector3(0.1f, 1f, 1);
                    StatusTipBg.DOScaleX(1, 0.1f).SetDelay(0.5f);
                }
                else
                    statusItem.ShowMe(false);
            }

            if (status == CardUpgradeStatus.None)
            {
                StatusTipBg.gameObject.SetActive(false);
            }

            if (status == CardUpgradeStatus.Max)
            {
                StatusTipText.text = "球员已经满级，无法再升级!";
            }
            else if (status == CardUpgradeStatus.CanUpQuality)
            {
                StatusTipText.text = "将球员提升到更高的品质!";
            }

        }

        private void OnClose(BabuButton sender)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            UIController.Instance.CloseWindow<CardUpgradeUI>();
            EventManager.Instance.Dispatch(EventID.RefreshWindow, 2);
        }

        public void OnClickStatusImage(CardUpgradeStatus status)
        {
            switch (status)
            {
                case CardUpgradeStatus.CanUpQuality:
                    Properties.UpgradeType = CardUpgradeType.UpgradeQuality;
                    UpdateData();
                    UpdateUI();
                    SetStatusImage(CardUpgradeStatus.None);
                    Anim.ChangeUpgradeType(CardUpgradeType.UpgradeQuality);
                    break;
                case CardUpgradeStatus.CanUpStar:
                    Properties.UpgradeType = CardUpgradeType.UpgradeStar;
                    UpdateData();
                    UpdateUI();
                    SetStatusImage(CardUpgradeStatus.None);
                    Anim.ChangeUpgradeType(CardUpgradeType.UpgradeStar);
                    break;
                case CardUpgradeStatus.Max:

                    break;
            }

        }


        //全满的时候，到顶了
        private void SetMaxInfo()
        {
            maxInfoPanel.SetActive(true);

            Anim.PlayMaxInfoItemsAnim();

            qualityHighCard.HideFlagAndPositonText();

            // 设置球员身高
            heightText.text = $"{Properties.Card.Config.Height.ToString()} {Lang.Get(LangID.CmTxt)}";
            // 设置球员体重
            weightText.text = $"{Properties.Card.Config.Weight.ToString()} {Lang.Get(LangID.KgTxt)}";
            // 设置球员体能
            energyText.text = $"{Properties.Card.TotalEnergyRatio.ToString("f2")} %";

            // 设置球员位置
            posText.text = Configs.SeparatedPosition.GetConfig(Properties.Card.Config.AdaptPosition[0]).Abbreviation;
            // 设置球员状态
            SpriteManager.GetSprite(AtlasNames.Player, SpriteNames.Player.PlayerState[(int)Properties.Card.Status], (s) => { stateImg.sprite = s; });

            // 设置球员伤病状态
            LangID healthLangID = new LangID[] { 0, LangID.HealthText, LangID.MinorInjuryText, LangID.SeriousInjury }[(int)Properties.Card.InjuryType];
            healthText.text = Lang.Get(healthLangID);
            // 🔴设置球员禁赛状态
            banText.text = "无"; //string.Empty;
            //设置球员号码
            numberText.text = Properties.Card.PlayerCardNumber.ToString();
        }
    }
}
