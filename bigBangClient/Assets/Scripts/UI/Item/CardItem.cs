
using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CardItem : MonoBehaviour
    {

        [CustomLabel("是否需要显示首发标记")]
        [SerializeField] public bool needShowStarterSign = false;
        [SerializeField] public Image imgPos;

        // 球员姓名
        [SerializeField] private TMP_Text nameText;
        // 球员实力
        [SerializeField] private TMP_Text combatEffectivenessText;
        // 卡片图片
        [SerializeField] private Image cardImg;
        // 旗帜图片
        [SerializeField] private Image flagImg;
        // 足球颜色
        [SerializeField] private Image ballImg;
        // 边框颜色
        //[SerializeField] private Image borderImg;
        // 卡牌背面图片
        [SerializeField] private Image backImg;
        // 星级
        [SerializeField] private StarListItem starListItem;
        // 流光效果
        [SerializeField] private UIShiny shiny;
        //解雇图片
        [SerializeField] private Button fireButton;
        // 卡牌质量
        public int Quality { get; private set; }
        public CardItemAnim Anim;

        [SerializeField] private GameObject maxPanel;
        [SerializeField] private Image smallCardLowQ;

        [SerializeField] private Image smallCardL2H;
        [SerializeField] private Image smallCardHighQ;

        // 球员头像1
        [SerializeField] private Image playerImg1;
        // 球员头像遮罩1
        [SerializeField] private GameObject playerImgMask1;

        // 球员头像2
        [SerializeField] private Image playerImg2;
        // 球员金色背景
        [SerializeField] private Image YellowBgImage;
        [SerializeField] private Image reddot;
        // 球员头像遮罩2
        [SerializeField] private GameObject playerImgMask2;

        private PlayerCard Card
        {
            get;
            set;
        }

        public void SetDataAndHideLeftStar(PlayerCard card)
        {
            Card = card;
            if (needShowStarterSign == false)
            {
                ballImg.gameObject.SetActive(false);
            }
            else
            {
                // 首发标记
                ballImg.gameObject.SetActive(card.IsStarter());
            }
            SetBaseData(card.Config, card.Quality);
            // 设置星级
            starListItem.SetStarAndHideLeftForeground(card.Star);
            // 设置图片
            starListItem.SetImage(card.Quality);
            // 设置球员号码
            //combatEffectivenessText.text = card.FightPoint.ToString();
            combatEffectivenessText.text = card.PlayerCardNumber.ToString();
        }


        /**
        *showFakeInfo 是否显示表示升阶上来的标记
        **/
        public void SetData(PlayerCard card, bool showFakeInfo = true, bool showRedDot = false)
        {
            Card = card;

            RefreshSelect();
            RefreshCollectionState();

            if (showRedDot)
            {
                if (card.IsStarter())
                {
                    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "/" + card.CardId.ToString());
                    node.IsRed(reddot.transform);
                }
            }

            //引导特殊卡牌记录
            if (card.Config.Id == GuideManager.UpLevelCardID)
            {
                GuideManager.UpLevelCardItem = this;
            }

            if (needShowStarterSign == false)
            {
                ballImg.gameObject.SetActive(false);
            }
            else
            {
                // 首发标记
                ballImg.gameObject.SetActive(card.IsStarter());
            }
            SetBaseData(card.Config, card.Quality);

            if (card.IsMaxAndZeroStar() == true)
            {
                starListItem.HideAllStar();
                if (showFakeInfo == true)
                {
                    maxPanel.SetActive(true);

                    //smallCardLowQ.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.SmallQualityCard, card.Config.Quality);
                    //smallCardL2H.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.SmallQualityL2H, card.Config.Quality);
                    //smallCardHighQ.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.SmallQualityCard, card.Quality);

                    string smallCardLowQName = SpriteNames.Card.SmallQualityCard.Replace("{quality}", card.Config.Quality.ToString());
                    SpriteManager.GetSprite(AtlasNames.Card, smallCardLowQName, s => { if (s != smallCardLowQ.sprite) { smallCardLowQ.sprite = s; } });

                    string smallCardL2HName = SpriteNames.Card.SmallQualityL2H.Replace("{quality}", card.Config.Quality.ToString());
                    SpriteManager.GetSprite(AtlasNames.Card, smallCardL2HName, s => { if (s != smallCardL2H.sprite) { smallCardL2H.sprite = s; } });

                    string smallCardHighQName = SpriteNames.Card.SmallQualityCard.Replace("{quality}", card.Quality.ToString());
                    SpriteManager.GetSprite(AtlasNames.Card, smallCardHighQName, s => { if (s != smallCardHighQ.sprite) { smallCardHighQ.sprite = s; } });
                }
            }
            else
            {
                // 设置星级
                starListItem.SetLevel(card.Star);
                maxPanel.SetActive(false);
            }

            // 设置图片
            starListItem.SetImage(card.Quality);
            // 设置球员评价
            //combatEffectivenessText.text = card.FightPoint.ToString();
            combatEffectivenessText.text = card.PlayerCardNumber.ToString();

        }

        public void SetConfigShow(CardModelConfig config, int star = 0)
        {
            SetBaseData(config, config.Quality);
            // 设置星级
            starListItem.SetLevel(star);
            // 设置图片
            starListItem.SetImage(config.Quality);
            var positionCfg = Configs.SeparatedPosition.GetConfig(config.AdaptPosition[0]);
            int sum = 0;
            for (int i = AbilityId.Shoot; i <= AbilityId.Will; i++)
            {
                sum += config.Ability[i] * positionCfg.AbilityRatio[i];
            }
            sum /= GameConst.ABILITY_NORMAL;
            //加特技加成分
            if (config.Skills.Length > 0)
            {
                //var tempList = SkillDic.ToList();
                for (int i = 0; i < config.Skills.Length; i++)
                {

                    if (config.Skills[i] == 0) continue;
                    SkillConfig sc = Configs.Skill.GetConfig(config.Skills[i]);
                    if (sc != null)
                        sum += sc.EffectaddValue[0];
                    else
                    {
                        Debug.LogError("skill id error " + config.Skills[i]);
                    }
                }
            }


            int iSum = (int)Mathf.Floor(sum + 0.5f) + config.ExtraAbility;



            //combatEffectivenessText.text = iSum.ToString();
            combatEffectivenessText.text = config.Number.ToString();

        }

        //npc
        public void SetNpcShow(ChallengePlayerConfig challengePlayerConfig, int separatedPosition)
        {
            SetNpcData(challengePlayerConfig);
            starListItem.SetLevel(challengePlayerConfig.Star);
            starListItem.SetImage(challengePlayerConfig.Quality);
            // var positionCfg = Configs.SeparatedPosition.GetConfig(separatedPosition);
            // int sum = 0;
            // for (int i = AbilityId.Shoot; i <= AbilityId.Will; i++)
            // {
            //     sum += challengePlayerConfig.Ability[i] * positionCfg.AbilityRatio[i];
            // }
            // sum /= GameConst.ABILITY_NORMAL;
            //combatEffectivenessText.text = ((int)Mathf.Floor(sum + 0.5f)).ToString();

            nameText.text = challengePlayerConfig.Name;
            SeparatedPositionConfig separatedPositionConfig = Configs.SeparatedPosition.GetConfig(separatedPosition);
            //positionText.text = separatedPositionConfig.Abbreviation;
            SpriteManager.GetSprite(AtlasNames.CardUp, "pos" + separatedPositionConfig.Id.ToString(), s => { if (imgPos.sprite != s) { imgPos.sprite = s; } });
            combatEffectivenessText.text = challengePlayerConfig.Number.ToString();

        }

        private async void SetNpcData(ChallengePlayerConfig challengePlayerConfig)
        {
            playerImg1.sprite = await SpriteProxy.GetNpcPortrait(challengePlayerConfig.Portrait);
            playerImgMask1.gameObject.SetActive(true);
            playerImgMask2.gameObject.SetActive(false);

            nameText.text = challengePlayerConfig.Name;
            // 设置球员位置
            //positionText.text = GetPositionSeparatedShortName(challengePlayerConfig);
            SpriteManager.GetSprite(AtlasNames.CardUp, "pos" + challengePlayerConfig.AdaptPosition[0].ToString(), s => { if (imgPos.sprite != s) { imgPos.sprite = s; } });
            // 设置卡片颜色
            //cardImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Background, challengePlayerConfig.Quality);
            string cardImgName = SpriteNames.Card.Background.Replace("{quality}", challengePlayerConfig.Quality.ToString());
            SpriteManager.GetSprite(AtlasNames.Card, cardImgName, s => { if (s != cardImg.sprite) { cardImg.sprite = s; } });
            // 设置旗帜颜色
            //flagImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Flag, challengePlayerConfig.Quality);
            // 设置上阵图片
            //ballImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.OnFormation, challengePlayerConfig.Quality);
            ballImg.sprite = await SpriteManager.GetSprite(AtlasNames.Card, "OnFormation");
            // 设置边框颜色
            //borderImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Border, challengePlayerConfig.Quality);
            //string borderImgName = SpriteNames.Card.Border.Replace("{quality}", challengePlayerConfig.Quality.ToString());
            //SpriteManager.GetSprite(AtlasNames.Card, borderImgName, s =>
            //{
            //    if (s != borderImg.sprite)
            //    {
            //        borderImg.sprite = s;
            //    }
            //});
            // 设置背面图片
            //backImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Back, challengePlayerConfig.Quality);
            string backImgName = SpriteNames.Card.Back.Replace("{quality}", challengePlayerConfig.Quality.ToString());
            SpriteManager.GetSprite(AtlasNames.Card, backImgName, s =>
            {
                if (s != backImg.sprite)
                {
                    backImg.sprite = s;
                }
            });
            // 只有金色和红色牌才有流光效果
            if (shiny != null)
            {
                shiny.enabled = challengePlayerConfig.Quality >= QualityType.Orange;
                shiny.Play();
            }
            Quality = challengePlayerConfig.Quality;
        }

        [SerializeField] private RectTransform peakBorder = null;
        [SerializeField] private List<Image> peakBorderImageList = null;
        [SerializeField] private RectTransform peakYear = null;
        [SerializeField] private List<Image> peakYearBgImageList = null;
        [SerializeField] private TMP_Text peakYearText = null;
        [SerializeField] public PeakImage peakImage = null;
        private async void SetBaseData(CardModelConfig config, int quality)
        {
            peakImage.peakImage.SetAlpha(1);
            // 设置球员头像
            SetPlayerHead(config);

            // 设置球员姓名
            nameText.text = config.Name;
            //这里实在没法带颜色
            //nameText.color = Card.NameColor;
            // 设置球员位置
            //positionText.text = GetPositionSeparatedShortName(config);
            //imgPos.sprite = await SpriteManager.GetSprite(AtlasNames.CardUp, "pos" + config.AdaptPosition[0].ToString());
            SpriteManager.GetSprite(AtlasNames.CardUp, "pos" + config.AdaptPosition[0].ToString(), sprite =>
            {
                if (imgPos.sprite != sprite)
                {
                    imgPos.sprite = sprite;
                }
            });
            // 设置卡片颜色
            //cardImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Background, quality);
            string cardImgName = SpriteNames.Card.Background.Replace("{quality}", quality.ToString());
            SpriteManager.GetSprite(AtlasNames.Card, cardImgName, sprite =>
            {
                if (cardImg.sprite != sprite)
                {
                    cardImg.sprite = sprite;
                }
            });

            //设置巅峰卡边框//设置巅峰卡年限
            bool isPeak = PlayerCard.IsPeak(config);
            peakBorder.gameObject.SetActive(isPeak);
            peakYear.gameObject.SetActive(isPeak);
            peakImage.SetData(config);
            if (isPeak)
            {
                for (int i = 0; i < peakBorderImageList.Count; i++)
                {
                    bool isThisQuality = i + 1 == quality;
                    peakBorderImageList[i]?.gameObject.SetActive(isThisQuality);
                    peakYearBgImageList[i]?.gameObject.SetActive(isThisQuality);
                }
                peakYearText.text = config.PeakYear;
            }

            // 设置旗帜颜色
            //flagImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Flag, quality);
            // 设置足球颜色
            //ballImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Ball, quality);
            ballImg.sprite = await SpriteManager.GetSprite(AtlasNames.Card, "OnFormation");
            // 设置边框颜色
            //borderImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Border, quality);
            //string borderImgName = SpriteNames.Card.Border.Replace("{quality}", quality.ToString());
            //SpriteManager.GetSprite(AtlasNames.Card, borderImgName, sprite =>
            //{
            //    if (borderImg.sprite != sprite)
            //    {
            //        borderImg.sprite = sprite;
            //    }
            //});
            // 设置背面图片
            //backImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Back, quality);
            string backImgName = SpriteNames.Card.Back.Replace("{quality}", quality.ToString());
            SpriteManager.GetSprite(AtlasNames.Card, backImgName, sprite =>
            {
                if (backImg.sprite != sprite)
                {
                    backImg.sprite = sprite;
                }
            });
            // 只有金色和红色牌才有流光效果
            if (shiny != null)
            {
                shiny.enabled = quality >= QualityType.Orange;
                shiny.Play();
            }
            Quality = quality;
        }
        private async void SetPlayerHead(CardModelConfig config)
        {
            bool isYellow = config.Quality >= 4;
            playerImgMask1.gameObject.SetActive(!isYellow);
            playerImgMask2.gameObject.SetActive(isYellow);
            if (isYellow == false)
            {
                SpriteManager.GetSprite(AtlasNames.Portrait, config.Portrait.ToString(), sprite =>
                {
                    if (playerImg1.sprite != sprite)
                    {
                        playerImg1.sprite = sprite;
                    }
                });
                //playerImg1.sprite = await SpriteProxy.GetPlayerPortrait(config.Portrait);
            }
            else
            {
                SpriteManager.GetSprite(AtlasNames.PortraitYellow, config.Portrait.ToString(), sprite =>
                {
                    if (playerImg2.sprite != sprite)
                    {
                        playerImg2.sprite = sprite;
                    }
                });
                //playerImg2.sprite = await SpriteProxy.GetPlayerPortraitYellow(config.Portrait);
                int yellowBgId = config.CardYellowBg;
                if (yellowBgId <= 0) yellowBgId = 1;
                YellowBgImage.sprite = await SpriteProxy.GetPlayerCardYellowBg(yellowBgId);
            }
        }

        private string GetPositionName(CardModelConfig config)
        {
            var cfg = Configs.Position.GetConfig(config.Position);
            if (cfg == null) return "";
            return cfg.Name;
        }

        private string GetPositionSeparatedShortName(CardModelConfig config)
        {
            var cfg = Configs.SeparatedPosition.GetConfig(config.AdaptPosition[0]);
            if (cfg == null) return "";
            return cfg.Abbreviation;
        }
        private string GetPositionSeparatedShortName(ChallengePlayerConfig challengePlayerConfig)
        {
            var cfg = Configs.SeparatedPosition.GetConfig(challengePlayerConfig.AdaptPosition[0]);
            if (cfg == null) return "";
            return cfg.Abbreviation;
        }

        public void SetPlayerEffect(PlayerCard card)
        {
            Card = card;
            //combatEffectivenessText.text = card.FightPoint.ToString();
            combatEffectivenessText.text = card.PlayerCardNumber.ToString();
        }

        public void SetTestConfigPiecesShow(CardModelConfig config)
        {
            SetBaseData(config, config.Quality);
            // 设置球员姓名
            nameText.text = PlayerCard.GetFullName(config) + "碎片";
        }
        // 显示背面
        public void ShowBack()
        {
            backImg.gameObject.SetActive(true);
        }

        // 隐藏背面
        public void HidBack()
        {
            backImg.gameObject.SetActive(false);
            ResetBack();
        }

        public void BackToWhite()
        {
            backImg.gameObject.SetActive(true);
            var effect = backImg.GetComponent<UIEffect>();
            if (effect != null)
            {
                DOTween.To(value => effect.colorFactor = value, 0, 0.8f, 0.5f);
            }
            else
            {
                Debug.Log("Effect = null");
            }
        }

        public void ResetBack()
        {
            var effect = backImg.GetComponent<UIEffect>();
            if (effect != null)
            {
                effect.colorFactor = 0;
            }
        }


        //最高等级的时候隐藏一些信息
        public void HideFlagAndPositonText()
        {
            flagImg.gameObject.SetActive(false);
            //positionText.gameObject.SetActive(false);
            fireButton.gameObject.SetActive(false);

        }

        public void ResetFlagAndPositonText()
        {
            flagImg.gameObject.SetActive(true);
            //positionText.gameObject.SetActive(true);
            fireButton.gameObject.SetActive(false);
        }

        public void HideFakeInfo()
        {
            maxPanel.SetActive(false);
        }

        private void OnEnable()
        {
            fireButton.onClick.AddListener(OnClickFire);
            viewButton.onClick.AddListener(OnClickViewButton);
        }

        private void OnDisable()
        {
            fireButton.onClick.RemoveListener(OnClickFire);
            viewButton.onClick.RemoveListener(OnClickViewButton);
        }

        private void OnClickFire()
        {
            //UIUtils.CloseAllPanels();
            //UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.Fire));
            // UIController.Instance.OpenWindow<PlayerFireUI>(new PlayerFireUIProperties(Card));
        }

        public bool isSelf = true;
        [SerializeField] private Button viewButton = null;
        private void OnClickViewButton()
        {
            if (!isSelf)
            {
                UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(Card, false));
            }
        }

        #region 数字藏品

        public bool isUsingInCillectionUI = false;

        [SerializeField] private RectTransform collection = null;
        [SerializeField] private Image collectionDarkImageNormalLow = null;
        [SerializeField] private Image collectionDarkImageNormalRed = null;
        [SerializeField] private Image collectionDarkImagePeak = null;
        [SerializeField] private Image collectionDarkImageStar = null;
        [SerializeField] private Image collectionBgImage = null;
        [SerializeField] private TMP_Text collectionInitText = null;
        [SerializeField] private TMP_Text collectionSaleText = null;
        [SerializeField] private Image selectImage = null;
        [SerializeField] private Image collectionUsingInFormation = null;

        public void RefreshCollectionState()
        {
            if (isUsingInCillectionUI == false)
            {
                collection.gameObject.SetActive(false);
                return;
            }

            reddot.gameObject.SetActive(false);

            if (!Card.isCollectionCard)
            {
                collectionDarkImageNormalLow.gameObject.SetActive(false);
                collectionDarkImageNormalRed.gameObject.SetActive(false);
                collectionDarkImagePeak.gameObject.SetActive(false);
                collectionDarkImageStar.gameObject.SetActive(false);
                collectionBgImage.gameObject.SetActive(false);
                collectionInitText.gameObject.SetActive(false);
                collectionSaleText.gameObject.SetActive(false);
                collectionUsingInFormation.gameObject.SetActive(IsUsingInFormation());
                return;
            }

            int quality = Card.Quality;
            bool isPeak = Card.IsPeak();
            collectionDarkImageNormalLow.gameObject.SetActive(quality <= QualityType.Orange && !isPeak);
            collectionDarkImageNormalRed.gameObject.SetActive(quality > QualityType.Orange && !isPeak);
            collectionDarkImagePeak.gameObject.SetActive(isPeak && Card.Config.PeakLogo == "Peak");
            collectionDarkImageStar.gameObject.SetActive(isPeak && Card.Config.PeakLogo == "AllStar");
            collectionBgImage.gameObject.SetActive(true);
            collectionInitText.gameObject.SetActive(Card.PropStatus == 0);
            collectionSaleText.gameObject.SetActive(Card.PropStatus == 1);
            collectionUsingInFormation.gameObject.SetActive(false);
        }

        public void PlayHighlightAnim()
        {
            if (isUsingInCillectionUI == false)
            {
                return;
            }
            if (selectImage.gameObject.activeSelf == false)
            {
                return;
            }
            selectImage.DOKill();
            selectImage.transform.DOKill();
            selectImage.SetAlpha(0);
            selectImage.transform.localScale = Vector3.one * 1.8f;
            selectImage.DOFade(1, 0.2f).AddTo(this.gameObject);
            selectImage.transform.DOScale(1.4f, 0.2f).AddTo(this.gameObject);
        }

        public void RefreshSelect()
        {
            if (isUsingInCillectionUI == false)
            {
                selectImage.gameObject.SetActive(false);
                return;
            }
            bool isSelect = Card == CollectionManager.Instance.selectedPlayerCard;
            selectImage.gameObject.SetActive(isSelect);
            if (isSelect)
            {
                selectImage.DOKill();
                selectImage.transform.DOKill();
                selectImage.SetAlpha(1f);
                selectImage.transform.localScale = Vector3.one * 1.4f;
            }
        }

        private bool IsUsingInFormation()
        {
            if (Card.IsStarter())
            {
                //Tips.PopTips($"经典赛首发球员不能解雇。");
                return true;
            }
            if (Card.IsStarter1())
            {
                //Tips.PopTips($"赛事首发球员不能解雇。");
                return true;
            }
            if (Card.IsStarter2())
            {
                //Tips.PopTips($"排位赛首发球员不能解雇。");
                return true;
            }
            if (Card.IsStarter3())
            {
                //Tips.PopTips($"篮球殿堂首发球员不能解雇。");
                return true;
            }
            if (Card.IsStarter4())
            {
                //Tips.PopTips($"百分大战上场球员不能解雇。");
                return true;
            }
            if (Card.IsUsingInBounty)
            {
                //Tips.PopTips($"悬赏任务已派遣球员不能解雇。");
                return true;
            }
            if (Card.SkillTrainRoomId != 0)
            {
                //Tips.PopTips($"特级训练中的球员不能解雇。");
                return true;
            }
            return false;
        }

        #endregion

    }
}
