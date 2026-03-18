using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    [System.Serializable]
    public class CardUpUIProperties : PanelProperties
    {
        public PlayerCard playerCard { get; set; }
        public List<PlayerCard> playerCardList { get; set; }
        public CardUpUI.SubUIID SubUI = CardUpUI.SubUIID.Level;
        public CardUpUIProperties(PlayerCard card, CardUpUI.SubUIID ui = CardUpUI.SubUIID.Level)
        {
            playerCardList = Player.CardManager.GetCardList();
            playerCard = card;
            SubUI = ui;
        }
    }

    public class CardUpUI : APanelController<CardUpUIProperties>
    {
        #region 初始化
        public enum SubUIID
        {
            Level = 0,
            Break = 1,
            Star = 2
        }

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            btnTupo.OnClick += OnTupo;
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.RefreshWindow, RefreshWindow);
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            btnUpStar.OnClick += OnUpstar;
            autoUpLevelBtn.OnClick += OnClickAutoUpLevelBtn;
            statusBtn[0].OnClick += AdjustPlayerStatus0;
            statusBtn[1].OnClick += AdjustPlayerStatus1;
            statusBtn[2].OnClick += AdjustPlayerStatus2;
            btnPreviewSkill.onClick.AddListener(PreviewSkill);
            btnPreviewSkill1.OnClick += PreviewSkill;
            PlayerNumber.onClick.AddListener(OnChangeNumber);
            dragActionComponent.DragBeginAction += OnBeginDrag;
            dragActionComponent.DragMoveAction += OnDrag;
            dragActionComponent.DragEndAction += OnEndDrag;
            EventManager.Instance.Register(EventID.RefreshCardRecoverProperties, RefreshCardRecoverProperties);
            EventManager.Instance.Register(EventID.OnCardUpgradeStar, RefreshCardRecoverProperties);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            btnTupo.OnClick -= OnTupo;
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Unregister(EventID.RefreshWindow, RefreshWindow);

            statusBtn[0].OnClick -= AdjustPlayerStatus0;
            statusBtn[1].OnClick -= AdjustPlayerStatus1;
            statusBtn[2].OnClick -= AdjustPlayerStatus2;
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
            btnUpStar.OnClick -= OnUpstar;
            autoUpLevelBtn.OnClick -= OnClickAutoUpLevelBtn;
            btnPreviewSkill.onClick.RemoveListener(PreviewSkill);
            btnPreviewSkill1.OnClick -= PreviewSkill;
            PlayerNumber.onClick.RemoveListener(OnChangeNumber);
            dragActionComponent.DragBeginAction -= OnBeginDrag;
            dragActionComponent.DragMoveAction -= OnDrag;
            dragActionComponent.DragEndAction -= OnEndDrag;
            EventManager.Instance.Unregister(EventID.RefreshCardRecoverProperties, RefreshCardRecoverProperties);
            EventManager.Instance.Unregister(EventID.OnCardUpgradeStar, RefreshCardRecoverProperties);
        }

        [SerializeField] private CardUpUIGuide cardUpUIGuide;
        [SerializeField] private List<Image> redList;
        protected override void OnPropertiesSet()
        {
            RefreshUI();
        }
        private void RefreshUI()
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);

            //注意刷数据这个行为都放到刷红点的事件中了，主要是因为调用不规范，之前不少操作会刷多次。
            RefreshCardStatePanel();
            RefreshCardTrainInfoItem();
            bottomToggleGroup.Switch((int)Properties.SubUI);
            //ShowPad(Properties.SubUI);
            cardUpUIGuide.CheckGuide();
            canDragChangePlayer = (GuideManager.IsGuideDoing(GuideID.guideUpLevelPlayer) == false);

            OnlyRefreshRedDot();

            TouchManager.Instance.EnableTouch();
        }

        private void RefreshCardRecoverProperties(object[] _)
        {
            RefreshCardStatePanel();
        }
        #endregion

        #region 关闭界面
        [SerializeField] private Button closeBtn;
        private void OnClose()
        {
            closeBtn.GetComponent<ButtonAnim>().PlayBack(() => UIController.Instance.HidePanel<CardUpUI>(),
            playAudio: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            });
        }
        #endregion

        #region 切换页签

        [SerializeField] private BabuToggleGroup bottomToggleGroup;
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            ShowPad((SubUIID)selectedIndex);
        }
        private void ShowPad(SubUIID padIndex)
        {
            Properties.SubUI = padIndex;
            HideAllPad();
            switch (padIndex)
            {
                case SubUIID.Level: OnShowLevel(); CbaLogManager.Instance.AddLog(1032); break;
                case SubUIID.Break: OnShowBreak(); CbaLogManager.Instance.AddLog(1033); break;
                case SubUIID.Star: OnShowStar(); CbaLogManager.Instance.AddLog(1034); break;
            }
        }

        [SerializeField] private RectTransform cardStatePanel = null;
        [SerializeField] private CardTrainInfoItem cardTrainInfoItem = null;
        [SerializeField] private TMP_Text limitByTeamLevelText = null;
        [SerializeField] private TMP_Text limitByMaxLevel = null;
        [SerializeField] private HorizontalLayoutGroup costPanel = null;
        [SerializeField] private RectTransform levelPanel = null;
        [SerializeField] private RectTransform breakPanel = null;
        [SerializeField] private RectTransform starPanel = null;
        private void HideAllPad()
        {
            cardTrainInfoItem.gameObject.SetActive(false);
            limitByTeamLevelText.gameObject.SetActive(false);
            limitByMaxLevel.gameObject.SetActive(false);
            costPanel.gameObject.SetActive(false);
            levelPanel.gameObject.SetActive(false);
            breakPanel.gameObject.SetActive(false);
            starPanel.gameObject.SetActive(false);
        }

        #endregion

        #region 球员公共基本信息和状态信息

        [SerializeField] private ImageFont levelImageFont = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private RectTransform peakYearPanel = null;
        [SerializeField] private TMP_Text peakNameText = null;
        [SerializeField] private TMP_Text peakYearText = null;
        [SerializeField] private PeakImage peakImage = null;
        [SerializeField] private TMP_Text txtEquipGrade = null;
        [SerializeField] private ImageFont breakNumImageFont = null;
        [SerializeField] private ImageFont imgFightPoint = null;
        //[SerializeField] private TMP_Text breakAddNumText = null;
        [SerializeField] private TMP_Text numberText = null;
        [SerializeField] private TMP_Text energyPercentText = null;
        [SerializeField] private TMP_Text hurtNumText = null;
        [SerializeField] private TMP_Text stateNumText = null;
        [SerializeField] private TMP_Text txtPosition = null;
        [SerializeField] private Image stateImage = null;
        [SerializeField] private List<BabuButton> statusBtn;
        [SerializeField] private List<TMP_Text> txtWarningList;
        [SerializeField] private Button PlayerNumber;
        [SerializeField] private Image isStarterImage = null;

        private async void RefreshCardStatePanel()
        {
            Player.CalFightPoint_Single(Properties.playerCard.CardId, true);
            SetPlayerHead(Properties.playerCard.Quality, Properties.playerCard.Config.Portrait, Properties.playerCard.Config.Quality);
            SetPlayerStar(Properties.playerCard.Star);
            levelImageFont.text = Properties.playerCard.Level.ToString();
            bool isPeak = PlayerCard.IsPeak(Properties.playerCard.Config);
            nameText.gameObject.SetActive(!isPeak);
            peakYearPanel.gameObject.SetActive(isPeak);
            if (!isPeak)
            {
                nameText.text = PlayerCard.GetFullName(Properties.playerCard.Config);
                nameText.color = Properties.playerCard.NameColor;
            }
            else
            {
                peakImage.SetData(Properties.playerCard);
                peakNameText.text = Properties.playerCard.Config.Name;
                peakYearText.text = Properties.playerCard.Config.PeakYear;
                peakNameText.color = Properties.playerCard.NameColor;
            }
            numberText.text = Properties.playerCard.PlayerCardNumber.ToString();

            int equipGrade = Properties.playerCard.EquipGrade;
            //breakNumImageFont.text = "{0}阶".SafeFormat(equipGrade);
            imgFightPoint.text = Properties.playerCard.FightPoint.ToString();
            txtEquipGrade.text = "+" + equipGrade.ToString();
            int position = (int)Properties.playerCard.GetAdaptPosition();

            SeparatedPositionConfig separatedPositionConfig = Configs.SeparatedPosition.GetConfig(position);
            txtPosition.text = separatedPositionConfig.Abbreviation;

            int equipGradeId = position * 1000 + equipGrade;
            //JerseyBreakConfig jerseyBreakConfig = Configs.JerseyBreak.GetConfig(equipGradeId);
            //breakAddNumText.text = jerseyBreakConfig != null ? jerseyBreakConfig.CardNameSuffix : "";

            if (Properties.playerCard.Energy < GameConst.CardSingleEnergyWarning)
            {
                txtWarningList[0].gameObject.SetActive(true);
                txtWarningList[0].GetComponent<LoomAnim>().PlayText(0.5f, 0.5f, 0.3f);
            }
            else
            {
                txtWarningList[0].gameObject.SetActive(false);
                txtWarningList[0].GetComponent<LoomAnim>().Stop();
            }
            //energyPercentText.text = "{0}%(储备{1}%)".SafeFormat(Properties.playerCard.SingleEnergyRatio.ToString("f2"), Properties.playerCard.BackupEnergyRatio.ToString("f2"));
            energyPercentText.text = "{0}%".SafeFormat(Properties.playerCard.TotalEnergyRatio.ToString("f2"));

            if ((int)Properties.playerCard.InjuryType > 1)
            {
                txtWarningList[1].gameObject.SetActive(true);
                txtWarningList[1].GetComponent<LoomAnim>().PlayText(0.5f, 0.5f, 0.3f);
            }
            else
            {
                txtWarningList[1].gameObject.SetActive(false);
                txtWarningList[1].GetComponent<LoomAnim>().Stop();
            }
            hurtNumText.text = Properties.playerCard.GatPlayerCardInjuryTypeStr();

            if ((int)Properties.playerCard.Status < 3)
            {
                txtWarningList[2].gameObject.SetActive(true);
                txtWarningList[2].GetComponent<LoomAnim>().PlayText(0.5f, 0.5f, 0.3f);
            }
            else
            {
                txtWarningList[2].gameObject.SetActive(false);
                txtWarningList[2].GetComponent<LoomAnim>().Stop();
            }
            stateNumText.text = Properties.playerCard.GetPlayerCardStatusStr();

            stateImage.sprite = await Properties.playerCard.GetPlayerCardStatusSprite();

            isStarterImage.gameObject.SetActive(Properties.playerCard.IsStarter());
        }

        private void AdjustPlayerStatus0(BabuButton obj)
        {
            UIController.Instance.OpenWindow<PlayerRecoverUI>(new PlayerRecoverUIProperties(Properties.playerCard, PlayerRecoverType.RecoverEnergy));
        }

        private void AdjustPlayerStatus1(BabuButton obj)
        {
            UIController.Instance.OpenWindow<PlayerRecoverUI>(new PlayerRecoverUIProperties(Properties.playerCard, PlayerRecoverType.RecoverMedical));
        }


        private void OnChangeNumber()
        {
            UIController.Instance.OpenWindow<ChangeNumberUI>(new ChangeNumberUIProperties(Properties.playerCard, numberText));
        }

        private void AdjustPlayerStatus2(BabuButton obj)
        {
            UIController.Instance.OpenWindow<PlayerRecoverUI>(new PlayerRecoverUIProperties(Properties.playerCard, PlayerRecoverType.Coach2State));
        }

        [SerializeField] private Image cardBackImage = null;
        [SerializeField] private Image playerImgMask1 = null;
        [SerializeField] private Image playerImg1 = null;
        [SerializeField] private Image playerImgMask2 = null;
        [SerializeField] private Image playerImg2 = null;
        private async void SetPlayerHead(int quality, int portrait, int oldQuality)
        {
            cardBackImage.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.SquareBack, quality);
            bool isYellow = oldQuality >= 4;
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

        [SerializeField] private List<GameObject> playerStarList = new();
        private async void SetPlayerStar(int star)
        {
            if (star > 5)
            {
                int colorfulStarCount = star - 5;
                for (int i = 0; i < playerStarList.Count; i++)
                {
                    playerStarList[i].SetActive(true);
                    if (i + 1 <= colorfulStarCount)
                        playerStarList[i].GetComponent<Image>().sprite = await SpriteProxy.GetColorfulStar();
                    else
                        playerStarList[i].GetComponent<Image>().sprite = await SpriteProxy.GetYellowStar();
                }
            }
            else
            {
                for (int i = 0; i < playerStarList.Count; i++)
                {
                    playerStarList[i].SetActive(i + 1 <= star);
                    playerStarList[i].GetComponent<Image>().sprite = await SpriteProxy.GetYellowStar();
                }
            }
        }

        #endregion

        #region 球员公共属性信息

        private void RefreshCardTrainInfoItem(bool playAni = false)
        {
            if (playAni)
            {
                cardTrainInfoItem.SetDataCmp(Properties.playerCard, playAni);
                cardTrainInfoItem.PlayAddScoreAnim();
            }
            else
            {
                cardTrainInfoItem.SetDataShow(Properties.playerCard);
            }
        }

        private void OnlyRefreshRedDot()
        {
            if (Properties.playerCard.IsStarter())
            {
                Player.CardManager.CheckRedDot(Properties.playerCard.CardId);
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "/" + Properties.playerCard.CardId.ToString() + "/lv");
                node.IsRed(redList[0].transform);
                node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "/" + Properties.playerCard.CardId.ToString() + "/equip");
                node.IsRed(redList[1].transform);
                node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "/" + Properties.playerCard.CardId.ToString() + "/star");
                node.IsRed(redList[2].transform);
            }
            else
            {
                redList[0].gameObject.SetActive(false);
                redList[1].gameObject.SetActive(false);
                redList[2].gameObject.SetActive(false);
            }
        }

        private void RefreshRedDot(object[] args = null)
        {
            RefreshCardStatePanel();
            RefreshCardTrainInfoItem(true);
            OnlyRefreshRedDot();
        }

        #endregion

        #region 升级

        [SerializeField] private List<CardUpLevelGoodsUseItem> levelItemList = new();
        private bool isLevelInited = false;
        private int maxLevel = 0;
        private void OnShowLevel()
        {
            cardTrainInfoItem.gameObject.SetActive(true);
            levelPanel.gameObject.SetActive(true);
            if (!isLevelInited)
            {
                maxLevel = Configs.CardLevel.GetConfigList()[^1].Id;
                isLevelInited = true;
                for (int i = 0; i < 4; i++)
                {
                    CardUpLevelGoodsUseItem levelItem = levelItemList[i];
                    int goodsId = GoodsId.CardUpLevelGoodsId[i];
                    levelItem.SetData(goodsId, OnLevelItemClick, OnLevelItemClickEnd);
                }
            }
            RefreshLevelAndExp();
            ResetLevelGoodsNum();
            RefreshLevelUpButton();
            useGoodsDic.Clear();
            nowFakeExp = -1;
            lastFakeLevel = -1;
        }
        private void RefreshLevelUpButton()
        {
            limitByTeamLevelText.gameObject.SetActive(false);
            limitByMaxLevel.gameObject.SetActive(false);
            autoUpLevelBtn.gameObject.SetActive(false);

            Tuple<int, float> checkLevelAndProgress = PlayerCard.GetLevelAndExpProgress(Properties.playerCard.Exp);

            if (checkLevelAndProgress.Item1 >= maxLevel)
            {
                limitByMaxLevel.gameObject.SetActive(true);
                return;
            }

            if (checkLevelAndProgress.Item1 > Player.Level)
            {
                limitByTeamLevelText.gameObject.SetActive(true);
                return;
            }

            autoUpLevelBtn.gameObject.SetActive(true);
        }
        private void ResetLevelGoodsNum()
        {
            for (int i = 0; i < 4; i++)
            {
                CardUpLevelGoodsUseItem levelItem = levelItemList[i];
                GameItem gameItem = levelItem.inventoryItem.GetGameItem();
                gameItem.Count = gameItem.GetPlayerCount();
                levelItem.inventoryItem.SetCount(gameItem.Count.ToString());
            }
        }

        [SerializeField] private Image levelProgressFgImage = null;
        [SerializeField] private RectTransform levelTextPanel = null;
        [SerializeField] private TMP_Text levelText = null;
        [SerializeField] private TMP_Text levelProgressText = null;
        private void RefreshLevelAndExp()
        {
            expProgressChangeSeq?.Kill();
            expProgressChangeSeq = DOTween.Sequence();
            Tuple<int, float> levelAndProgress = PlayerCard.GetLevelAndExpProgress(Properties.playerCard.Exp);
            int showLevel = Mathf.Min(levelAndProgress.Item1, maxLevel, Player.Level);
            bool isMaxLevel = levelAndProgress.Item1 >= maxLevel || levelAndProgress.Item1 > Player.Level;
            levelText.text = "<color=#13b237>{0}</color>/{1}".SafeFormat(showLevel, Mathf.Min(Player.Level, maxLevel));
            levelImageFont.text = Mathf.Max(Properties.playerCard.Level, showLevel).ToString();
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelTextPanel);
            levelProgressFgImage.fillAmount = isMaxLevel ? 1f : Properties.playerCard.ExpProgress;
            levelProgressText.gameObject.SetActive(levelAndProgress.Item1 < maxLevel);
            levelProgressText.SetAlpha(levelAndProgress.Item1 < maxLevel ? 1f : 0f);
            if (levelAndProgress.Item1 < maxLevel) levelProgressText.text = "{0}<color=#FBF17B>/{1}</color>".SafeFormat(Properties.playerCard.LevelNowExp, Properties.playerCard.LevelMaxExp);
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(levelText.transform.parent as RectTransform);
        }

        private Dictionary<int, int> useGoodsDic = new();
        private int nowFakeExp = -1;
        private int lastFakeLevel = -1;
        private Sequence expProgressChangeSeq = null;
        private bool OnLevelItemClick(CardUpLevelGoodsUseItem levelItem)
        {
            if (nowFakeExp == -1) nowFakeExp = Properties.playerCard.Exp;
            if (lastFakeLevel == -1) lastFakeLevel = Properties.playerCard.Level;
            int checkExp = nowFakeExp;
            Tuple<int, float> checkLevelAndProgress = PlayerCard.GetLevelAndExpProgress(checkExp);

            if (checkLevelAndProgress.Item1 >= maxLevel)
            {
                Tips.PopTips("已升至最高等级");
                return false;
            }
            if (checkLevelAndProgress.Item1 > Player.Level)
            {
                Tips.PopTips("请提升球队等级");
                return false;
            }
            GameItem gameItem = levelItem.inventoryItem.GetGameItem();
            if (gameItem.Count <= 0)
            {
                UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(gameItem.Type, gameItem.Id, 1));
                return false;
            }

            gameItem.Count--;
            levelItem.inventoryItem.SetCount(gameItem.Count.ToString());
            if (useGoodsDic.ContainsKey(gameItem.Id) == false) useGoodsDic.Add(gameItem.Id, 0);
            useGoodsDic[gameItem.Id]++;

            nowFakeExp += levelItem.goodsConfig.Param1;
            Tuple<int, float> levelAndProgress = PlayerCard.GetLevelAndExpProgress(nowFakeExp);

            int newLevel = Mathf.Min(levelAndProgress.Item1, Player.Level, maxLevel);

            expProgressChangeSeq?.Kill();
            expProgressChangeSeq = DOTween.Sequence();
            bool isMaxLevel = levelAndProgress.Item1 >= maxLevel || levelAndProgress.Item1 > Player.Level;
            bool isLevelChange = lastFakeLevel != newLevel;
            lastFakeLevel = newLevel;
            if (isLevelChange)
            {
                expProgressChangeSeq.Append(levelProgressFgImage.DOFillAmount(1f, 0.1f));
                expProgressChangeSeq.AppendCallback(() =>
                {
                    if (levelAndProgress.Item1 < maxLevel) levelProgressFgImage.fillAmount = 0;
                });
                if (!isMaxLevel) expProgressChangeSeq.Append(levelProgressFgImage.DOFillAmount(levelAndProgress.Item2, 0.1f));
            }
            else
            {
                expProgressChangeSeq.Append(levelProgressFgImage.DOFillAmount(isMaxLevel ? 1f : levelAndProgress.Item2, 0.2f));
            }
            levelText.text = "<color=#13b237>{0}</color>/{1}".SafeFormat(newLevel, Mathf.Min(Player.Level, maxLevel));
            levelImageFont.text = Mathf.Max(Properties.playerCard.Level, newLevel).ToString();

            expProgressChangeSeq.Insert(0f, levelProgressText.DOFade(0, 0.01f));
            if (levelAndProgress.Item1 < maxLevel)
            {
                levelProgressText.text = "{0}<color=#FBF17B>/{1}</color>".SafeFormat(PlayerCard.GetLevelNowExp(lastFakeLevel, nowFakeExp), PlayerCard.GetLevelMaxExp(lastFakeLevel));
                expProgressChangeSeq.Insert(0.01f, levelProgressText.DOFade(1, 0.04f));
            }

            return true;
        }

        private void OnLevelItemClickEnd(CardUpLevelGoodsUseItem levelItem)
        {
            bool hasUseGoods = false;
            foreach (var item in useGoodsDic)
            {
                if (item.Value > 0)
                {
                    hasUseGoods = true;
                    break;
                }
            }
            if (hasUseGoods == true)
            {
                bool isLevelUp = lastFakeLevel != Properties.playerCard.Level;
                int newExp = nowFakeExp;
                NetworkManager.Instance.CardUpgradeLevel(Properties.playerCard.CardId, useGoodsDic, (resp) =>
                {
                    if (resp.Succeed == false)
                    {
                        RefreshLevelAndExp();
                        ResetLevelGoodsNum();
                        Debug.LogWarningFormat("CardUpUI , OnLevelItemClickEnd , CardUpgradeLevel , resp.Succeed == false , Properties.playerCard.CardId = {0}", Properties.playerCard.CardId);
                    }
                    else
                    {
                        Properties.playerCard.Exp = newExp;
                        if (isLevelUp)
                        {
                            Tuple<int, float> levelAndProgress = PlayerCard.GetLevelAndExpProgress(Properties.playerCard.Exp);
                            bool isMaxLevel = levelAndProgress.Item1 >= maxLevel;
                            int newLevel = isMaxLevel ? maxLevel : levelAndProgress.Item1;
                            Properties.playerCard.Level = newLevel;
                            //TODO 数值变化表现
                            Player.CalFightPoint_Single(Properties.playerCard.CardId, true);
                            RefreshCardStatePanel();
                        }
                        RefreshRedDot();
                    }
                    RefreshLevelUpButton();
                });
            }
            useGoodsDic.Clear();
            nowFakeExp = -1;
            lastFakeLevel = -1;
        }

        [SerializeField] private BabuButton autoUpLevelBtn = null;
        private void OnClickAutoUpLevelBtn(BabuButton sender)
        {
            bool noUpLevelGoods = true;
            for (int i = 0; i < 4; i++)
            {
                CardUpLevelGoodsUseItem levelItem = levelItemList[i];
                GameItem gameItem = levelItem.inventoryItem.GetGameItem();
                if (gameItem.Count > 0)
                {
                    noUpLevelGoods = false;
                    break;
                }
            }
            if (noUpLevelGoods == true)
            {
                CardUpLevelGoodsUseItem levelItem = levelItemList[0];
                GameItem gameItem = levelItem.inventoryItem.GetGameItem();
                UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(gameItem.Type, gameItem.Id, 1));
                return;
            }

            int maxCardLevel = Player.Level + 1;
            if (maxCardLevel > maxLevel) maxCardLevel = maxLevel;
            CardLevelConfig cardLevelConfig = Configs.CardLevel.GetConfig(maxCardLevel);
            int maxExp = cardLevelConfig.ExpTotal;
            int nowExp = Properties.playerCard.Exp;
            int needExp = maxExp - nowExp;
            if (needExp < 0)
            {
                Tips.PopTips("已升至最高等级");
                return;
            }

            List<int> useCountList = new() { 0, 0, 0, 0 };
            int needExpNow = needExp;
            for (int k = 3; k >= 0; k--)//用大的先填满大部分
            {
                if (needExpNow <= 0) break;
                CardUpLevelGoodsUseItem levelItem = levelItemList[k];
                int hasCount = levelItem.inventoryItem.GetGameItem().Count;
                int needUseCount = needExpNow / levelItem.goodsConfig.Param1;
                if (needUseCount > hasCount) needUseCount = hasCount;
                needExpNow -= needUseCount * levelItem.goodsConfig.Param1;
                useCountList[k] = needUseCount;
                levelItem.inventoryItem.GetGameItem().Count = hasCount - needUseCount;
                levelItem.inventoryItem.SetCount(levelItem.inventoryItem.GetGameItem().Count.ToString());
            }

            for (int k = 0; k < 4; k++)//差一点点先用小得
            {
                if (needExpNow <= 0) break;
                CardUpLevelGoodsUseItem levelItem = levelItemList[k];
                int hasCount = levelItem.inventoryItem.GetGameItem().Count;
                int needUseCount = needExpNow / levelItem.goodsConfig.Param1;
                if (needUseCount > hasCount) needUseCount = hasCount;
                needExpNow -= needUseCount * levelItem.goodsConfig.Param1;
                if (needExpNow > 0 && hasCount > needUseCount)
                {
                    needUseCount++;
                    needExpNow -= levelItem.goodsConfig.Param1;
                }
                useCountList[k] += needUseCount;
                levelItem.inventoryItem.GetGameItem().Count = hasCount - needUseCount;
                levelItem.inventoryItem.SetCount(levelItem.inventoryItem.GetGameItem().Count.ToString());
            }

            int newExp = nowExp;
            Dictionary<int, int> useGoodsDicFast = new();//整理需要使用的道具键值对
            for (int k = 0; k < 4; k++)
            {
                CardUpLevelGoodsUseItem levelItem = levelItemList[k];
                GameItem gameItem = levelItem.inventoryItem.GetGameItem();
                int useCount = useCountList[k];
                if (useCount <= 0) continue;
                useGoodsDicFast.Add(levelItem.goodsConfig.Id, useCount);
                newExp += levelItem.goodsConfig.Param1 * useCount;
            }

            int oldLevel = Properties.playerCard.Level;
            NetworkManager.Instance.CardUpgradeLevel(Properties.playerCard.CardId, useGoodsDicFast, (resp) =>
            {
                if (resp.Succeed == false)
                {
                    RefreshLevelAndExp();
                    ResetLevelGoodsNum();
                    Debug.LogWarningFormat("CardUpUI , OnClickAutoUpLevelBtn , CardUpgradeLevel , resp.Succeed == false , Properties.playerCard.CardId = {0}", Properties.playerCard.CardId);
                }
                else
                {
                    expProgressChangeSeq?.Kill();
                    expProgressChangeSeq = DOTween.Sequence();
                    Properties.playerCard.Exp = newExp;
                    Tuple<int, float> levelAndProgress = PlayerCard.GetLevelAndExpProgress(Properties.playerCard.Exp);
                    int newLevel = Mathf.Min(levelAndProgress.Item1, Player.Level, maxLevel);
                    bool isLevelUp = oldLevel != newLevel;
                    bool isMaxLevel = levelAndProgress.Item1 >= maxLevel || levelAndProgress.Item1 > Player.Level;
                    if (isLevelUp)
                    {
                        Properties.playerCard.Level = newLevel;
                        expProgressChangeSeq.Append(levelProgressFgImage.DOFillAmount(1f, 0.1f));
                        expProgressChangeSeq.AppendCallback(() =>
                        {
                            if (!isMaxLevel) levelProgressFgImage.fillAmount = 0;
                            levelText.text = "<color=#13b237>{0}</color>/{1}".SafeFormat(newLevel, Mathf.Min(Player.Level, maxLevel));
                        });
                        if (!isMaxLevel) expProgressChangeSeq.Append(levelProgressFgImage.DOFillAmount(levelAndProgress.Item2, 0.1f));


                        Player.CalFightPoint_Single(Properties.playerCard.CardId, true);
                        RefreshCardStatePanel();
                        RefreshRedDot();
                    }
                    else
                    {
                        expProgressChangeSeq.Append(levelProgressFgImage.DOFillAmount(isMaxLevel ? 1f : levelAndProgress.Item2, 0.2f));
                    }

                    expProgressChangeSeq.Insert(0f, levelProgressText.DOFade(0, 0.1f));
                    if (levelAndProgress.Item1 < maxLevel)
                    {
                        levelProgressText.text = "{0}<color=#FBF17B>/{1}</color>".SafeFormat(PlayerCard.GetLevelNowExp(Properties.playerCard.Level, Properties.playerCard.Exp), PlayerCard.GetLevelMaxExp(Properties.playerCard.Level));
                        expProgressChangeSeq.Insert(0.1f, levelProgressText.DOFade(1, 0.1f));
                    }

                    if (isLevelUp)
                    {
                        //TODO 数值变化表现
                        //Properties.playerCard.GetCombatEffectiveness(0, true, true);

                        Player.CalFightPoint_Single(Properties.playerCard.CardId, true);
                    }
                }
                RefreshLevelUpButton();
            });

        }

        #endregion

        #region 升阶

        [SerializeField] private List<cardEquipItem> equipList = new();
        [SerializeField] private BabuButton btnTupo;
        [SerializeField] private Button btnPreviewSkill;
        [SerializeField] private BabuButton btnPreviewSkill1;
        [SerializeField] private CardUpAnim tupoAni;
        [SerializeField] private TMP_Text txtTuPo;
        [SerializeField] private TMP_Text txtLackOfLevel_Tupo;

        /// <summary>
        /// 预览技能
        /// </summary>
        /// 
        private void PreviewSkill()
        {
            PreviewSkill(null);
        }

        private void PreviewSkill(BabuButton sender)
        {
            UIController.Instance.OpenWindow<GiftSkillPreviewUI>(new GiftSkillPreviewProperties(Properties.playerCard.CardId));
        }

        private void RefreshWindow(object[] args = null)
        {
            if (args != null)
            {
                if ((int)args[0] == 1 && bottomToggleGroup.EnableIndex == 1)
                {
                    OnShowBreak();
                }
                if ((int)args[0] == 2 && bottomToggleGroup.EnableIndex == 2)
                {
                    RefreshCardStatePanel();
                    RefreshCardTrainInfoItem();
                    RefreshRedDot();
                    OnShowStar();
                }

                if ((int)args[0] == 99)
                {
                    RefreshCardStatePanel();
                }
            }
        }

        private void OnShowBreak()
        {
            cardTrainInfoItem.gameObject.SetActive(true);
            breakPanel.gameObject.SetActive(true);
            Player.CardManager.CheckEquipStatusAll(Properties.playerCard);

            List<JerseyUpgradeConfig> list = Properties.playerCard.GetEquipLevelsConfig(Properties.playerCard.EquipLevels);
            for (int index = 0; index < 4; index++)
            {
                equipList[index].SetData(Properties.playerCard, index, list[index]);
            }

            var tupoConfig = Configs.JerseyBreak.GetConfig(Properties.playerCard.DefaultPosition * 1000 + Properties.playerCard.EquipGrade + 1);
            if (Properties.playerCard.EquipStatus.CanTuPo == EquipStatus.Ready || Properties.playerCard.EquipStatus.CanTuPo == EquipStatus.LackOfMaterial)
            {
                costItems = GameItemUtils.CreateGameItems(tupoConfig.Cost).ToList();
                setCostData();

                txtTuPo.text = "突破+" + tupoConfig.Level.ToString();
                costPanel.gameObject.SetActive(true);
                btnTupo.gameObject.SetActive(true);
                btnPreviewSkill.gameObject.SetActive(false);
                btnPreviewSkill1.gameObject.SetActive(true);
                txtLackOfLevel_Tupo.gameObject.SetActive(false);
            }
            else if (Properties.playerCard.EquipStatus.CanTuPo == EquipStatus.LackOfLevel)
            {
                //卡牌等级不足
                btnPreviewSkill.gameObject.SetActive(false);
                btnPreviewSkill1.gameObject.SetActive(false);
                txtLackOfLevel_Tupo.text = string.Format("球员达到Lv.{0}可突破", tupoConfig.CardLevel);
                txtLackOfLevel_Tupo.gameObject.SetActive(true);
                btnTupo.gameObject.SetActive(false);
            }
            else
            {
                costPanel.gameObject.SetActive(false);
                btnTupo.gameObject.SetActive(false);
                btnPreviewSkill.gameObject.SetActive(true);
                btnPreviewSkill1.gameObject.SetActive(false);
                txtLackOfLevel_Tupo.gameObject.SetActive(false);
            }
        }

        public void OnTupo(BabuButton sender)
        {
            string error = Player.PackageManager.IsGameItemsEnough(costItems);
            if (error != "")
            {
                //Tips.PopTips(error);
                return;
            }
            Player.CardManager.CardEquipGradeUp(Properties.playerCard.CardId, () =>
            {
                //Properties.playerCard.EquipGrade++;
                RefreshRedDot();
                OnShowBreak();
                tupoAni.PlayAni(Properties.playerCard);
            });
        }

        #endregion

        #region 升星
        [SerializeField] public List<CardUpStarItem> StarItemList;
        [SerializeField] private RectTransform _layouttrans;
        [SerializeField] private BabuButton btnUpStar;
        [SerializeField] private TMP_Text btnTxtUpStar;
        [SerializeField] private TMP_Text btnTxtUpStarMax;
        [SerializeField] private List<CostItem> costList;
        [SerializeField] private StarUpPad starPad;
        [SerializeField] private List<InventoryBaseItem> leftSkills;
        [SerializeField] private List<InventoryBaseItem> rightSkills;
        [SerializeField] private StarListItem starListLeft;
        [SerializeField] private StarListItem starListRight;




        private List<GameItem> costItems;

        private void setCostData()
        {
            for (var index = 0; index < 3; index++)
            {
                if (index <= costItems.Count - 1)
                {
                    costList[index].gameObject.SetActive(true);
                    costList[index].SetData(costItems[index], true);
                }
                else
                {
                    costList[index].gameObject.SetActive(false);
                }
            }
            StartCoroutine(rebuildLayOut());
        }

        private void OnShowStar()
        {
            starPanel.gameObject.SetActive(true);
            var cfgs = Properties.playerCard.GetStar_CurrentAndNext();
            starPad.SetData(Properties.playerCard, cfgs);

            bindStarSkill(cfgs.Item1, leftSkills);
            bindStarSkill(cfgs.Item2, rightSkills);
            starListLeft.SetLevel(Properties.playerCard.Star);
            starListRight.gameObject.SetActive(cfgs.Item2 != null);
            starListRight.SetLevel(cfgs.Item2 != null ? cfgs.Item2.Star : 0);

            if (Properties.playerCard.IsStarAndQualityMax() == false)
            {
                btnTxtUpStarMax.gameObject.SetActive(false);
                btnUpStar.gameObject.SetActive(true);
                costPanel.gameObject.SetActive(true);
                if (Properties.playerCard.CouldUpgradeStarInThisQuality())
                {
                    btnTxtUpStar.text = "升 星";
                    costItems = Player.CardManager.GetUpgradeStarItems(Properties.playerCard.CardId);
                }
                else
                {
                    btnTxtUpStar.text = "升 品";
                    costItems = Player.CardManager.GetUpgradeQualityItems(Properties.playerCard.CardId);
                }
                costPanel.gameObject.SetActive(true);
                setCostData();
            }
            else
            {
                costPanel.gameObject.SetActive(false);
                btnTxtUpStarMax.gameObject.SetActive(true);
                btnUpStar.gameObject.SetActive(false);
            }
        }

        private async void bindStarSkill(CardUpgradeConfig cfg, List<InventoryBaseItem> skList)
        {
            List<int> result = new List<int>();
            Dictionary<int, int> SkLv;
            if (cfg != null)
            {
                //天赋技能
                SkLv = cfg.Sklv;


            }
            else
            {
                SkLv = new Dictionary<int, int>() { { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 1 } };
            }

            var giftSkillTemplateIdList = Configs.CardModel.GetConfig(Properties.playerCard.CardId).GiftIds;
            foreach (var key in SkLv.Keys)
            {
                if (key > giftSkillTemplateIdList.Length) continue;
                result.Add(giftSkillTemplateIdList[key - 1] + (SkLv[key] - 1) * 10);
            }

            for (var index = 0; index < 4; index++)
            {
                if (index >= result.Count)
                {
                    skList[index].gameObject.SetActive(false);
                }
                else
                {
                    skList[index].gameObject.SetActive(true);

                    var skId = result[index];
                    var _skCfg = Configs.GiftSkill.GetConfig(skId);
                    var skillActived = Properties.playerCard.ActivedGiftSkillCount > index;
                    var _fireSection = PlayerCard.GetSkillFireSection(_skCfg);

                    var sp = await SpriteProxy.GetGiftSkillImg(_skCfg);
                    skList[index].SetData(_skCfg.Name, _skCfg.Desc, sp, _skCfg.Sklv, skillActived, false, true, _skCfg.Fire > 0, _fireSection);
                    skList[index].SetText("Lv." + _skCfg.Sklv.ToString());
                }
            }

        }

        System.Collections.IEnumerator rebuildLayOut()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_layouttrans);
        }

        public void OnUpstar(BabuButton sender)
        {
            string error = Player.PackageManager.IsGameItemsEnough(costItems);
            if (error != "")
            {
                //Tips.PopTips(error);
                return;
            }

            if (Properties.playerCard.CouldUpgradeStarInThisQuality())
            {
                CardUpgradeType updateType = CardUpgradeType.UpgradeStar;
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
                //这里用的是老界面，打开就升星或者升品了。
                UIController.Instance.OpenWindow<CardUpgradeUI>(new CardUpgradeUIProperties(updateType, Properties.playerCard));

            }
            else
            {
                CardUpgradeType updateType = CardUpgradeType.UpgradeQuality;
                UIController.Instance.OpenWindow<CardUpgradeUI>(new CardUpgradeUIProperties(updateType, Properties.playerCard));

            }

        }

        #endregion

        public void PlayFightPoint()
        {
            int start = int.Parse(imgFightPoint.text);
            int end = Properties.playerCard.FightPoint;
            DOTween.To(value => imgFightPoint.text = ((int)value).ToString(), start, end, .5f).SetDelay(0.2f);
        }

        #region 拖拽背景快速换球员

        private bool canDragChangePlayer = true;
        [SerializeField] private DragActionComponent dragActionComponent = null;
        private bool isDragging = false;
        private Vector2 startPos;
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canDragChangePlayer == false) return;
            isDragging = true;
            startPos = eventData.pointerCurrentRaycast.screenPosition;
        }
        private float minMovePixel = 5;
        private void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            float offset = eventData.pointerCurrentRaycast.screenPosition.x - startPos.x;
            if (Mathf.Abs(offset) > minMovePixel)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
                MoveOnce(offset < 0);
                isDragging = false;
            }
        }
        private void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
        private void MoveOnce(bool next)
        {
            int nowIndex = -1;
            for (int i = 0; i < Properties.playerCardList.Count; i++)
            {
                if (Properties.playerCardList[i].CardId == Properties.playerCard.CardId)
                {
                    nowIndex = i;
                    break;
                }
            }
            if (nowIndex == -1) return;
            nowIndex += (next ? 1 : -1);
            nowIndex += Properties.playerCardList.Count;
            nowIndex %= Properties.playerCardList.Count;
            Properties.playerCard = Properties.playerCardList[nowIndex];
            RefreshUI();
        }

        #endregion
    }
}
