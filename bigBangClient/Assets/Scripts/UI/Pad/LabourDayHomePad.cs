using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.Animation;
using BigBang.UI;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.AllStarManager;
using static BigBang.SpriteNames;
using GameItem = Utils.GameItem.GameItem;
using Vector2 = UnityEngine.Vector2;

public class LabourDayHomePad : MonoBehaviour, IActivity
{
    #region 初始化
    [SerializeField] private BabuButton helpButton = null;

    private void OnEnable()
    {
        helpButton.OnClick += OnClickHelpButton;
        dicePanel.OnClick += OnClickDicePanel;
        onceButton.OnClick += OnClickOnceButton;
        tenButton.OnClick += OnClickTenButton;
        EventManager.Instance.Register(EventID.OnRefreshGoods, RefreshDiceCount);
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
    }

    private void OnDisable()
    {
        helpButton.OnClick -= OnClickHelpButton;
        dicePanel.OnClick -= OnClickDicePanel;
        onceButton.OnClick -= OnClickOnceButton;
        tenButton.OnClick -= OnClickTenButton;
        EventManager.Instance.Unregister(EventID.OnRefreshGoods, RefreshDiceCount);
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
    }
    private ActivityData activityData = null;
    private bool isChessInited = false;
    public void LoadActivity(ActivityData _data)
    {
        isRollBtnJustclicked = false;
        activityData = _data;
        // HideAll();
        // AllStarManager.Instance.GetServerData(() =>
        // {
        //     RefreshUI();
        //     if (AllStarManager.Instance.IsNeedShowEnd)
        //     {
        //         UIController.Instance.OpenWindow<AllStarEndUI>();
        //     }
        // });
        ClearRollDiceAnim();
        ClearGoToAnim();
        if (!isChessInited)
        {
            isChessInited = true;
            GenAllTile();
            EnsurePosition();
            GenAllPath();
            StartChessUpDown();
        }
        ResetChessData();
        SetTileActivityData();
        RefreshTileInfo();
        SetBg(LabourDayManager.Instance.mapIndex);
        RefreshChessPosition();
        RefreshDiceCount();
        RefreshLeftTime();
    }
    #endregion

    #region 按钮回调

    private void OnClickHelpButton(BabuButton button)
    {
        UIController.Instance.OpenWindow<LabourDayHomeHelpUI>();
    }

    #endregion

    #region 格子

    [SerializeField] private GameObject tileItemPrefab = null;
    [SerializeField] private RectTransform tileItemRoot = null;
    private List<LabourDayHomeTileItem> tileItemList = new List<LabourDayHomeTileItem>();
    [SerializeField] private List<Vector2Int> tilePosList = new List<Vector2Int>();
    private void GenAllTile()
    {
        for (int i = 0; i < tilePosList.Count; i++)
        {
            GameObject go = Instantiate(tileItemPrefab, tileItemRoot);
            go.gameObject.SetActive(true);
            LabourDayHomeTileItem tileItem = go.GetComponent<LabourDayHomeTileItem>();
            tileItem.SetData(i, tilePosList[i]);
            tileItemList.Add(tileItem);
        }
    }
    private void SetTileActivityData()
    {
        foreach (LabourDayHomeTileItem tileItem in tileItemList)
        {
            tileItem.SetActivityData(activityData);
        }
    }
    private void RefreshTileInfo()
    {
        foreach (LabourDayHomeTileItem tileItem in tileItemList)
        {
            tileItem.RefreshInfo(LabourDayManager.Instance.mapInnerIndex);
        }
    }

    [SerializeField] private Vector3 zeroTileLocalPosition;
    [SerializeField] private Vector3 tileSpace;

    // #if UNITY_EDITOR
    //     /// <summary>
    //     /// Update is called every frame, if the MonoBehaviour is enabled.
    //     /// </summary>
    //     private void Update()
    //     {
    //         EnsurePosition();
    //     }
    // #endif
    private void EnsurePosition()
    {
        foreach (LabourDayHomeTileItem tileItem in tileItemList)
        {
            Vector3 pos = zeroTileLocalPosition + new Vector3(tileItem.pos.x * tileSpace.x + tileItem.pos.y * -tileSpace.x, tileItem.pos.x * tileSpace.y + tileItem.pos.y * tileSpace.y, 0);
            tileItem.transform.localPosition = pos;
        }
    }

    #endregion

    #region 路径

    [SerializeField] private GameObject pathPrefabX = null;
    [SerializeField] private GameObject pathPrefabY = null;
    [SerializeField] private RectTransform pathRoot = null;
    private List<Image> pathImageList = new List<Image>();
    [SerializeField] private Vector2 pathOffset = new(0, -11.54f);
    private void GenAllPath()
    {
        for (int i = 0; i < tileItemList.Count - 1; i++)
        {
            LabourDayHomeTileItem nowTileItem = tileItemList[i];
            LabourDayHomeTileItem nextTileItem = tileItemList[i + 1];
            bool isX = nowTileItem.pos.x != nextTileItem.pos.x;
            GameObject go = Instantiate(isX ? pathPrefabX : pathPrefabY, pathRoot);
            go.gameObject.SetActive(true);
            Image pathImage = go.GetComponent<Image>();
            pathImage.transform.localPosition = new Vector3((nowTileItem.transform.localPosition.x + nextTileItem.transform.localPosition.x) / 2 + pathOffset.x, (nowTileItem.transform.localPosition.y + nextTileItem.transform.localPosition.y) / 2 + pathOffset.y, 0);
            pathImageList.Add(pathImage);
        }
    }

    #endregion

    #region 热气球

    private void ResetChessData()
    {
        chessItem.SetData(LabourDayManager.Instance.mapIndex, LabourDayManager.Instance.mapInnerIndex);
    }

    [SerializeField] private LabourDayHomeChessItem chessItem = null;
    private void RefreshChessPosition()
    {
        LabourDayHomeTileItem nowtileItem = tileItemList[chessItem.mapInnerIndex];
        chessItem.chessItem.localPosition = nowtileItem.transform.localPosition;
    }

    private Sequence chessSequence = null;
    private void StartChessUpDown()
    {
        chessSequence?.Kill();
        chessSequence = DOTween.Sequence();
        chessItem.upDownMoveTrans.anchoredPosition = new Vector2(0, -10);
        chessSequence.Append(chessItem.upDownMoveTrans.DOAnchorPosY(10, 2.3f).SetEase(Ease.InOutQuad));
        chessSequence.Append(chessItem.upDownMoveTrans.DOAnchorPosY(-10, 2.3f).SetEase(Ease.InOutQuad));
        chessSequence.SetLoops(-1);
        chessSequence.AddTo(this.gameObject);
    }

    #endregion

    #region 背景

    [SerializeField] private List<Image> cityImageList = new();
    private void SetBg(int mapIndex)
    {
        for (int i = 0; i < cityImageList.Count; i++)
        {
            cityImageList[i].gameObject.SetActive(i == mapIndex);
        }
    }

    #endregion

    #region 投掷按钮

    [SerializeField] private TMP_Text diceNumText = null;
    [SerializeField] private UIShiny onceBtnUIShiny = null;
    [SerializeField] private UIShiny threeBtnUIShiny = null;
    private void RefreshDiceCount(object[] _ = null)
    {
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, int.Parse(activityData.cfg.Param2), 0);
        int diceCount = gameItem.GetPlayerCount();
        diceNumText.text = diceCount.ToString();
        onceBtnUIShiny.enabled = diceCount >= 1 && !LabourDayManager.Instance.IsGetAllReward;
        threeBtnUIShiny.enabled = diceCount >= 3 && !LabourDayManager.Instance.IsGetAllReward;
    }

    [SerializeField] private BabuButton dicePanel = null;
    private void OnClickDicePanel(BabuButton _)
    {
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, int.Parse(activityData.cfg.Param2), 0);
        gameItem.ShowTip();
    }

    [SerializeField] private BabuButton onceButton = null;
    [SerializeField] private BabuButton tenButton = null;
    private void OnClickOnceButton(BabuButton _)
    {
        CheckAndRoll(1);
    }
    private void OnClickTenButton(BabuButton _)
    {
        CheckAndRoll(3);
    }
    private bool isRollBtnJustclicked = false;
    private void CheckAndRoll(int rollCount)
    {
        if (isRollBtnJustclicked) return;
        isRollBtnJustclicked = true;
        UnityTimer.Timer.Register(this.gameObject, 1.5f, () => { isRollBtnJustclicked = false; });
        if (LabourDayManager.Instance.IsGetAllReward)
        {
            Tips.PopTips("已经通关{0}".SafeFormat(activityData.cfg.Name));
            return;
        }
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, int.Parse(activityData.cfg.Param2), 0);
        if (gameItem.GetPlayerCount() < rollCount)
        {
            Tips.PopTips("{0}不足".SafeFormat(gameItem.GetName()));
            return;
        }
        LabourDayManager.Instance.RollDice(rollCount, GoToNowPosition);
    }
    private void GoToNowPosition()
    {
        TouchManager.Instance.DisableTouch();
        PlayRollDiceAnim(() =>
        {
            PlayGoToAnim((List<GameItem> gameItemList) =>
            {
                int honourId = 0;
                foreach (var item in gameItemList)
                {
                    if (item.Type == GameItemType.Honour)
                    {
                        honourId = item.Id;
                        break;
                    }
                }
                if (honourId == 0) return;
                AchievementData achievementData = BigBang.Player.AchievementManager.GetAchievementData(honourId);
                UIController.Instance.OpenWindow<HonourGetUI>(new HonourGetUIProperties(achievementData, true));
            });
        });
    }
    [SerializeField] private RectTransform rollDicePanel = null;
    [SerializeField] private Image rollDiceBgImage = null;
    [SerializeField] private List<SkeletonGraphic> diceRollingDiceSkeletonGraphicList1 = new();
    [SerializeField] private List<Image> diceEndImageList1 = new();
    [SerializeField] private List<SkeletonGraphic> diceRollingDiceSkeletonGraphicList3 = new();
    [SerializeField] private List<Image> diceEndImageList3 = new();
    [SerializeField] private List<Sprite> diceSpriteList = new();
    private Sequence rollSequence = null;
    private void PlayRollDiceAnim(Action callback)
    {
        if (LabourDayManager.Instance.diceNumList.Count != 1 && LabourDayManager.Instance.diceNumList.Count != 3)
        {
            Debug.LogWarning("LabourDayHomePad , PlayRollDiceAnim , LabourDayManager.Instance.diceNumList.Count != 1 && LabourDayManager.Instance.diceNumList.Count != 3");
            TouchManager.Instance.EnableTouch();
            return;
        }

        bool isOne = LabourDayManager.Instance.diceNumList.Count == 1;
        foreach (SkeletonGraphic diceRollingDiceSkeletonGraphic1 in diceRollingDiceSkeletonGraphicList1)
        {
            diceRollingDiceSkeletonGraphic1.gameObject.SetActive(isOne);
        }
        foreach (Image diceEndImage1 in diceEndImageList1)
        {
            diceEndImage1.gameObject.SetActive(false);
        }
        foreach (SkeletonGraphic diceRollingDiceSkeletonGraphic3 in diceRollingDiceSkeletonGraphicList3)
        {
            diceRollingDiceSkeletonGraphic3.gameObject.SetActive(!isOne);
        }
        foreach (Image diceEndImage3 in diceEndImageList3)
        {
            diceEndImage3.gameObject.SetActive(false);
        }
        rollDiceBgImage.SetAlpha(0);
        List<SkeletonGraphic> diceRollingDiceSkeletonGraphicList = isOne ? diceRollingDiceSkeletonGraphicList1 : diceRollingDiceSkeletonGraphicList3;
        List<Image> diceEndImageList = isOne ? diceEndImageList1 : diceEndImageList3;
        rollSequence?.Kill();
        rollSequence = DOTween.Sequence();
        rollSequence.AddTo(this.gameObject);
        rollDicePanel.gameObject.SetActive(true);
        rollSequence.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.EVENT_UPSTAR); });
        rollSequence.Append(rollDiceBgImage.DOFade(0.4f, 0.3f));
        rollSequence.AppendInterval(1.8f);
        rollSequence.AppendCallback(() =>
        {
            foreach (SkeletonGraphic diceRollingDiceSkeletonGraphic in diceRollingDiceSkeletonGraphicList)
            {
                diceRollingDiceSkeletonGraphic.gameObject.SetActive(false);
            }
            for (int i = 0; i < diceEndImageList.Count; i++)
            {
                Image diceEndImage = diceEndImageList[i];
                diceEndImage.transform.localScale = Vector3.one;
                diceEndImage.SetAlpha(1);
                int diceNum = LabourDayManager.Instance.diceNumList[i];
                diceEndImage.sprite = diceSpriteList[diceNum - 1];
                diceEndImage.gameObject.SetActive(true);
            }
        });
        rollSequence.AppendInterval(0.4f);
        rollSequence.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.RSLT_INFO); });
        rollSequence.AppendInterval(0.6f);
        for (int i = 0; i < diceEndImageList.Count; i++)
        {
            if (i == 0)
            {
                rollSequence.Append(diceEndImageList[i].transform.DOScale(2.0f, 0.3f));
            }
            else
            {
                rollSequence.Join(diceEndImageList[i].transform.DOScale(2.0f, 0.3f));
            }
            rollSequence.Join(diceEndImageList[i].DOFade(0, 0.3f));
        }
        rollSequence.Join(rollDiceBgImage.DOFade(0f, 0.3f));
        rollSequence.AppendCallback(() =>
        {
            rollDicePanel.gameObject.SetActive(false);
            callback?.Invoke();
        });
    }
    private void ClearRollDiceAnim()
    {
        rollSequence?.Kill();
        rollDicePanel.gameObject.SetActive(false);
    }

    private Sequence gotoSequence = null;
    [SerializeField] private RectTransform cloudPanel = null;
    [SerializeField] private Image topCloudImage = null;
    [SerializeField] private Image bottomCloudImage = null;
    [SerializeField] private RectTransform contentPanel = null;
    private float cloudMoveTime = 1.0f;
    private void PlayGoToAnim(Action<List<GameItem>> callback)
    {
        List<GameItem> gameItemList = new();
        gotoSequence?.Kill();
        gotoSequence = DOTween.Sequence();
        gotoSequence.AddTo(this.gameObject);
        while (chessItem.mapIndex < LabourDayManager.Instance.mapIndex || chessItem.mapInnerIndex < LabourDayManager.Instance.mapInnerIndex)
        {
            if (chessItem.mapInnerIndex < 30)
            {
                chessItem.mapInnerIndex++;
                LabourDayHomeTileItem nowtileItem = tileItemList[chessItem.mapInnerIndex];
                int festivalTrivalId = activityData.cfg.Id * 1000 + chessItem.mapIndex * 30 + chessItem.mapInnerIndex;
                FestivalTravelConfig festivalTravelConfig = Configs.FestivalTravel.GetConfig(festivalTrivalId);
                if (festivalTravelConfig == null)
                {
                    Debug.LogWarning("LabourDayHomePad , PlayGoToAnim ,  festivalTravelConfig == null , festivalTrivalId = " + festivalTrivalId);
                    return;
                }
                gameItemList.Add(GameItemUtils.CreateGameItem(festivalTravelConfig.Reward));
                int chessMapIndex = chessItem.mapIndex;
                gotoSequence.AppendCallback(() =>
                {
                    AudioManager.Instance.PlaySound(AudioNames.OPENREDENVELOPE);
                    chessItem.PlayCollectAnim(nowtileItem, 0.5f);
                    nowtileItem.RefreshInfo(chessMapIndex < LabourDayManager.Instance.mapIndex ? 30 : LabourDayManager.Instance.mapInnerIndex);
                });
                gotoSequence.Append(chessItem.chessItem.DOLocalMove(nowtileItem.transform.localPosition, 0.5f));
            }
            else
            {
                chessItem.mapIndex++;
                chessItem.mapInnerIndex = 0;
                gotoSequence.AppendCallback(() =>
                {
                    AudioManager.Instance.PlaySound(AudioNames.PLANE);
                    cloudPanel.gameObject.SetActive(true);
                    topCloudImage.rectTransform.anchoredPosition = new Vector2(0, 841);
                    bottomCloudImage.rectTransform.anchoredPosition = new Vector2(0, -841);
                    topCloudImage.SetAlpha(0);
                    bottomCloudImage.SetAlpha(0);
                });
                gotoSequence.Append(topCloudImage.DOFade(1, cloudMoveTime));
                gotoSequence.Join(contentPanel.DOScale(0.8f, cloudMoveTime * 0.8f));
                gotoSequence.Join(topCloudImage.rectTransform.DOAnchorPosY(-260, cloudMoveTime));
                gotoSequence.Join(bottomCloudImage.DOFade(1, cloudMoveTime));
                gotoSequence.Join(bottomCloudImage.rectTransform.DOAnchorPosY(260, cloudMoveTime));
                gotoSequence.AppendCallback(() =>
                {
                    foreach (LabourDayHomeTileItem tileItem in tileItemList)
                    {
                        tileItem.RefreshInfo(0);
                    }
                    SetBg(chessItem.mapIndex);
                    LabourDayHomeTileItem nowtileItem = tileItemList[0];
                    chessItem.chessItem.localPosition = nowtileItem.transform.localPosition;
                });
                gotoSequence.AppendInterval(0.5f);
                gotoSequence.Append(topCloudImage.DOFade(0, cloudMoveTime));
                gotoSequence.Join(topCloudImage.rectTransform.DOAnchorPosY(841, cloudMoveTime));
                gotoSequence.Join(bottomCloudImage.DOFade(0, cloudMoveTime));
                gotoSequence.Join(bottomCloudImage.rectTransform.DOAnchorPosY(-841, cloudMoveTime));
                gotoSequence.Join(contentPanel.DOScale(1.0f, cloudMoveTime * 0.8f).SetDelay(cloudMoveTime * 0.2f));
                gotoSequence.AppendCallback(() =>
                {
                    cloudPanel.gameObject.SetActive(false);
                });
                gotoSequence.AppendInterval(0.1f);
            }
        }
        gotoSequence.AppendInterval(0.5f);
        gotoSequence.AppendCallback(() =>
        {
            TouchManager.Instance.EnableTouch();
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(gameItemList, () =>
            {
                callback?.Invoke(gameItemList);
                chessItem.ClearAllFakeGoods();
            }));
        });
    }
    private void ClearGoToAnim()
    {
        gotoSequence?.Kill();
        cloudPanel.gameObject.SetActive(false);
        contentPanel.localScale = Vector3.one;
    }



    #endregion

    #region 倒计时

    [SerializeField] private TMP_Text timeText = null;
    private void RefreshLeftTime()
    {
        if (activityData == null) return;
        long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
        timeText.text = "剩余时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
    }

    #endregion

}
