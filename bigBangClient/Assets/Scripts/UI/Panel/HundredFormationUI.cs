using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using DG.Tweening;
using GameConfig.Config;
using GameConfig;

namespace BigBang.UI
{
    public class HundredFormationUIProperties : WindowProperties
    {
        public HundredProgress hundredProgress = HundredProgress.NotOpen;
        public HundredFormationUI.HFType hfType = HundredFormationUI.HFType.Limit;
        public int leftTime = -1;
        public HundredFormationUIProperties(HundredProgress hundredProgress, HundredFormationUI.HFType hfType, int leftTime)
        {
            this.hundredProgress = hundredProgress;
            this.hfType = hfType;
            this.leftTime = leftTime;
        }
    }
    public class HundredFormationUI : AWindowController<HundredFormationUIProperties>
    {
        #region 初始化与监听
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private TMP_Text formationTimeTipText = null;
        [SerializeField] private LeftTimeComponent leftTimeComponent = null;
        [SerializeField] private RectTransform battleStrengthPanel = null;
        [SerializeField] private TMP_Text strengthNumText = null;
        [SerializeField] private TMP_Text changePositionTipText = null;
        [SerializeField] private List<HundredCardItem> startCardList = null;
        [SerializeField] private HundredCardGridAdapter hundredCardGridAdapter = null;
        [SerializeField] private RectTransform selectPanel = null;
        [SerializeField] private RectTransform leftTimePanel = null;
        [SerializeField] private BabuButton confirmBtn = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClickCloseBtn;
            confirmBtn.OnClick += OnClickConfirmBtn;
            Babu.EventManager.Instance.Register(EventID.OnClickHundredCardItemDown, OnClickHundredCardItemDown);
            Babu.EventManager.Instance.Register(EventID.OnClickHundredCardItemUp, OnClickHundredCardItemUp);
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClickCloseBtn;
            confirmBtn.OnClick -= OnClickConfirmBtn;
            Babu.EventManager.Instance.Unregister(EventID.OnClickHundredCardItemDown, OnClickHundredCardItemDown);
            Babu.EventManager.Instance.Unregister(EventID.OnClickHundredCardItemUp, OnClickHundredCardItemUp);
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
        }
        #endregion

        #region 退出与保存
        private void OnClickCloseBtn(BabuButton _)
        {
            if (hfType == HFType.Lock || hfType == HFType.Limit && leftTime <= 0)
            {
                UIController.Instance.CloseWindow<HundredFormationUI>();
                return;
            }
            Formation formation = Player.FightManager.FormationController.GetFormation(FormationID.Hundred);
            List<int> formationList = FormationToList(formation);
            bool isChanged = Utility.IsListSame(formationList, StartCardIdList) == false;
            if (!isChanged)
            {
                UIController.Instance.CloseWindow<HundredFormationUI>();
                return;
            }
            UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("阵容发生变更是否确认保存", () =>
            {
                UIController.Instance.CloseWindow<HundredFormationUI>();
            }, () =>
            {

            }, false, "不保存并退出", "返回"));
        }
        private void OnClickConfirmBtn(BabuButton _)
        {
            Formation formation = Player.FightManager.FormationController.GetFormation(FormationID.Hundred);
            List<int> formationList = FormationToList(formation);
            bool isSameFormation = Utility.IsListSame(formationList, StartCardIdList);
            if (!isSameFormation)
            {
                formation.SetChangeFlag(true);
                Dictionary<int, int> startDic = ListToFormationStartDic(StartCardIdList);
                formation.StarterBoardCardDic = startDic;
                formation.SubstituteBoardCardDic.Clear();
                formation.SaveToServer();
                UIController.Instance.CloseWindow<HundredFormationUI>();
            }
            Tips.PopTips("阵容已保存");
        }
        private List<int> FormationToList(Formation formation)
        {
            List<int> cardIdList = new() { 0, 0, 0, 0, 0 };
            foreach (var item in formation.StarterBoardCardDic)
            {
                FormationBoardConfig formationBoardConfig = Configs.FormationBoard.GetConfig(item.Key);
                cardIdList[formationBoardConfig.SeparatedPosition - 1] = item.Value;
            }
            return cardIdList;
        }
        private Dictionary<int, int> ListToFormationStartDic(List<int> cardIdList)
        {
            Dictionary<int, int> indexPosDec = new();
            foreach (FormationBoardConfig formationBoardConfig in Configs.FormationBoard.GetConfigList())
            {
                indexPosDec.Add(formationBoardConfig.SeparatedPosition - 1, formationBoardConfig.Id);
            }
            Dictionary<int, int> startDic = new();
            for (int i = 0; i < cardIdList.Count; i++)
            {
                int cardId = cardIdList[i];
                int posId = indexPosDec[i];
                startDic.Add(posId, cardId);
            }
            return startDic;
        }
        #endregion

        #region 数据刷新与显示刷新
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            SetType(Properties.hfType);

            RefreshBenchData();
            SetStartData();
            SetOrderData();

            RefreshHundredCardGridAdapter();
            RefreshStartCard();
            ResetCardPosition();

            ShowMainCambat(false);
        }

        private void RefreshLeftTimeOneSec()
        {
            if (hfType != HFType.Limit) return;
            if (leftTime > 0)
            {
                leftTime--;
                if (leftTime <= 0)
                {
                    UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties("比赛即将开始，无法更改阵容", "确定", () => { UIController.Instance.CloseWindow<HundredFormationUI>(); }));
                }
            }
            leftTimeComponent.SetLeftTimeText(leftTime);
        }
        private int leftTime = 0;
        private void SetLeftTime(int leftTime)
        {
            this.leftTime = leftTime;
            leftTimeComponent.SetLeftTimeText(leftTime);
        }

        /// <summary> 布阵界面的 2 个状态 </summary>
        public enum HFType
        {
            /// <summary> 限定时间内可以进行调整 </summary>
            Limit = 0,
            /// <summary> 已锁定 </summary>
            Lock = 1,
            /// <summary> 一直可以调整 </summary>
            Open = 2,
        }

        public HFType hfType = HFType.Lock;
        private void SetType(HFType signType)
        {
            this.hfType = signType;
            switch (signType)
            {
                case HFType.Limit:
                    {
                        leftTimePanel.gameObject.SetActive(true);
                        formationTimeTipText.gameObject.SetActive(true);
                        formationTimeTipText.text = "调整阵容剩余时间";
                        changePositionTipText.text = "拖动上阵球员可调整出场顺序";
                        leftTimeComponent.gameObject.SetActive(true);
                        confirmBtn.gameObject.SetActive(true);
                        selectPanel.SetTop(145.2482f);
                        selectPanel.SetBottom(136.9084f);
                        StartSelectPosition = 1;
                        SetLeftTime(Properties.leftTime);
                    }
                    break;
                case HFType.Open:
                    {
                        leftTimePanel.gameObject.SetActive(true);
                        formationTimeTipText.gameObject.SetActive(true);
                        formationTimeTipText.text = Properties.hundredProgress == HundredProgress.Sign ? "报名期间可调整阵容" : "目前可以调整阵容";
                        changePositionTipText.text = "拖动上阵球员可调整出场顺序";
                        leftTimeComponent.gameObject.SetActive(false);
                        confirmBtn.gameObject.SetActive(true);
                        selectPanel.SetTop(145.2482f);
                        selectPanel.SetBottom(136.9084f);
                        StartSelectPosition = 1;
                        SetLeftTime(-1);
                    }
                    break;
                case HFType.Lock:
                    {
                        leftTimePanel.gameObject.SetActive(false);
                        confirmBtn.gameObject.SetActive(false);
                        changePositionTipText.text = "阵容已锁定";
                        selectPanel.SetTop(83.15097f);
                        selectPanel.SetBottom(21.13098f);
                    }
                    break;
            }

        }

        List<HundredCardData> hundredCardDataList = new();
        Dictionary<int, HundredCardData> hundredCardDataDic = new();
        private void RefreshBenchData()//刷新候补卡牌数据
        {
            hundredCardDataList = new();
            hundredCardDataDic = new();
            foreach (PlayerCard playerCard in Player.CardManager.CardList)
            {
                HundredCardData hundredCardData = new();
                hundredCardData.playerCard = playerCard;
                hundredCardData.isCanMove = this.hfType == HFType.Limit || this.hfType == HFType.Open;
                hundredCardDataList.Add(hundredCardData);
                hundredCardDataDic.Add(playerCard.CardId, hundredCardData);
            }
            hundredCardDataList = hundredCardDataList.OrderByDescending(data => data.playerCard.FightPoint)
                .ThenByDescending(data => data.playerCard.Quality)
                .ThenByDescending(data => data.playerCard.Star)
                .ThenByDescending(data => data.playerCard.CardId)
                .ToList();
        }
        private void RefreshHundredCardGridAdapter()//刷新候补卡牌显示
        {
            hundredCardGridAdapter.SetData(hundredCardDataList);
        }

        private int StartSelectPosition = 1;//上面 5 个人选了哪个，从 1 开始
        private List<int> StartCardIdList = new();
        private void SetStartData()//刷新首发数据
        {
            Formation formation = Player.FightManager.FormationController.GetFormation(FormationID.Hundred);
            if (formation.StarterBoardCardDic.Count <= 0)
            {
                Tips.PopTips("没有阵容信息！请先报名");
                Debug.LogError("没有阵容信息！");
                return;
            }
            StartCardIdList = FormationToList(formation);
        }
        private void SetOrderData()//刷新出场顺序数据
        {
            for (int i = 0; i < 5; i++)
            {
                hundredCardDataDic[StartCardIdList[i]].orderNumber = i + 1;
            }
            for (int i = 0; i < hundredCardDataList.Count; i++)
            {
                if (StartCardIdList.Contains(hundredCardDataList[i].playerCard.CardId) == false)
                {
                    hundredCardDataList[i].orderNumber = 0;
                }
            }
        }
        private void RefreshStartCard()//刷新首发卡牌界面
        {
            for (int i = 0; i < 5; i++)
            {
                startCardList[i].SetData(hundredCardDataDic[StartCardIdList[i]], true);
                startCardList[i].SetSelect(StartSelectPosition == i + 1);
            }
        }
        #endregion

        #region 下面的卡牌换到上面

        private void OnClickHundredCardItemDown(object[] args)//布阵阶段点击了下面的卡牌，判断卡牌状态后替换卡牌
        {
            HundredCardItem hundredCardItem = args[0] as HundredCardItem;

            if (Player.FightManager.FormationController.isNeedAlertHundredWrongChange)
            {
                string alertText = "该名球员";
                bool isNeedAlert = false;
                if (hundredCardItem.hundredCardData.playerCard.IsHurt())
                {
                    if (isNeedAlert == true) alertText += "，";
                    alertText += "身体受伤";
                    isNeedAlert = true;
                }
                if (hundredCardItem.hundredCardData.playerCard.Status == PlayerCardStatus.Down || hundredCardItem.hundredCardData.playerCard.Status == PlayerCardStatus.VeryDown)
                {
                    if (isNeedAlert == true) alertText += "，";
                    alertText += "状态不好";
                    isNeedAlert = true;
                }
                if (hundredCardItem.hundredCardData.playerCard.Energy < GameConst.CardSingleEnergyWarning)
                {
                    if (isNeedAlert == true) alertText += "，";
                    alertText += "体力值不足";
                    isNeedAlert = true;
                }
                if (isNeedAlert)
                {
                    alertText += "，是否前往回复？";
                }

                if (isNeedAlert)
                {
                    ConfirmBoxCheckUIProperties confirmBoxCheckUIProperties = new ConfirmBoxCheckUIProperties(alertText, () =>
                    {
                        CardUpUIProperties cardUpUIProperties = new CardUpUIProperties(hundredCardItem.hundredCardData.playerCard);
                        UIController.Instance.CloseWindow<HundredFormationUI>();
                        UIController.Instance.ShowPanel<CardUpUI>(cardUpUIProperties);
                    }, () =>
                    {
                        MoveDownCardToTop(hundredCardItem);
                    }, !Player.FightManager.FormationController.isNeedAlertHundredWrongChange, "不再提醒", (bool isCheck) =>
                    {
                        Player.FightManager.FormationController.isNeedAlertHundredWrongChange = !isCheck;
                    });
                    confirmBoxCheckUIProperties.SetConfirmColor(false, "前往恢复", "直接上阵");
                    UIController.Instance.OpenWindow<ConfirmBoxCheckUI>(confirmBoxCheckUIProperties);
                }
                else
                {
                    MoveDownCardToTop(hundredCardItem);
                }
            }
            else
            {
                MoveDownCardToTop(hundredCardItem);
            }
        }
        private void MoveDownCardToTop(HundredCardItem downCardItem)//将下面的卡牌换到上面
        {
            StartCardIdList[StartSelectPosition - 1] = downCardItem.hundredCardData.playerCard.CardId;
            SetOrderData();
            RefreshStartCard();
            RefreshHundredCardGridAdapter();
            ShowMainCambat(true);
        }
        private void OnClickHundredCardItemUp(object[] args)//布阵阶段点击了上面的卡牌
        {
            // 点击上部卡牌音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);

            HundredCardItem hundredCardItem = args[0] as HundredCardItem;
            StartSelectPosition = hundredCardItem.hundredCardData.orderNumber;
            RefreshStartCard();
        }
        #endregion

        #region 顶部卡牌拖拽

        private void Start()
        {
            for (int i = 0; i < 5; i++)
            {
                HundredCardItem hundredCardItem = startCardList[i];
                hundredCardItem.DragBeginCardItem = DragBeginCardItem;
                hundredCardItem.DragMoveCardItem = DragMoveCardItem;
                hundredCardItem.DragEndCardItem = DragEndCardItem;
            }
        }
        [SerializeField] private List<float> upCardXList = new();
        private void DragBeginCardItem(PointerEventData data, HundredCardItem hundredCardItem)
        {
            hundredCardItem.transform.SetAsLastSibling();
        }
        private void DragMoveCardItem(PointerEventData data, HundredCardItem hundredCardItem)
        {
            float localX = Utility.ConvertScreenPositionToLocalPosition(hundredCardItem.transform.parent as RectTransform, data.pointerCurrentRaycast.screenPosition, UIController.Instance.GetCamera()).x;
            localX = Utility.KeepInRange(localX, upCardXList[0], upCardXList[^1]);
            hundredCardItem.transform.SetLocalPositionX(localX);
            int nowPosIndex = GetNowPosXIndex(localX);


            if (nowPosIndex != StartSelectPosition)
            {
                Debug.Log("nowPosIndex = " + nowPosIndex);
                Debug.Log("StartSelectPosition = " + StartSelectPosition);

                // 移动上部卡牌音效
                AudioManager.Instance.PlaySound(AudioNames.ANI_HOVER);

                int startCardId = StartCardIdList[StartSelectPosition - 1];
                StartCardIdList.RemoveAt(StartSelectPosition - 1);
                StartCardIdList.Insert(nowPosIndex - 1, startCardId);

                HundredCardItem startCardItem = startCardList[StartSelectPosition - 1];
                startCardList.RemoveAt(StartSelectPosition - 1);
                startCardList.Insert(nowPosIndex - 1, startCardItem);

                StartSelectPosition = nowPosIndex;
                SetOrderData();
                RefreshHundredCardGridAdapter();

                for (int i = 0; i < 5; i++)
                {
                    HundredCardItem moveCardItem = startCardList[i];
                    moveCardItem.transform.DOKill();
                    if (i == nowPosIndex - 1) continue;
                    float moveTargetX = upCardXList[i];
                    float moveLengh = MathF.Abs(moveTargetX - moveCardItem.transform.localPosition.x);
                    float moveTime = moveLengh / 630f * 0.2f;
                    moveCardItem.transform.DOLocalMoveX(moveTargetX, moveTime).SetEase(Ease.Linear).AddTo(this.gameObject);
                }
            }
        }
        private void DragEndCardItem(PointerEventData data, HundredCardItem hundredCardItem)
        {
            for (int i = 0; i < 5; i++)
            {
                HundredCardItem moveCardItem = startCardList[i];
                moveCardItem.transform.DOKill();
                float moveTargetX = upCardXList[i];
                float moveLengh = MathF.Abs(moveTargetX - moveCardItem.transform.localPosition.x);
                float moveTime = moveLengh / 630f * 0.2f;
                moveCardItem.transform.DOLocalMoveX(moveTargetX, moveTime).SetEase(Ease.Linear).AddTo(this.gameObject);
            }
        }
        private int GetNowPosXIndex(float localX)
        {
            for (int i = 0; i < 4; i++)
            {

                if (localX <= (upCardXList[i] + upCardXList[i + 1]) / 2) return i + 1;
            }
            return 5;
        }
        private void ResetCardPosition()
        {
            for (int i = 0; i < 5; i++)
            {
                HundredCardItem moveCardItem = startCardList[i];
                moveCardItem.transform.DOKill();
                float moveTargetX = upCardXList[i];
                moveCardItem.transform.SetLocalPositionX(moveTargetX);
            }
        }
        #endregion

        #region 战斗力滚动动画

        private int GetStartCombat()
        {
            int totalCombat = 0;
            for (int i = 0; i < 5; i++)
            {
                HundredCardItem moveCardItem = startCardList[i];
                totalCombat += moveCardItem.hundredCardData.playerCard.FightPoint;
            }
            return totalCombat;
        }

        private Tween mainCambatTween;
        private void clearCombatTween()
        {
            mainCambatTween?.Kill();
        }

        private void ShowMainCambat(bool needAni = true)
        {
            if (needAni == false)
            {
                clearCombatTween();
                strengthNumText.text = GetStartCombat().ToString("###,###");
            }
            else
            {
                clearCombatTween();
                int num = 0;
                int.TryParse(strengthNumText.text.Replace(",", ""), out num);
                strengthNumText.DOChangeNumberEx(GetStartCombat(), 1.0f, 1.2f, num, "###,###", battleStrengthPanel.transform).AddTo(this.gameObject);
            }
        }

        #endregion

    }
}