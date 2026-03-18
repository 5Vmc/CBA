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
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.DragonBoatFestivalManager;
using GameItem = Utils.GameItem.GameItem;
using Vector2 = UnityEngine.Vector2;

public class DragonBoatFestivalHomePad : MonoBehaviour, IActivity
{
    #region 初始化

    [SerializeField] private RectTransform buttonPanel = null;
    [SerializeField] private BabuButton helpButton = null;
    [SerializeField] private BabuButton taskButton = null;
    [SerializeField] private Image taskDotNodeImg = null;
    [SerializeField] private UIShiny taskIconImage = null;
    [SerializeField] private BabuButton progressButton = null;
    [SerializeField] private Image progressDotNodeImg = null;
    [SerializeField] private UIShiny progressIconImage = null;
    [SerializeField] private BabuButton rankButton = null;
    [SerializeField] private TMP_Text leftTimeText = null;
    [SerializeField] private BabuButton drumButton = null;
    [SerializeField] private Image drumIconImage = null;
    [SerializeField] private TMP_Text drumCountText = null;

    [SerializeField] private Image tasteImage = null;
    [SerializeField] private RectTransform dragonPanel = null;
    [SerializeField] private RectTransform leftNamePanel = null;
    [SerializeField] private TMP_Text leftServerText = null;
    [SerializeField] private TMP_Text leftNameText = null;
    [SerializeField] private ClubIconItem leftClubIconImage = null;
    [SerializeField] private RectTransform rightNamePanel = null;
    [SerializeField] private TMP_Text rightServerText = null;
    [SerializeField] private TMP_Text rightNameText = null;
    [SerializeField] private ClubIconItem rightClubIconImage = null;

    [SerializeField] private RectTransform joinPanel = null;
    [SerializeField] private BabuButton joinButtonLeft = null;
    [SerializeField] private BabuButton joinButtonRight = null;
    [SerializeField] private RectTransform upPanel = null;
    [SerializeField] private BabuButton drumImageButton = null;
    [SerializeField] private TMP_Text leftLengthText = null;
    [SerializeField] private TMP_Text rightLengthText = null;
    [SerializeField] private Image leftSelectImage = null;
    [SerializeField] private Image rightSelectImage = null;
    [SerializeField] private RectTransform winPanel = null;
    [SerializeField] private TMP_Text leftLengthTextWin = null;
    [SerializeField] private TMP_Text rightLengthTextWin = null;
    [SerializeField] private RectTransform leftWinPanel = null;
    [SerializeField] private RectTransform rightWinPanel = null;

    [SerializeField] private RectTransform loadingPanel = null;
    [SerializeField] private TMP_Text notOpenText = null;
    [SerializeField] private TMP_Text closedText = null;

    private void OnEnable()
    {
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        drumButton.OnClick += OnClickDrumButton;
        joinButtonLeft.OnClick += OnClickJoinButtonLeft;
        joinButtonRight.OnClick += OnClickJoinButtonRight;
        helpButton.OnClick += OnClickHelpButton;
        taskButton.OnClick += OnClickTaskButton;
        progressButton.OnClick += OnClickProgressButton;
        rankButton.OnClick += OnClickRankButton;
        drumImageButton.OnClick += OnClickDrumImageButton;
        EventManager.Instance.Register(EventID.RefreshDragonBoatFestivalUI, RefreshDragonBoatFestivalUI);
        EventManager.Instance.Register(EventID.OnRefreshGoods, OnRefreshGoods);
        EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshUIRedDot);
        EventManager.Instance.Register(EventID.OnUpDragonBoatFestivalTeam, OnUpDragonBoatFestivalTeam);
    }

    private void OnDisable()
    {
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
        drumButton.OnClick -= OnClickDrumButton;
        joinButtonLeft.OnClick -= OnClickJoinButtonLeft;
        joinButtonRight.OnClick -= OnClickJoinButtonRight;
        helpButton.OnClick -= OnClickHelpButton;
        taskButton.OnClick -= OnClickTaskButton;
        progressButton.OnClick -= OnClickProgressButton;
        rankButton.OnClick -= OnClickRankButton;
        drumImageButton.OnClick -= OnClickDrumImageButton;
        EventManager.Instance.Unregister(EventID.RefreshDragonBoatFestivalUI, RefreshDragonBoatFestivalUI);
        EventManager.Instance.Unregister(EventID.OnRefreshGoods, OnRefreshGoods);
        EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshUIRedDot);
        EventManager.Instance.Unregister(EventID.OnUpDragonBoatFestivalTeam, OnUpDragonBoatFestivalTeam);
    }
    [SerializeField] private LoadingDataAnim loadingDataAnim = null;
    private ActivityData activityData = null;
    public void LoadActivity(ActivityData _data)
    {
        activityData = _data;
        ClearBoatWinAnim();
        ClearBoatWaitAnim();
        ClearBoatPlayingAnim();
        ClearEnterAlphaAnim();
        ClearFastBoatAnim();
        ClearBgAnim();
        ClearFlyAnim();
        HideAll();
        leftBoatPanel.SetAnchoredPositionY(-800);
        rightBoatPanel.SetAnchoredPositionY(-800);
        leftOldMeter = 0;
        rightOldMeter = 0;
        loadingDataAnim.Init();
        //playoffFinalsGuessHomePadAnim.Init();
        loadingDataAnim.PlayEnter();
        PlayBgAnim();
        PlayEnterAlphaAnim();
        DragonBoatFestivalManager.Instance.GetCourseData(() =>
        {
            RefreshUI();
            loadingDataAnim.PlayExit();
            //playoffFinalsGuessHomePadAnim.PlayEnter(() =>
            //{

            //});
            if (DragonBoatFestivalManager.Instance.IsNeedShowEnd)
            {
                UIController.Instance.OpenWindow<DragonBoatFestivalEndUI>();
            }
        });
    }
    private void RefreshDragonBoatFestivalUI(object[] _)
    {
        HideAll();
        RefreshUI();
    }
    [SerializeField] private Image drumDotNodeImg = null;
    private void RefreshUIRedDot(object[] _ = null)
    {
        taskDotNodeImg.gameObject.SetActive(DragonBoatFestivalManager.Instance.CanCollectTask);
        taskIconImage.enabled = DragonBoatFestivalManager.Instance.CanCollectTask;
        progressDotNodeImg.gameObject.SetActive(DragonBoatFestivalManager.Instance.CanCollectProgress);
        progressIconImage.enabled = DragonBoatFestivalManager.Instance.CanCollectProgress;
        drumDotNodeImg.gameObject.SetActive(DragonBoatFestivalManager.Instance.CanUseDrum);
    }
    #endregion

    #region 按钮回调

    private void OnClickHelpButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<DragonBoatFestivalHomeHelpUI>();
    }
    private void OnClickJoinButtonLeft(BabuButton _)
    {
        OnClickJoinButton(Team.Left);
    }
    private void OnClickJoinButtonRight(BabuButton _)
    {
        OnClickJoinButton(Team.Right);
    }
    private void OnClickJoinButton(Team team)
    {
        string teamStr = team == Team.Left ? "<color=#ACFF35><size=36>甜</size>粽龙舟队</color>" : "<color=#FFED42><size=36>咸</size>粽龙舟队</color>";
        UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("确定加入 {0} 吗？加入后不可更改".SafeFormat(teamStr), () =>
        {
            DragonBoatFestivalManager.Instance.PickDragonBoat(team, () =>
            {
                Tips.PopTips("您加入了" + teamStr);
            });
        }, null));
    }
    private void OnClickTaskButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<DragonBoatFestivalHomeTaskUI>();
    }
    private void OnClickProgressButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<DragonBoatFestivalProgressUI>();
    }
    private void OnClickRankButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<DragonBoatRankUI>();
    }
    private void OnClickDrumImageButton(BabuButton _)
    {
        Stage stage = DragonBoatFestivalManager.Instance.GetStage();
        if (stage != Stage.NormalPlaying)
        {
            Tips.PopTips("已过可用时间");
            return;
        }
        ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
        if (activityData == null) return;
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, activityData.cfg.Param1, 0);
        if (gameItem.GetPlayerCount() <= 0)
        {
            Tips.PopTips("{0}不足".SafeFormat(gameItem.GetName()));
            return;
        }
        UIController.Instance.OpenWindow<DragonBoatFestivalUpCountUI>();
    }

    #endregion

    #region 界面刷新

    private void HideAll()
    {
        buttonPanel.gameObject.SetActive(false);
        dragonPanel.gameObject.SetActive(false);
        joinPanel.gameObject.SetActive(false);
        upPanel.gameObject.SetActive(false);
        winPanel.gameObject.SetActive(false);
        loadingPanel.gameObject.SetActive(false);
        closedText.gameObject.SetActive(false);
        notOpenText.gameObject.SetActive(false);
        tasteImage.gameObject.SetActive(false);
    }

    private Stage stage = Stage.NotOpen;
    private void RefreshUI()
    {
        stage = DragonBoatFestivalManager.Instance.GetStage();
        switch (stage)
        {
            case Stage.CanSelectTeam:
                buttonPanel.gameObject.SetActive(true);
                dragonPanel.gameObject.SetActive(true);
                joinPanel.gameObject.SetActive(true);
                tasteImage.gameObject.SetActive(true);
                bgMoveSpeed = 60f;
                RefreshDrumCount();
                RefreshTopPlayer();
                CheckAndStartFlyAnim();
                PlayBoatWaitAnim();
                break;
            case Stage.NormalPlaying:
                buttonPanel.gameObject.SetActive(true);
                dragonPanel.gameObject.SetActive(true);
                upPanel.gameObject.SetActive(true);
                bgMoveSpeed = 100f;
                RefreshDrumCount();
                RefreshTopPlayer();
                RefreshUpPanel();
                CheckAndStartFlyAnim();
                PlayBoatPlayingAnim();
                break;
            case Stage.Ending:
                buttonPanel.gameObject.SetActive(true);
                dragonPanel.gameObject.SetActive(true);
                winPanel.gameObject.SetActive(true);
                bgMoveSpeed = 60f;
                RefreshDrumCount();
                RefreshTopPlayer();
                RefreshWinPanel();
                PlayBoatTeamWinAnim();
                break;
            case Stage.NotOpen:
                notOpenText.gameObject.SetActive(true);
                break;
            case Stage.Closed:
                closedText.gameObject.SetActive(true);
                break;
        }
        RefreshTime();
        RefreshUIRedDot();
    }

    private void OnRefreshGoods(object[] _)
    {
        RefreshDrumCount();
    }
    private async void RefreshDrumCount()
    {
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, activityData.cfg.Param1, 0);
        drumIconImage.sprite = await gameItem.GetIcon();
        drumCountText.text = gameItem.GetPlayerCount().ToString();
    }
    private void OnClickDrumButton(BabuButton _)
    {
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, activityData.cfg.Param1, 0);
        ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties(gameItem);
        itemtipsUIProperties.SetPos(drumIconImage.transform, new Vector3(0, -20f, 0));
        UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
    }
    private void RefreshTopPlayer()
    {
        bool hasLeader = DragonBoatFestivalManager.Instance.courseData.Leaders.Count == 2;
        leftNamePanel.gameObject.SetActive(hasLeader);
        rightNamePanel.gameObject.SetActive(hasLeader);
        if (!hasLeader) return;
        leftServerText.text = "{0}区".SafeFormat(DragonBoatFestivalManager.Instance.courseData.Leaders[0].ServerId);
        leftNameText.text = DragonBoatFestivalManager.Instance.courseData.Leaders[0].Name;
        leftClubIconImage.SetIcon(DragonBoatFestivalManager.Instance.courseData.Leaders[0].Icon);
        rightServerText.text = "{0}区".SafeFormat(DragonBoatFestivalManager.Instance.courseData.Leaders[1].ServerId);
        rightNameText.text = DragonBoatFestivalManager.Instance.courseData.Leaders[1].Name;
        rightClubIconImage.SetIcon(DragonBoatFestivalManager.Instance.courseData.Leaders[1].Icon);
    }
    private int leftOldMeter = 0;
    private int rightOldMeter = 0;
    private void RefreshUpPanel()
    {
        leftSelectImage.gameObject.SetActive(DragonBoatFestivalManager.Instance.myTeam == Team.Left);
        rightSelectImage.gameObject.SetActive(DragonBoatFestivalManager.Instance.myTeam == Team.Right);
        bool hasMeter = DragonBoatFestivalManager.Instance.courseData.Meters.Count == 2;
        leftLengthText.gameObject.SetActive(hasMeter);
        rightLengthText.gameObject.SetActive(hasMeter);
        if (hasMeter)
        {

            int leftNewMeter = DragonBoatFestivalManager.Instance.courseData.Meters[0];
            if (leftOldMeter != leftNewMeter)
            {
                DOChangeNumberEx(leftLengthText, leftOldMeter, leftNewMeter, 5f, "{0}<color=#FFFFFF>米</color>");
                leftOldMeter = leftNewMeter;
            }

            int rightNewMeter = DragonBoatFestivalManager.Instance.courseData.Meters[1];
            if (rightOldMeter != rightNewMeter)
            {
                DOChangeNumberEx(rightLengthText, rightOldMeter, rightNewMeter, 5f, "{0}<color=#FFFFFF>米</color>");
                rightOldMeter = rightNewMeter;
            }

        }
    }
    /// <summary>
    /// 开头带放大效果的 滚动数字动画
    /// </summary>
    public static Sequence DOChangeNumberEx(TMP_Text txt, int fromNum, int toNum, float duration, string stringFormat)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(txt.transform.DOScale(1.2f, 0.2f));
        seq.Append(DOTween.To(value => txt.text = stringFormat.SafeFormat(Mathf.RoundToInt(value).ToString("###,###")), fromNum, toNum, duration));
        seq.Append(txt.transform.DOScale(1f, 0.2f));
        return seq;
    }
    private void RefreshWinPanel()
    {
        bool hasMeter = DragonBoatFestivalManager.Instance.courseData.Meters.Count == 2;
        bool isLeftWin = hasMeter && DragonBoatFestivalManager.Instance.courseData.Meters[0] >= DragonBoatFestivalManager.Instance.courseData.Meters[1];
        bool isRightWin = hasMeter && DragonBoatFestivalManager.Instance.courseData.Meters[0] <= DragonBoatFestivalManager.Instance.courseData.Meters[1];
        leftWinPanel.gameObject.SetActive(isLeftWin);
        rightWinPanel.gameObject.SetActive(isRightWin);
        leftLengthTextWin.gameObject.SetActive(hasMeter);
        rightLengthTextWin.gameObject.SetActive(hasMeter);
        if (hasMeter)
        {
            leftLengthTextWin.text = "{0}<color=#FFFFFF>米</color>".SafeFormat(DragonBoatFestivalManager.Instance.courseData.Meters[0]);
            rightLengthTextWin.text = "{0}<color=#FFFFFF>米</color>".SafeFormat(DragonBoatFestivalManager.Instance.courseData.Meters[1]);
        }
    }

    #endregion

    #region 刷新时间

    private bool needRefreshTime = false;
    private void HideTime()
    {
        timeRoot.gameObject.SetActive(false);
        needRefreshTime = false;
    }
    private void RefreshLeftTimeOneSec()
    {
        if (needRefreshTime) RefreshTime();
    }
    [SerializeField] private RectTransform timeRoot = null;
    private void RefreshTime()
    {
        if (stage == Stage.CanSelectTeam || stage == Stage.NormalPlaying)
        {
            long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
            if (leftTime <= 0)
            {
                HideAll();
                RefreshUI();
            }
            else
            {
                needRefreshTime = true;
                timeRoot.gameObject.SetActive(true);
                leftTimeText.text = "助力时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
            }
        }
        else if (stage == Stage.Ending)
        {
            long leftTime = activityData.HideTime - Utils.DataConvUtil.ServerTime;
            if (leftTime <= 0)
            {
                HideAll();
                RefreshUI();
            }
            else
            {
                needRefreshTime = true;
                timeRoot.gameObject.SetActive(true);
                leftTimeText.text = "活动时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
            }
        }
        else
        {
            HideTime();
        }
    }

    #endregion


    #region 动画

    #region 鸟
    [SerializeField] private RectTransform flyRoot = null;
    [SerializeField] private RectTransform flySkeletonGraphicRect = null;
    private Sequence flySeq = null;
    private void ClearFlyAnim()
    {
        realFlySeq?.Kill();
        realFlySeq = null;
        flySeq?.Kill();
        flySeq = null;
        flyRoot.gameObject.SetActive(false);
    }
    private void CheckAndStartFlyAnim()
    {
        if (flySeq == null)
        {
            RestartFlyAnim();
        }
    }
    private readonly Vector2 flyStartPoint = new(294, -402);
    private readonly Vector2 flyStopPoint = new(-1200, 1681);
    private readonly float flyTime = 2f;
    private readonly float flyWaitTime = 8f;
    private void RestartFlyAnim()
    {
        ClearFlyAnim();
        flyRoot.gameObject.SetActive(true);
        flySeq = DOTween.Sequence();
        flySeq.AddTo(this.gameObject);
        flySeq.AppendCallback(() =>
        {
            RealFlyAnim();
        });
        flySeq.AppendInterval(flyWaitTime + flyTime);
        flySeq.SetLoops(-1);
    }
    private Sequence realFlySeq = null;
    private void RealFlyAnim()
    {
        realFlySeq?.Kill();
        realFlySeq = null;
        realFlySeq = DOTween.Sequence();
        realFlySeq.AddTo(this.gameObject);
        float randomOffsetX = Utility.GetRandomFloat(-300, 300);
        realFlySeq.AppendCallback(() =>
        {
            flySkeletonGraphicRect.anchoredPosition = flyStartPoint + new Vector2(randomOffsetX, 0);
        });
        realFlySeq.Append(flySkeletonGraphicRect.DOAnchorPos(flyStopPoint + new Vector2(randomOffsetX, 0), flyTime).SetEase(Ease.InSine));
    }
    #endregion

    #region 水

    private readonly float bgPicHeight = 1680;
    [SerializeField] private List<RectTransform> bgTransList = new();//两个位置
    [SerializeField] private List<Image> bgImageList = new();//两个位置的图
    [SerializeField] private List<Image> bgPicList = new();//存图片的
    private int picIndex = 1;
    private void ClearBgAnim()
    {
        foreach (var item in bgTransList)
        {
            item.gameObject.SetActive(false);
        }

        bgImageList[0].sprite = bgPicList[0].sprite;
        bgTransList[0].gameObject.SetActive(true);
        bgTransList[0].SetAnchoredPositionY(0);

        bgImageList[1].sprite = bgPicList[1].sprite;
        bgTransList[1].gameObject.SetActive(true);
        bgTransList[1].SetAnchoredPositionY(bgPicHeight);

        picIndex = 1;
    }
    private bool isMovingBg = false;
    private void PlayBgAnim()
    {
        isMovingBg = true;
    }
    private float bgMoveSpeed = 40f;
    private void MoveBgOnce()
    {
        float moveY = Time.deltaTime * bgMoveSpeed;
        foreach (var item in bgTransList)
        {
            item.SetAnchoredPositionY(item.anchoredPosition.y - moveY);
        }
        if (bgTransList[0].anchoredPosition.y < -bgPicHeight)
        {
            bgTransList[0].SetAnchoredPositionY(bgTransList[1].anchoredPosition.y + bgPicHeight);
            picIndex = (picIndex + 1) % bgPicList.Count;
            bgImageList[0].sprite = bgPicList[picIndex].sprite;
        }
        if (bgTransList[1].anchoredPosition.y < -bgPicHeight)
        {
            bgTransList[1].SetAnchoredPositionY(bgTransList[0].anchoredPosition.y + bgPicHeight);
            picIndex = (picIndex + 1) % bgPicList.Count;
            bgImageList[1].sprite = bgPicList[picIndex].sprite;
        }
    }
    private void Update()
    {
        if (isMovingBg) MoveBgOnce();
    }

    #endregion

    #region 船

    [SerializeField] private SkeletonGraphic leftBoatSkeletonGraphic = null;
    [SerializeField] private SkeletonGraphic rightBoatSkeletonGraphic = null;
    [SerializeField] private RectTransform leftBoatPanel = null;
    [SerializeField] private RectTransform rightBoatPanel = null;
    private Sequence boatWaitSeq = null;
    private void ClearBoatWaitAnim()
    {
        boatWaitSeq?.Kill();
        boatWaitSeq = null;
    }
    private void PlayBoatWaitAnim()
    {
        ClearBoatPlayingAnim();
        leftBoatSkeletonGraphic.AnimationState.SetAnimation(0, "manhua", true);
        rightBoatSkeletonGraphic.AnimationState.SetAnimation(0, "manhua", true);
        boatWaitSeq = DOTween.Sequence();
        boatWaitSeq.AddTo(this.gameObject);
        boatWaitSeq.Append(leftBoatPanel.DOAnchorPosY(0, 5f));
        boatWaitSeq.Join(rightBoatPanel.DOAnchorPosY(0, 5f));
    }
    private readonly float boatPlayingWinOffsetY = 40f;
    private readonly float boatPlayingLoseOffsetY = 120f;
    private Sequence boatPlayingSeq = null;
    private void ClearBoatPlayingAnim()
    {
        boatPlayingSeq?.Kill();
        boatPlayingSeq = null;
    }
    private void PlayBoatPlayingAnim()
    {
        if (fastBoatSeq != null) return;
        ClearBoatPlayingAnim();
        drumDotNodeImg.gameObject.SetActive(DragonBoatFestivalManager.Instance.CanUseDrum);
        bool hasMeter = DragonBoatFestivalManager.Instance.courseData.Meters.Count == 2;
        bool isLeftWin = hasMeter && DragonBoatFestivalManager.Instance.courseData.Meters[0] >= DragonBoatFestivalManager.Instance.courseData.Meters[1];
        bool isRightWin = hasMeter && DragonBoatFestivalManager.Instance.courseData.Meters[0] <= DragonBoatFestivalManager.Instance.courseData.Meters[1];
        leftBoatSkeletonGraphic.AnimationState.SetAnimation(0, "manhua", true);
        rightBoatSkeletonGraphic.AnimationState.SetAnimation(0, "manhua", true);
        boatPlayingSeq = DOTween.Sequence();
        boatPlayingSeq.AddTo(this.gameObject);
        boatPlayingSeq.Append(leftBoatPanel.DOAnchorPosY(isLeftWin ? -boatPlayingWinOffsetY : -boatPlayingLoseOffsetY, 5f));
        boatPlayingSeq.Join(rightBoatPanel.DOAnchorPosY(isRightWin ? -boatPlayingWinOffsetY : -boatPlayingLoseOffsetY, 5f));
    }
    private readonly float boatEndWinOffsetY = 80f;
    private Sequence boatWinSeq = null;
    private void ClearBoatWinAnim()
    {
        boatWinSeq?.Kill();
        boatWinSeq = null;
    }
    private void PlayBoatTeamWinAnim()
    {
        ClearBoatWinAnim();
        bool hasMeter = DragonBoatFestivalManager.Instance.courseData.Meters.Count == 2;
        bool isLeftWin = hasMeter && DragonBoatFestivalManager.Instance.courseData.Meters[0] >= DragonBoatFestivalManager.Instance.courseData.Meters[1];
        bool isRightWin = hasMeter && DragonBoatFestivalManager.Instance.courseData.Meters[0] <= DragonBoatFestivalManager.Instance.courseData.Meters[1];
        leftBoatSkeletonGraphic.AnimationState.SetAnimation(0, "manhua", true);
        rightBoatSkeletonGraphic.AnimationState.SetAnimation(0, "manhua", true);
        boatWinSeq = DOTween.Sequence();
        boatWinSeq.AddTo(this.gameObject);
        boatWinSeq.Append(leftBoatPanel.DOAnchorPosY(isLeftWin ? 0 : -boatEndWinOffsetY, 5f));
        boatWinSeq.Join(rightBoatPanel.DOAnchorPosY(isRightWin ? 0 : -boatEndWinOffsetY, 5f));
    }
    private Sequence fastBoatSeq = null;
    private void ClearFastBoatAnim()
    {
        fastBoatSeq?.Kill();
        fastBoatSeq = null;
        drumImageButtonImage.SetAlpha(1);
        drumSkeletonGraphic.gameObject.SetActive(false);
        drumDotNodeImg.gameObject.SetActive(DragonBoatFestivalManager.Instance.CanUseDrum);
    }
    private void OnUpDragonBoatFestivalTeam(object[] _)
    {
        PlayFastBoatAnim();
    }
    private readonly float spineFrameTime = 1f / 30f;
    private readonly int drumFrame = 7;
    private readonly int drumOneGroupCount = 3;
    private readonly int drumOneGroupAllFrameCount = 23;
    private readonly int drumGroupCount = 6;
    private void PlayFastBoatAnim()
    {
        ClearFastBoatAnim();
        ClearBoatPlayingAnim();
        drumDotNodeImg.gameObject.SetActive(false);
        fastBoatSeq = DOTween.Sequence();
        fastBoatSeq.AddTo(this.gameObject);
        bool hasMeter = DragonBoatFestivalManager.Instance.courseData.Meters.Count == 2;
        bool isLeftWin = hasMeter && DragonBoatFestivalManager.Instance.courseData.Meters[0] >= DragonBoatFestivalManager.Instance.courseData.Meters[1];
        bool isRightWin = hasMeter && DragonBoatFestivalManager.Instance.courseData.Meters[0] <= DragonBoatFestivalManager.Instance.courseData.Meters[1];
        SkeletonGraphic boatSkeletonGraphicFast = DragonBoatFestivalManager.Instance.myTeam == Team.Left ? leftBoatSkeletonGraphic : rightBoatSkeletonGraphic;
        RectTransform boatPanelFast = DragonBoatFestivalManager.Instance.myTeam == Team.Left ? leftBoatPanel : rightBoatPanel;
        SkeletonGraphic boatSkeletonGraphicSlow = DragonBoatFestivalManager.Instance.myTeam == Team.Right ? leftBoatSkeletonGraphic : rightBoatSkeletonGraphic;
        RectTransform boatPanelSlow = DragonBoatFestivalManager.Instance.myTeam == Team.Right ? leftBoatPanel : rightBoatPanel;
        boatSkeletonGraphicFast.AnimationState.SetAnimation(0, "kuaihua", true);
        boatSkeletonGraphicSlow.AnimationState.SetAnimation(0, "manhua", true);
        drumImageButtonImage.SetAlpha(0);
        drumSkeletonGraphic.gameObject.SetActive(true);
        drumSkeletonGraphic.AnimationState.SetEmptyAnimation(0, 0);
        drumSkeletonGraphic.AnimationState.SetAnimation(0, "animation", true);
        fastBoatSeq.Append(boatPanelFast.DOAnchorPosY(0, 4.6f));
        Team slowTeam = DragonBoatFestivalManager.Instance.myTeam == Team.Left ? Team.Right : Team.Left;
        bool isSlowTeamWin = slowTeam == Team.Left ? isLeftWin : isRightWin;
        float slowOffsetY = isSlowTeamWin ? -boatPlayingWinOffsetY : -boatPlayingLoseOffsetY;
        fastBoatSeq.Join(boatPanelSlow.DOAnchorPosY(slowOffsetY, 4.6f));
        fastBoatSeq.AppendCallback(() =>
        {
            drumImageButtonImage.SetAlpha(1);
            drumSkeletonGraphic.gameObject.SetActive(false);
            leftBoatSkeletonGraphic.AnimationState.SetAnimation(0, "manhua", true);
            rightBoatSkeletonGraphic.AnimationState.SetAnimation(0, "manhua", true);
        });
        fastBoatSeq.Append(leftBoatPanel.DOAnchorPosY(isLeftWin ? -boatPlayingWinOffsetY : -boatPlayingLoseOffsetY, 5f));
        fastBoatSeq.Join(rightBoatPanel.DOAnchorPosY(isRightWin ? -boatPlayingWinOffsetY : -boatPlayingLoseOffsetY, 5f));
        fastBoatSeq.AppendCallback(() =>
        {
            ClearFastBoatAnim();
        });
        for (int i = 0; i < drumGroupCount; i++)
        {
            for (int j = 0; j < drumOneGroupCount; j++)
            {
                fastBoatSeq.InsertCallback(spineFrameTime * (drumFrame * j + drumOneGroupAllFrameCount * i) - 0.2f, () => { AudioManager.Instance.PlaySound(AudioNames.DRUM); });
            }
        }
        fastBoatSeq.InsertCallback(spineFrameTime * (drumFrame * 0 + drumOneGroupAllFrameCount * drumGroupCount) - 0.2f, () => { AudioManager.Instance.PlaySound(AudioNames.DRUM); });
    }

    #endregion

    #region 鼓

    [SerializeField] private SkeletonGraphic drumSkeletonGraphic = null;
    [SerializeField] private Image drumImageButtonImage = null;

    #endregion

    #region 入场

    [SerializeField] private RectTransform alphaPanel = null;
    private Sequence enterAlphaSeq = null;
    private void ClearEnterAlphaAnim()
    {
        enterAlphaSeq?.Kill();
        enterAlphaSeq = null;
        alphaPanel.gameObject.SetAlpha(0);
    }
    private void PlayEnterAlphaAnim()
    {
        ClearEnterAlphaAnim();
        enterAlphaSeq = DOTween.Sequence();
        enterAlphaSeq.AddTo(this.gameObject);
        enterAlphaSeq.AppendInterval(0.5f);
        enterAlphaSeq.Append(alphaPanel.gameObject.DOFade(1, 1.0f));
    }

    #endregion

    #endregion

}
