using Babu;
using BigBang.Animation;
using BigBang.UI;
using Coffee.UIEffects;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace BigBang.Battle
{
    public enum ShootUIEnterPos
    {
        Unknow,
        tinyFun,
        Jump,
    }
    public class ShootUIProperties : PanelProperties
    {
        public ShootUIEnterPos shootUIEnterPos = ShootUIEnterPos.Unknow;
        public ShootUIProperties(ShootUIEnterPos shootUIEnterPos)
        {
            this.shootUIEnterPos = shootUIEnterPos;
        }
    }
    public class ShootUI : APanelController<ShootUIProperties>
    {
        #region 初始化

        #region 基础
        protected override void AddListeners()
        {
            base.AddListeners();

            DragArea.DragBeginAction += DragBegin;
            DragArea.DragMoveAction += DragMove;
            DragArea.DragEndAction += DragEnd;
            closeBtn.onClick.AddListener(OnClose);
            startButton.OnClick += OnClickStartButton;
            helpButton.OnClick += OnClickHelpButton;
            rankButton.OnClick += OnClickRankButton;

            RegDebugEvents();
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            DragArea.DragBeginAction -= DragBegin;
            DragArea.DragMoveAction -= DragMove;
            DragArea.DragEndAction -= DragEnd;
            closeBtn.onClick.RemoveListener(OnClose);
            startButton.OnClick -= OnClickStartButton;
            helpButton.OnClick -= OnClickHelpButton;
            rankButton.OnClick -= OnClickRankButton;

            UnRegDebugEvents();
        }

        [SerializeField] private GameObject QuickDebugPanel;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            AudioManager.Instance.PlayMusic(AudioNames.BATTLE_BG);

            if (GuideManager.IsFinished(GuideID.guideShootGame) == false) GuideManager.DoGuide(GuideID.guideShootGame);

            Process3DRes();

            InitOnce();

            CreateBallPool();

            RestartBattle();
        }

        [SerializeField] private RectTransform scorePanel = null;
        [SerializeField] private TMP_Text pullTipText = null;
        [SerializeField] private RectTransform ballSequencePanel = null;
        [SerializeField] private RectTransform levelUpPanel = null;
        private void RestartBattle()
        {
            Clear();

            ResetTimeLeft();
            ResetScore();
            ResetStage();
            ResetStartPanel();

            scorePanel.gameObject.SetActive(false);
            startPanel.gameObject.SetActive(true);
            pullTipText.gameObject.SetActive(false);
            ballSequencePanel.gameObject.SetActive(false);
            AimPanel.gameObject.SetActive(false);
            levelUpPanel.gameObject.SetActive(false);
            RefreshDiamondText();
        }
        private void AfterCount()
        {
            PrepareNewBall();
        }

        [SerializeField] private Button closeBtn;
        private bool isPopWindowShowing = false;
        private void OnClose()
        {
            isPopWindowShowing = true;
            ClearSendBallAni();
            ClearCount321Ani();
            ClearBallAni();
            ClearTimeLeftAni();
            ClearBallShakeAni();
            ClearCircleLightAni();
            ClearPingPongAni();
            ClearShowPointerAni();
            ClearHidePointerAni();
            AimPanel.SetAlpha(0);
            DragArea.gameObject.SetActive(false);
            if (startPanel.gameObject.activeSelf == true)
            {
                ShootEndRewardUI.GoToOut(Properties.shootUIEnterPos);
            }
            else
            {
                UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("要立刻结束游戏吗？", OnGameRealEnd, () =>
                {
                    isPopWindowShowing = false;
                    PlayCount321Ani(() =>
                    {
                        PrepareNewBall();
                        PlayTimeLeftAni();
                    });
                }));
            }
        }

        private void Clear()
        {
            ClearSendBallAni();
            ClearLevelUpAni();
            ClearGuideFingerAni();
            ClearBallAni();
            ClearShakeHoopAni();
            ClearPingPongAni();
            ResetPingPongPos();
            ClearCircleLightAni();
            ClearPointerMidLightAni();
            ClearBallPosResetAni();
            ClearHidePointerAni();
            ClearBallShakeAni();
            ClearGoodPopAni();
            ClearTimeLeftAni();
            ClearShowPointerAni();
            ClearCount321Ani();
            AimPanel.SetAlpha(1f);
            DragArea.gameObject.SetActive(false);
            isSendingCd = false;
        }
        private bool isInitOnce = false;
        private void InitOnce()
        {
            if (isInitOnce == true) return;
            isInitOnce = true;

            SetBallStartPos();
            InitCircleLightPool();
            InitPointerMidLightPool();
            InitTimePanelPos();
        }
        #endregion

        #region 顶部，说明按钮，排行榜按钮，钻石数量

        [SerializeField] private BabuButton helpButton = null;
        [SerializeField] private BabuButton rankButton = null;

        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<ShootHelpUI>();
        }
        private void OnClickRankButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<ShootRankUI>();
        }

        [SerializeField] private TMP_Text myDiamondNumText = null;
        private void RefreshDiamondText()
        {
            myDiamondNumText.text = Player.PackageManager.Diamond.ToString("N0");
        }

        #endregion

        #region 开始按钮

        [SerializeField] private BabuButton startButton = null;
        [SerializeField] private RectTransform topButtonPanel = null;
        [SerializeField] private RectTransform myDiamondPanel = null;
        [SerializeField] private RectTransform startPanel = null;
        private void OnClickStartButton(BabuButton _)
        {
            bool isFree = Player.ActivityManager.ShootGameTimesLeft > 0;
            if (isFree)
            {
                StartGameCount();
            }
            else
            {
                int costNum = GetCostNum();
                if (costNum > Player.PackageManager.Diamond)
                {
                    Tips.PopError(ErrorID.DiamondNotEnough);
                }
                else
                {
                    UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("确定花费{0}钻石，开始游戏吗？".SafeFormat(costNum), () =>
                    {
                        StartGameCount();
                    }));
                }
            }
        }
        private void StartGameCount()
        {
            topButtonPanel.gameObject.SetActive(false);
            myDiamondPanel.gameObject.SetActive(false);
            startPanel.gameObject.SetActive(false);
            scorePanel.gameObject.SetActive(true);
            startPanel.gameObject.SetActive(false);
            pullTipText.gameObject.SetActive(true);
            ballSequencePanel.gameObject.SetActive(true);
            AimPanel.gameObject.SetActive(true);
            PlayCount321Ani(AfterCount);
        }
        [SerializeField] private RectTransform freePanel = null;
        [SerializeField] private TMP_Text freeTimesText = null;
        [SerializeField] private RectTransform costPanel = null;
        [SerializeField] private TMP_Text costTimesText = null;
        [SerializeField] private TMP_Text costDiamondText = null;
        [SerializeField] private UIShiny startButtonUIShiny = null;
        private void ResetStartPanel()
        {
            topButtonPanel.gameObject.SetActive(true);
            myDiamondPanel.gameObject.SetActive(true);
            startPanel.gameObject.SetActive(true);
            bool isFree = Player.ActivityManager.ShootGameTimesLeft > 0;
            startButtonUIShiny.enabled = isFree;
            freePanel.gameObject.SetActive(isFree);
            costPanel.gameObject.SetActive(!isFree);
            if (isFree)
            {
                freeTimesText.text = "今日免费次数: <color=#40F569>{0}</color>/{1}".SafeFormat(Player.ActivityManager.ShootGameTimesLeft, GameConst.ChallengeTimes);
            }
            else
            {
                costTimesText.text = "第<color=#E43535>{0}</color>次消耗".SafeFormat(Player.ActivityManager.ShootGameTimes + 1);
                costDiamondText.text = "-{0}".SafeFormat(GetCostNum());
            }
        }
        private int GetCostNum()
        {
            int n = Mathf.Max(0, Player.ActivityManager.ShootGameTimes + 1 - GameConst.ChallengeTimes);
            int baseNum = GameConst.ChallengeCostBase;
            int costNum = baseNum * Mathf.RoundToInt(Mathf.Pow(2, n - 1));
            return costNum;
        }

        #endregion

        #region 3D

        [SerializeField] private GameObject shootAsset;
        private GameObject shootGameObject;
        private Transform shootTrans;
        [SerializeField] private RawImage shootImg;
        private Camera shootCamera;
        private Transform circleMidPointTrans;
        private Transform ballShowPointTrans;
        private SpriteRenderer bgSpriteRenderer;
        private Transform timePointTransLeft;
        private Transform timePointTransRight;

        private void Process3DRes()
        {
            shootGameObject = GameObject.Instantiate(shootAsset);
            shootTrans = shootGameObject.transform;
            shootCamera = shootTrans.Find("Main Camera").GetComponent<Camera>();
            CameraManager.Instance.SetTexture(CameraID.Shoot, shootImg);
            circleMidPointTrans = shootTrans.Find("CircleMidPoint");
            ballShowPointTrans = shootTrans.Find("BallShowPoint");
            ballParentTrans = shootTrans.Find("BallParent");
            timePointTransLeft = shootTrans.Find("zhandou_lanqiujia").Find("Dummy009").Find("Lanqiujia_02").Find("TimePointLeft");
            timePointTransRight = shootTrans.Find("zhandou_lanqiujia").Find("Dummy009").Find("Lanqiujia_02").Find("TimePointRight");
            bgSpriteRenderer = shootTrans.Find("Battle2Bg").GetComponent<SpriteRenderer>();
            Transform lanqiujiaTrans = shootTrans.Find("zhandou_lanqiujia");
            basketAnimator = lanqiujiaTrans.GetComponent<Animator>();
            float cameraFOV = Utility.Lerp(30f, 36f, UIFrame.GetFixScreenLerpT());
            shootCamera.fieldOfView = cameraFOV;
            isCanHide = true;
        }

        private bool isCanHide = false;
        protected override void WhileHiding()
        {
            if (isCanHide == false) return;
            Clear();
            CameraManager.Instance.ReleaseTexture(CameraID.Shoot, shootImg);
            GameObject.Destroy(shootGameObject);
            DesBallPool();
            isCanHide = false;
        }



        #endregion

        #region 蓝球对象池

        [SerializeField] private GameObject ballPrefab;
        private ComponentPool<ShootBall> shootBallPool = new();
        private List<Sequence> ballSeqList = new();
        private Transform ballParentTrans;
        private void CreateBallPool()
        {
            shootBallPool.InitComponentPool(ballPrefab, 2, ballParentTrans, InitBall);
        }
        private void DesBallPool()
        {
            ClearBallAni();
            shootBallPool.DestoryAll();
        }
        private void ClearBallAni()
        {
            foreach (Sequence ballSeq in ballSeqList)
            {
                ballSeq?.Kill();
            }
            ballSeqList.Clear();
            shootBallPool.ClearOutComponent();
        }
        private void InitBall(ShootBall shootBall)
        {
            //shootBall.transform.localScale = Vector3.one * 0.6f;
        }

        #endregion

        #region 设置篮球的初始位置

        private Vector3 GetBallPosIn3D(Vector3 screenPoint)
        {
            Vector3 hitWorldPoint = Vector3.zero;
            screenPoint = UIFrame.ChangeUIScreenPointTo3DScreenPoint(screenPoint);
            Vector3 viewport = shootCamera.ScreenToViewportPoint(screenPoint);
            Ray ray = shootCamera.ViewportPointToRay(viewport);
            //Debug.DrawRay(ray.origin, ray.direction * 200000, Color.red);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.tag == Tags.Shoot)
                {
                    return hits[i].point;
                }
            }
            Debug.LogWarning("GetBallPosIn3D ， 找不到位置 , screenPoint = " + screenPoint);
            return Vector3.zero;
        }

        private Vector3 ballStartPos = Vector3.zero;
        private void SetBallStartPos()
        {
            Vector3 screenPoint = Utility.ConvertLocalPositionToScreenPosition(DragArea.GetComponent<RectTransform>(), Vector3.zero, uiCamera);
            ballStartPos = GetBallPosIn3D(screenPoint);
        }

        #endregion

        #region 结算

        public class ShootEndData
        {
            public int oldScore = 0;
            public int newScore = 0;
            public ShootUIEnterPos ShootUIEnterPos = ShootUIEnterPos.Unknow;
        }
        private bool isSendingCd = false;
        private void OnGameRealEnd()
        {
            Debug.Log("OnGameRealEnd , isSendingCd = {0} ".SafeFormat(isSendingCd));
            if (isSendingCd)
            {
                return;
            }
            isSendingCd = true;
            UnityTimer.Timer.Register(this.gameObject, 2f, () =>
            {
                isSendingCd = false;
            });
            Debug.Log("OnGameRealEnd , 游戏结束 , 等级 = {0} , 分数 = {1}".SafeFormat(shootGameStageConfig.Id, score));
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlayMusic(AudioNames.BGM_HOME);
            Clear();
            isSendingCd = true;
            AimPanel.SetAlpha(0);
            pullTipText.gameObject.SetActive(false);


            NetworkManager.Instance.GetShootGameReward(0, score, resp =>
            {
                ShootEndData shootEndData = new();
                shootEndData.oldScore = Player.ActivityManager.ShootGameTodayPoint;
                shootEndData.newScore = score;
                shootEndData.ShootUIEnterPos = Properties.shootUIEnterPos;
                UIController.Instance.OpenWindow<ShootEndRewardUI>(new ShootEndRewardUIProperties(shootEndData));

                if (score > Player.ActivityManager.ShootGameTodayPoint)
                {
                    Player.ActivityManager.ShootGameTodayPoint = score;//推送刷新?
                }
                Player.ActivityManager.ShootGameTimesLeft--;
                Player.ActivityManager.ShootGameTimes++;
                Player.ActivityManager.RefreshChallengeRedDot();
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                //EventManager.Instance.Dispatch(EventID.OnHomeUIRedDotReady, PanelNodePath.Home_Games, 7);
            });
        }

        public enum ShootTaskType
        {
            Time,
            Ball2,
            Ball3,
            Score,
            Consecutive,
            BigGift,
        }

        public class ShootTargetItemData
        {
            public ShootTaskType shootTaskType = ShootTaskType.Time;
            public int targetValue = 99;
            public int nowValue = 0;
            public bool success = false;
        }


        #endregion

        #endregion

        #region 游戏逻辑

        private ShootBall nowBall;
        private void PutNewBall()
        {
            Debug.Log("PutNewBall");
            nowBall = shootBallPool.GetComponentFormPool();
            nowBall.SetShootBallScoreType(IsNowBallScore3 ? ShootBallScoreType.Three : ShootBallScoreType.Two);
            nowBall.transform.position = ballStartPos;
            nowBall.ballTrans2.localRotation = Quaternion.Euler(Utility.GetRandomFloat(0, 360), Utility.GetRandomFloat(0, 360), Utility.GetRandomFloat(0, 360));
            nowBall.ballTrans3.localRotation = Quaternion.Euler(Utility.GetRandomFloat(0, 360), Utility.GetRandomFloat(0, 360), Utility.GetRandomFloat(0, 360));
            nowBall.HideTrail();
        }
        [SerializeField] private GameObject AimPanel;
        private void SendBall()
        {
            ClearBallShakeAni();
            ClearPingPongAni();
            DragArea.gameObject.SetActive(false);
            nowBall.ShowTrail();
            nowBall.ClearTrail();
            PlayHidePointerAni(() =>
            {
                PlaySendBallAni(() =>
                {
                    if (isPopWindowShowing == false)
                    {
                        PrepareNewBall();
                    }
                });
            });

        }

        private void PrepareNewBall()
        {
            DragArea.gameObject.SetActive(true);
            SetArrowHeight(1);
            SetArrowColor(1);
            SetPointerSpeed(1);
            PutNewBall();
            StartPingPongAni();
            ClearHidePointerAni();
            ClearBallShakeAni();
            PlayShowPointerAni();
            if (GuideManager.IsGuideDoing(GuideID.guideShootGame))
            {
                PlayGuideFingerAni();
                return;
            }
            PlayTimeLeftAni();
        }
        private void OnGameEnd()
        {
            Debug.Log("OnGameEnd");
            ClearTimeLeftAni();
            DragArea.gameObject.SetActive(false);
            UnityTimer.Timer.Register(this.gameObject, 1.5f, OnGameRealEnd);
        }

        #endregion

        #region 动画

        #region 拖动球，圆圈变化

        [SerializeField] private DragActionComponent DragArea;
        [SerializeField] private RectTransform CirclePanelTrans;
        private void DragBegin(PointerEventData eventData)
        {
            if (GuideManager.IsGuideDoing(GuideID.guideShootGame))
            {
                PlayTimeLeftAni();
                ClearGuideFingerAni();
                GuideManager.Finish(GuideID.guideShootGame);
            }

            AudioManager.Instance.PlaySound(AudioNames.BTN_SELECT);
            StartPingPongAni();
        }

        private Vector3 ballLocalPoint;
        private float diatanceT = 1;
        [SerializeField] private RectTransform aimPanelTrans;
        [SerializeField] private RectTransform ballBottomLimitPoint;
        private readonly float shakeDiatance = 45f;
        private void DragMove(PointerEventData eventData)
        {
            if (nowBall == null) return;
            ClearBallPosResetAni();

            ballLocalPoint = GetBallPositionLimit(eventData.position);

            float distance = Vector3.Distance(ballLocalPoint, CirclePanelTrans.localPosition);

            bool isShake = distance < shakeDiatance;
            if (isShake)
            {
                StartBallShakeAni();
            }
            else
            {
                ClearBallShakeAni();
                Vector3 ballScreenPoint = Utility.ConvertLocalPositionToScreenPosition(aimPanelTrans, ballLocalPoint, uiCamera);
                nowBall.transform.position = GetBallPosIn3D(ballScreenPoint);
            }


            diatanceT = GetT(distance);

            SetArrowHeight(diatanceT);
            SetArrowColor(diatanceT);
            SetPointerSpeed(Mathf.Lerp(10f, 1f, diatanceT));
        }
        private void DragEnd(PointerEventData eventData)
        {
            if (nowBall == null) return;

            ballLocalPoint = GetBallPositionLimit(eventData.position);
            float distance = Vector3.Distance(ballLocalPoint, CirclePanelTrans.localPosition);
            bool isShake = distance < shakeDiatance;
            if (!isShake)
            {
                PlayBallPosResetAni();
                return;
            }

            AudioManager.Instance.PlaySound(AudioNames.BTN_TARGET);
            SendBall();
        }
        private Camera _uiCamera;
        private Camera uiCamera
        {
            get
            {
                if (_uiCamera == null)
                {
                    _uiCamera = UIController.Instance.GetCamera();
                }
                return _uiCamera;
            }
        }

        [SerializeField] private RectTransform DragAreaTrans;
        private Vector3 GetBallPositionLimit(Vector3 realTouchScreenPoint)
        {
            Vector3 ballLocalPoint = Utility.ConvertScreenPositionToLocalPosition(aimPanelTrans, realTouchScreenPoint, uiCamera);
            if (ballLocalPoint.y > DragAreaTrans.localPosition.y)
            {
                ballLocalPoint = DragAreaTrans.localPosition;
            }
            if (ballLocalPoint.y < ballBottomLimitPoint.localPosition.y)
            {
                ballLocalPoint = ballBottomLimitPoint.localPosition;
            }
            ballLocalPoint.x = 0;
            return ballLocalPoint;
        }

        float minDistance = 10.0f;
        float maxDistance = 100.0f;
        /// <returns>0代表完全正确，1代表完全不对</returns>
        private float GetT(float distance)
        {
            maxDistance = Vector3.Distance(DragAreaTrans.localPosition, CirclePanelTrans.localPosition);
            if (distance < minDistance) return 0;
            if (distance > maxDistance) return 1;
            float t = (distance - minDistance) / (maxDistance - minDistance);
            return t;
        }

        [SerializeField] private List<RectTransform> arrowTransList = new();
        private float arrowHeightT = 1f;
        private void SetArrowHeight(float t)
        {
            arrowHeightT = t;
            float length = Mathf.Lerp(62.25f, 82.25f, t);
            foreach (RectTransform arrowTrans in arrowTransList)
            {
                arrowTrans.SetLocalPositionY(length);
            }
        }

        [SerializeField] private List<Image> arrowImageList = new();
        [SerializeField] private Color greenColor;
        [SerializeField] private Color orangeColor;
        private void SetArrowColor(float t)
        {
            Color nowColor = Color.Lerp(greenColor, orangeColor, t);
            foreach (Image arrowImage in arrowImageList)
            {
                arrowImage.color = nowColor;
            }
        }


        #endregion

        #region 松手复位

        private Sequence ballPosResetSeq;
        private void ClearBallPosResetAni()
        {
            ballPosResetSeq?.Kill();
            ballPosResetSeq = null;
        }
        private void PlayBallPosResetAni()
        {
            ClearBallPosResetAni();
            ChangeSpeedByMaxDistance();
            ballPosResetSeq = DOTween.Sequence();
            ballPosResetSeq.Append(nowBall.transform.DOMove(ballStartPos, 0.2f).SetEase(Ease.OutBack));
        }

        #endregion

        #region 指针摆动
        Sequence pingPongAniSeq;
        private void ClearPingPongAni()
        {
            pingPongAniSeq?.Kill();
            pingPongAniSeq = null;
        }
        [SerializeField] private RectTransform pointerRootTrans;
        private float pointerMoveTime
        {
            get
            {
                return Mathf.Lerp(pointerMoveTimeMin, pointerMoveTimeMax, shootGameStageConfig.SlideParam / 100f);
            }
        }
        private float pointerMoveTimeMin = 20f;
        private float pointerMoveTimeMax = 0.1f;
        private void StartPingPongAni()
        {
            ClearPingPongAni();
            ResetPingPongPos();
            pingPongAniSeq = DOTween.Sequence();
            pingPongAniSeq.timeScale = pointerMoveTimeScale;
            pingPongAniSeq.Append(pointerRootTrans.DOLocalRotate(new Vector3(0, 0, -28.5f), pointerMoveTime).SetEase(Ease.Linear));
            pingPongAniSeq.InsertCallback(pointerMoveTime / 2, PlayMidAni);
            pingPongAniSeq.Append(pointerRootTrans.DOLocalRotate(new Vector3(0, 0, 28.5f), pointerMoveTime).SetEase(Ease.Linear));
            pingPongAniSeq.InsertCallback(pointerMoveTime + pointerMoveTime / 2, PlayMidAni);
            pingPongAniSeq.SetLoops(-1, LoopType.Restart);
        }
        private void ResetPingPongPos()
        {
            pointerRootTrans.localRotation = Quaternion.Euler(new Vector3(0, 0, 28.5f));
        }
        private void PlayMidAni()
        {
            PlayCircleLightAni();
            PlayPointerMidLightAni();
        }
        private float pointerMoveTimeScale = 1f;
        private void SetPointerSpeed(float pointerMoveTimeScale)
        {
            //Debug.Log("SetPointerSpeed , pointerMoveTimeScale = " + pointerMoveTimeScale);
            this.pointerMoveTimeScale = pointerMoveTimeScale;
            if (pingPongAniSeq != null)
            {
                pingPongAniSeq.timeScale = pointerMoveTimeScale;
            }
        }
        private void ChangeSpeedByMaxDistance()
        {
            ballLocalPoint = DragAreaTrans.localPosition;
            float distance = Vector3.Distance(ballLocalPoint, CirclePanelTrans.localPosition);
            diatanceT = GetT(distance);
            SetArrowHeight(diatanceT);
            SetArrowColor(diatanceT);
            SetPointerSpeed(Mathf.Lerp(10f, 1f, diatanceT));
        }

        #endregion

        #region 篮球飞出

        private float enterAngle = 5f;//正负多少度内判定进球
        bool isEnter = true;//是否进球
        bool isAngleRight = true;//角度是否正确
        bool isLeft = true;//是否角度偏左
        bool isStrengthRight = true;//力度是否正确
        bool isUp = true;//是否力气过大

        Sequence sendBallseq = null;
        private void ClearSendBallAni()
        {
            sendBallseq?.Kill();
            sendBallseq = null;
        }

        private void PlaySendBallAni(Action OnPlayEnd)
        {
            ClearSendBallAni();
            ShootBall moveBall = nowBall;

            isEnter = true;//是否进球
            isAngleRight = true;//角度是否正确
            //isStrengthRight = true;//力度是否正确

            //float distance = Vector3.Distance(ballLocalPoint, CirclePanelTrans.localPosition);
            //if (distance > 45f)//移除力度判定
            //{
            //    isEnter = false;
            //    isStrengthRight = false;
            //}
            //isUp = ballLocalPoint.y < CirclePanelTrans.localPosition.y;//是否力气过大

            float angle = pointerRootTrans.localRotation.eulerAngles.z;
            if (angle > 180) angle -= 360;
            Debug.Log("diatanceT = " + diatanceT);
            Debug.Log("angle = " + angle);
            if (Mathf.Abs(angle) > enterAngle)
            {
                isEnter = false;
                isAngleRight = false;
            }
            isLeft = angle > 0;//是否角度偏左

            sendBallseq = DOTween.Sequence();

            //把球扔到球篮上方
            Vector3 bezierStartPos = moveBall.transform.position;

            Vector3 bezierEndPos = Vector3.zero;
            if (isEnter)
            {
                bezierEndPos = circleMidPointTrans.transform.position + new Vector3(0, 0.7f, 0);
            }
            else
            {
                if (isStrengthRight == false)
                {
                    Vector3 offset = Vector3.zero;
                    offset += new Vector3(0, isUp ? 2.5f : -2.5f, 0);

                    if (isAngleRight == false)
                    {
                        if (isLeft)
                        {
                            bezierEndPos = circleMidPointTrans.transform.position + new Vector3(-0.9f, 0.239f, 0) + new Vector3(0, 0.7f, 0);
                        }
                        else
                        {
                            bezierEndPos = circleMidPointTrans.transform.position + new Vector3(0.9f, 0.239f, 0) + new Vector3(0, 0.7f, 0);
                        }
                    }
                    bezierEndPos += offset;
                }
                else
                {
                    if (isLeft)
                    {
                        bezierEndPos = circleMidPointTrans.transform.position + new Vector3(-0.3f, 0.239f, 0) + new Vector3(0, 0.7f, 0);
                    }
                    else
                    {
                        bezierEndPos = circleMidPointTrans.transform.position + new Vector3(0.3f, 0.239f, 0) + new Vector3(0, 0.7f, 0);
                    }
                }

            }

            Vector3 bezierControlPos = (bezierStartPos + bezierEndPos) / 2 + new Vector3(0, 2.0f, 0);
            float bezierDuration = 0.6f;
            sendBallseq.Append(moveBall.transform.DOBezier2Move(bezierStartPos, bezierControlPos, bezierEndPos, bezierDuration).SetEase(Ease.Linear));
            //seq.Join(nowBall.transform.DOScale(0.9f, bezierDuration).SetEase(Ease.Linear));
            sendBallseq.Join(moveBall.ballRotTrans.DORotate(new Vector3(-360, 0, isLeft ? -60 : 60), bezierDuration, RotateMode.WorldAxisAdd).SetEase(Ease.Linear));

            //加分
            sendBallseq.AppendCallback(() =>
            {
                if (isEnter)
                {
                    if (IsNowBallScore3)
                    {
                        AudioManager.Instance.PlaySound(AudioNames.BATTLE_GOAL2);
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound(AudioNames.BATTLE_GOAL);
                    }
                    int addScore = IsNowBallScore3 ? 3 : 2;
                    AddScore(addScore);
                    CheckStageUp();
                }
                else
                {
                    AudioManager.Instance.PlaySound(AudioNames.BATTLE_SLAM);
                }
                StartGoodPopAni(isEnter);
                PlayShakeHoopAni();
                SetNextBall(isEnter);
            });

            //球进篮筐
            if (isStrengthRight == true)
            {
                float fadeOutDuration = 0.1f;
                float moveOutEndPosY = 0;
                if (isEnter)
                {
                    moveOutEndPosY = circleMidPointTrans.transform.position.y - 0.3f;
                }
                else
                {
                    moveOutEndPosY = circleMidPointTrans.transform.position.y - 0.226f;
                }
                sendBallseq.Append(moveBall.transform.DOMoveY(moveOutEndPosY, fadeOutDuration));
            }

            //未进球弹开
            if (isEnter == false)
            {
                if (isStrengthRight == true)
                {
                    float notEnterDuration = 0.3f;
                    Vector3 notEnterStartPoint = bezierEndPos;
                    notEnterStartPoint.y = circleMidPointTrans.transform.position.y - 0.226f;
                    Vector3 notEnterEndPoint = Vector3.zero;
                    if (isLeft)
                    {
                        notEnterEndPoint = new Vector3(-2.66f, -0.466f, -2.915f);
                    }
                    else
                    {
                        notEnterEndPoint = new Vector3(2.66f, -0.466f, -2.915f);
                    }
                    Vector3 notEnterControlPos = (notEnterStartPoint + notEnterEndPoint) / 2 + new Vector3(0, 2.0f, 0);
                    sendBallseq.Append(moveBall.transform.DOBezier2Move(notEnterStartPoint, notEnterControlPos, notEnterEndPoint, notEnterDuration).SetEase(Ease.Linear));
                }
            }

            sendBallseq.AppendCallback(() =>
            {
                shootBallPool.ReturnComponentToPool(nowBall);
                OnPlayEnd?.Invoke();
            });
        }

        #endregion

        #region 球框晃动

        private void ClearShakeHoopAni()
        {
            shakeHoopSeq?.Kill();
            shakeHoopSeq = null;
            //basketMoveTrans.localPosition = Vector3.zero;
        }

        private Animator basketAnimator;
        private Sequence shakeHoopSeq;
        private float shakeHoopDistanceY = -0.03f;
        private float shakeHoopDownTime = 0.05f;
        private float shakeHoopUpTime = 0.1f;
        private void PlayShakeHoopAni()
        {
            if (shakeHoopSeq != null) return;

            shakeHoopSeq = DOTween.Sequence();

            //shakeHoopSeq.AppendInterval(0.05f);

            //球网晃动
            shakeHoopSeq.AppendCallback(() =>
            {
                basketAnimator.SetTrigger("GoalTrigger");
            });

            ////球篮下沉
            //shakeHoopSeq.Append(basketMoveTrans.DOLocalMoveY(shakeHoopDistanceY, shakeHoopDownTime));

            ////球篮上浮
            //shakeHoopSeq.Append(basketMoveTrans.DOLocalMoveY(0, shakeHoopUpTime));

            //清除动画进行标记
            shakeHoopSeq.AppendCallback(() =>
            {
                shakeHoopSeq = null;
            });
        }

        #endregion

        #region 中间刻度光圈

        [SerializeField] private GameObject circleLightPrefab;
        private ComponentPool<Image> circleLightPool = new();
        private HashSet<Sequence> circleLightSeqSet = new();
        [SerializeField] private RectTransform circleLightParentTrans;
        private void InitCircleLightPool()
        {
            circleLightPool.InitComponentPool(circleLightPrefab, 2, circleLightParentTrans);
        }
        private void ClearCircleLightAni()
        {
            foreach (Sequence circleLightSeq in circleLightSeqSet)
            {
                circleLightSeq?.Kill();
            }
            circleLightSeqSet.Clear();
            circleLightPool.ClearOutComponent();
        }
        private void PlayCircleLightAni()
        {

            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            Sequence circleLightSeq = DOTween.Sequence();
            circleLightSeqSet.Add(circleLightSeq);
            Image circleLightImage = circleLightPool.GetComponentFormPool();
            RectTransform circleLightTrans = circleLightImage.GetComponent<RectTransform>();
            circleLightTrans.localScale = Vector3.zero;
            circleLightImage.SetAlpha(0);
            circleLightSeq.Append(circleLightTrans.DOScale(1.5f, 0.2f));
            circleLightSeq.Join(circleLightImage.DOFade(1f, 0.2f));
            circleLightSeq.Append(circleLightTrans.DOScale(1.8f, 0.3f));
            circleLightSeq.Join(circleLightImage.DOFade(0f, 0.3f));
            circleLightSeq.AppendCallback(() =>
            {
                circleLightSeq?.Kill();
                if (circleLightSeqSet.Contains(circleLightSeq))
                {
                    circleLightSeqSet.Remove(circleLightSeq);
                }
                circleLightPool.ReturnComponentToPool(circleLightImage);
            });
        }

        #endregion

        #region 中间刻度高亮

        [SerializeField] private GameObject pointerMidLightPrefab;
        private ComponentPool<Image> pointerMidLightPool = new();
        private HashSet<Sequence> pointerMidLightSeqSet = new();
        [SerializeField] private RectTransform pointerMidLightParentTrans;
        private void InitPointerMidLightPool()
        {
            pointerMidLightPool.InitComponentPool(pointerMidLightPrefab, 2, circleLightParentTrans);
        }
        private void ClearPointerMidLightAni()
        {
            foreach (Sequence pointerMidLightSeq in pointerMidLightSeqSet)
            {
                pointerMidLightSeq?.Kill();
            }
            pointerMidLightSeqSet.Clear();
            pointerMidLightPool.ClearOutComponent();
        }
        private void PlayPointerMidLightAni()
        {
            Sequence pointerMidLightSeq = DOTween.Sequence();
            pointerMidLightSeqSet.Add(pointerMidLightSeq);
            Image pointerMidLightImage = pointerMidLightPool.GetComponentFormPool();
            RectTransform pointerMidLightTrans = pointerMidLightImage.GetComponent<RectTransform>();
            pointerMidLightImage.SetAlpha(0);
            pointerMidLightSeq.Append(pointerMidLightImage.DOFade(1f, 0.2f));
            pointerMidLightSeq.Append(pointerMidLightImage.DOFade(0f, 0.3f));
            pointerMidLightSeq.AppendCallback(() =>
            {
                pointerMidLightSeq?.Kill();
                if (pointerMidLightSeqSet.Contains(pointerMidLightSeq))
                {
                    pointerMidLightSeqSet.Remove(pointerMidLightSeq);
                }
                pointerMidLightPool.ReturnComponentToPool(pointerMidLightImage);
            });
        }

        #endregion

        #region 松手隐藏指针

        private HashSet<Tween> hidePointerTweenSet = new();
        private void ClearHidePointerAni()
        {
            foreach (Sequence hidePointerSeq in hidePointerTweenSet)
            {
                hidePointerSeq?.Kill();
            }
            hidePointerTweenSet.Clear();
            rulerBgLightImage.SetAlpha(0);
            circleImage.transform.localScale = Vector3.one;
        }

        [SerializeField] private Image circleImage;
        [SerializeField] private Image rulerBgLightImage;
        private void PlayHidePointerAni(Action OnPlayEnd)
        {


            Sequence hidePointerSeq = DOTween.Sequence();
            hidePointerTweenSet.Add(hidePointerSeq);



            rulerBgLightImage.gameObject.SetActive(true);
            rulerBgLightImage.transform.localScale = Vector3.one * 2;
            rulerBgLightImage.SetAlpha(0);
            hidePointerSeq.Append(rulerBgLightImage.DOFade(1, 0.2f));
            hidePointerSeq.Join(rulerBgLightImage.transform.DOScale(1, 0.2f));

            hidePointerSeq.AppendCallback(() =>
            {
                OnPlayEnd?.Invoke();
            });

            hidePointerSeq.Append(AimPanel.DOFade(0, 0.8f));


            //hidePointerSeq.AppendCallback(() =>
            //{
            //    OnPlayEnd?.Invoke();
            //});

            Sequence hidePointerSeq2 = DOTween.Sequence();
            hidePointerTweenSet.Add(hidePointerSeq2);
            foreach (RectTransform arrowTrans in arrowTransList)
            {
                hidePointerSeq2.Join(arrowTrans.DOLocalMoveY(92.25f, 0.8f));
            }
            hidePointerSeq2.Join(circleImage.transform.DOScale(0.5f, 0.8f));
        }

        #endregion

        #region 显示指针

        private HashSet<Tween> showPointerTweenSet = new();
        private void ClearShowPointerAni()
        {
            foreach (Sequence showPointerTween in showPointerTweenSet)
            {
                showPointerTween?.Kill();
            }
            showPointerTweenSet.Clear();
        }

        private void PlayShowPointerAni(Action OnPlayEnd = null)
        {
            ClearShowPointerAni();
            Sequence showPointerSeq = DOTween.Sequence();
            hidePointerTweenSet.Add(showPointerSeq);
            showPointerSeq.Append(AimPanel.DOFade(1, 0.2f));
            showPointerSeq.AppendCallback(() =>
            {
                OnPlayEnd?.Invoke();
            });
        }

        #endregion

        #region 刻度盘和篮球晃动

        Sequence ballShakeAniSeq;
        private void ClearBallShakeAni()
        {
            ballShakeAniSeq?.Kill();
            ballShakeAniSeq = null;
            rulerPanel.localPosition = Vector3.zero;
        }
        private float ballShakeTime = 0.06f;
        [SerializeField] private RectTransform rulerPanel;
        private void StartBallShakeAni()
        {
            ClearBallShakeAni();

            pointerRootTrans.localRotation = Quaternion.Euler(new Vector3(0, 0, 28.5f));
            ballShakeAniSeq = DOTween.Sequence();
            //ballShakeAniSeq.timeScale = pointerMoveTimeScale;
            ballShakeAniSeq.AppendCallback(RandomShake);
            ballShakeAniSeq.AppendInterval(ballShakeTime);
            ballShakeAniSeq.SetLoops(-1, LoopType.Restart);
        }

        private float shakeValue
        {
            get
            {
                return Mathf.Lerp(shakeValueMin, shakeValueMax, shootGameStageConfig.ShakeParam / 100f);
            }
        }
        private float shakeValueMin = 0f;
        private float shakeValueMax = 50f;
        private void RandomShake()
        {
            Vector3 randomRulerShakeOffset = new Vector3(Utility.GetRandomFloat(-shakeValue, shakeValue), Utility.GetRandomFloat(-shakeValue, shakeValue), 0);
            rulerPanel.localPosition = randomRulerShakeOffset;
            float ballRandomRange = Mathf.Lerp(5f, 15f, diatanceT);
            Vector3 randomBallShakeOffset = new Vector3(Utility.GetRandomFloat(-ballRandomRange, ballRandomRange), Utility.GetRandomFloat(-ballRandomRange, ballRandomRange), 0);
            Vector3 ballScreenPoint = Utility.ConvertLocalPositionToScreenPosition(aimPanelTrans, ballLocalPoint + randomBallShakeOffset, uiCamera);
            nowBall.transform.position = GetBallPosIn3D(ballScreenPoint);
        }

        #endregion

        #region 进球太棒了提示
        private Sequence goodPopAniSeq;
        private void ClearGoodPopAni()
        {
            goodPopAniSeq?.Kill();
            goodPopAniSeq = null;
            enterPanel.SetActive(false);
            notEnterPanel.SetActive(false);
            notEnterTipText.color = Color.white;
        }
        [SerializeField] private RectTransform goodPanelTrans;
        [SerializeField] private GameObject enterPanel;
        [SerializeField] private GameObject notEnterPanel;
        [SerializeField] private TMP_Text notEnterTipText = null;
        private void StartGoodPopAni(bool isEnter)
        {
            ClearGoodPopAni();
            enterPanel.SetActive(isEnter);
            notEnterPanel.SetActive(!isEnter);
            if (isEnter == false)
            {
                string tipStr = "";
                if (isStrengthRight == false)
                {
                    if (isUp)
                    {
                        tipStr += "力量大了";
                    }
                    else
                    {
                        tipStr += "力量小了";
                    }
                }
                if (isAngleRight == false)
                {
                    if (string.IsNullOrWhiteSpace(tipStr) == false) tipStr += "，";
                    if (isLeft)
                    {
                        tipStr += "角度偏左";
                    }
                    else
                    {
                        tipStr += "角度偏右";
                    }
                }
                notEnterTipText.text = tipStr;
            }

            goodPanelTrans.localScale = Vector3.one * 5f;
            goodPanelTrans.gameObject.SetAlpha(0);
            goodPopAniSeq = DOTween.Sequence();
            goodPopAniSeq.Append(goodPanelTrans.gameObject.DOFade(1, 0.3f));
            goodPopAniSeq.Join(goodPanelTrans.DOScale(1.9f, 0.3f));
            goodPopAniSeq.Append(goodPanelTrans.DOScale(1.5f, 0.3f));
            goodPopAniSeq.Join(notEnterTipText.DOBlendableColor(Color.red, 0.3f));
            //goodPopAniSeq.AppendCallback(() =>
            //{
            //    AudioManager.Instance.PlaySound(AudioNames.BTN_SWITCH);
            //});
            goodPopAniSeq.Append(goodPanelTrans.DOScale(0.4f, 0.4f).SetEase(Ease.InBack));
            goodPopAniSeq.Join(goodPanelTrans.gameObject.DOFade(0, 0.3f));
            goodPopAniSeq.AppendCallback(() =>
            {
                enterPanel.SetActive(false);
                notEnterPanel.SetActive(false);
            });
        }

        #endregion

        #region 倒计时

        [SerializeField] private RectTransform timePanelTrans;
        private void InitTimePanelPos()
        {
            Vector3 timePointTransLeftScreen = Utility.ConvertWorldPositionToScreenPosition(timePointTransLeft.position, shootCamera);
            Vector3 timePointTransRightScreen = Utility.ConvertWorldPositionToScreenPosition(timePointTransRight.position, shootCamera);
            Vector3 timePointTransLeftLocal = Utility.ConvertScreenPositionToLocalPosition(aimPanelTrans, timePointTransLeftScreen, uiCamera);
            Vector3 timePointTransRightLocal = Utility.ConvertScreenPositionToLocalPosition(aimPanelTrans, timePointTransRightScreen, uiCamera);
            timePanelTrans.SetLocalPositionY(timePointTransLeftLocal.y);
            float scale = (timePointTransRightLocal.x - timePointTransLeftLocal.x) / timePanelTrans.sizeDelta.x;
            timePanelTrans.localScale = Vector3.one * scale;
            //timePanelTrans.SetLocalPositionY(Utility.Lerp(621f, 645f, Utility.GetScreenLerpT()));
        }

        [SerializeField] private ImageFont timeImageFont;
        private int millisecondLeft = 0;
        private void ClearTimeLeftAni()
        {
            isTimeRun = false;
        }
        private void SetTime(int sec)
        {
            millisecondLeft = sec * 1000;
        }
        private void AddTime(int sec)
        {
            millisecondLeft += sec * 1000;
        }
        private void ResetTimeLeft()
        {
            isPopWindowShowing = false;
            millisecondLeft = 0;
            isTimeLeftEnd = false;
            RefreTileLeftLabel(millisecondLeft);
        }
        private void PlayTimeLeftAni()
        {
            if (isPopWindowShowing == true) return;
            if (count321Panel.gameObject.activeSelf == true) return;
            isTimeRun = true;
        }
        private bool isTimeRun = false;
        private void Update()
        {
            if (isTimeRun)
            {
                RefreshTime();
            }
        }
        private void RefreshTime()
        {
            if (millisecondLeft <= 0)
            {
                isTimeRun = false;
                RefreTileLeftLabel(0);
                OnTimeLeftEnd();
                return;
            }
            millisecondLeft -= Mathf.FloorToInt(Time.deltaTime * 1000);
            if (millisecondLeft < 0) millisecondLeft = 0;
            RefreTileLeftLabel(millisecondLeft);
        }
        private void RefreTileLeftLabel(int millisecond)
        {
            timeImageFont.text = TimeUtils.FormatLeftTimeWithMillisecond(millisecond).Replace(':', '：');
        }
        private bool isTimeLeftEnd = false;
        private void OnTimeLeftEnd()
        {
            Debug.Log("OnTimeLeftEnd");
            isTimeRun = false;
            AudioManager.Instance.PlaySound(AudioNames.BATTLE_START_WHISTLE);
            RefreTileLeftLabel(0);
            isTimeLeftEnd = true;
            OnGameEnd();
        }

        #endregion

        #region 等级

        private void ResetStage()
        {
            SetStage(1);
            SetTime(shootGameStageConfig.Second);
        }
        private List<int> ballArray = new();
        ShootGameStageConfig shootGameStageConfig = null;
        [SerializeField] private TMP_Text nowLevelText = null;
        [SerializeField] private TMP_Text targetScoreText = null;
        private void SetStage(int level)
        {
            shootGameStageConfig = Configs.ShootGameStage.GetConfig(level);
            nowLevelText.text = "训练等级：{0}".SafeFormat(shootGameStageConfig.Id);
            targetScoreText.text = "目标得分：<color=#40F569>{0}</color>/{1}".SafeFormat(score, shootGameStageConfig.Point);
            ballArray.Clear();
            EnSureballArray();
            RefreshBallSequence();
        }
        private void EnSureballArray()
        {
            for (int i = 0; i < 4; i++)
            {
                if (ballArray.Count >= 4) break;
                ballArray.AddRange(shootGameStageConfig.ServeBall);
            }
        }
        private void CheckStageUp()
        {
            if (score >= shootGameStageConfig.Point)
            {
                if (Configs.ShootGameStage.GetConfigList()[^1] != shootGameStageConfig)
                {
                    SetStage(shootGameStageConfig.Id + 1);
                    AddTime(shootGameStageConfig.Second);
                    StartLevelUpAni();
                }
            }
        }

        #endregion

        #region 球类型列表面板
        [SerializeField] private List<Image> ball2List = new();
        [SerializeField] private List<Image> ball3List = new();
        private void RefreshBallSequence()
        {
            if (ballArray.Count < 4) EnSureballArray();
            for (int i = 0; i < 4; i++)
            {
                bool isBallIs3 = IsBallIs3(i);
                ball2List[i].gameObject.SetActive(!isBallIs3);
                ball3List[i].gameObject.SetActive(isBallIs3);
            }
        }
        private bool IsNowBallScore3
        {
            get
            {
                return IsBallIs3(0);
            }
        }
        private bool IsBallIs3(int index)
        {
            if (index >= 0 && index < ballArray.Count)
            {
                return ballArray[index] == 3;
            }
            return false;
        }

        private void SetNextBall(bool isEnter)
        {
            ballArray.RemoveAt(0);
            RefreshBallSequence();
        }

        #endregion

        #region 计分板

        private int score = 0;

        private void ResetScore()
        {
            score = 0;
            foreach (ShootScoreItem shootScoreItem in shootScoreItemList)
            {
                shootScoreItem.ResetToZero();
            }
        }
        private void AddScore(int addScore)
        {
            score += addScore;
            RefreshScore();
        }

        [SerializeField] private List<ShootScoreItem> shootScoreItemList = new();
        private void RefreshScore()
        {
            int showScore = Mathf.Min(score, 999);
            shootScoreItemList[0].ChangeToNum(showScore / 100 % 10);
            shootScoreItemList[1].ChangeToNum(showScore / 10 % 10);
            shootScoreItemList[2].ChangeToNum(showScore % 10);
            targetScoreText.text = "目标得分：<color=#40F569>{0}</color>/{1}".SafeFormat(score, shootGameStageConfig.Point);
        }

        #endregion

        #region 倒数321

        private Sequence count321Seq;
        private List<Sequence> countOneSeqList = new();
        [SerializeField] private GameObject count321Panel;
        [SerializeField] private List<Image> countImageList = new();

        private void ClearCount321Ani()
        {
            countIndex = 0;
            foreach (Sequence countOneSeq in countOneSeqList)
            {
                countOneSeq?.Kill();
            }
            countOneSeqList.Clear();
            count321Seq?.Kill();
            count321Panel.SetActive(false);
        }
        private float timeOffsetOneSecond = 1.0f;//1秒
        private float timeOffsetCount321 = 0.8f;//321的间隔
        private void PlayCount321Ani(Action playEndCallBack)
        {
            ClearCount321Ani();
            count321Panel.SetActive(true);
            foreach (Image countImage in countImageList)
            {
                countImage.gameObject.SetActive(false);
            }

            count321Seq = DOTween.Sequence();

            for (int i = 0; i < 3; i++)
            {
                count321Seq.AppendCallback(SetOneCountAni);
                count321Seq.AppendInterval(timeOffsetCount321);
            }
            if (timeOffsetOneSecond > timeOffsetCount321)
            {
                count321Seq.AppendInterval(timeOffsetOneSecond - timeOffsetCount321);
            }
            count321Seq.AppendCallback(() =>
            {
                ClearCount321Ani();
                AudioManager.Instance.PlaySound(AudioNames.BATTLE_START_WHISTLE);
                playEndCallBack?.Invoke();
            });
        }

        private int countIndex = 0;
        private void SetOneCountAni()
        {
            Image countImage = countImageList[countIndex];
            countIndex++;

            Sequence countOneSeq = DOTween.Sequence();
            countOneSeqList.Add(countOneSeq);

            countOneSeq.AppendCallback(() =>
            {
                countImage.gameObject.SetActive(true);
                countImage.SetAlpha(0);
                countImage.transform.localScale = Vector3.one * 15;
                AudioManager.Instance.PlaySound(AudioNames.EVENT_COMMONHIT);
            });
            countOneSeq.Append(countImage.DOFade(1, 0.3f));
            countOneSeq.Join(countImage.transform.DOScale(1.9f, 0.3f));
            countOneSeq.Append(countImage.transform.DOScale(1.5f, 0.3f));
            countOneSeq.AppendCallback(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_SWITCH);
            });
            countOneSeq.Append(countImage.transform.DOScale(0.4f, 0.4f).SetEase(Ease.InBack));
            countOneSeq.Join(countImage.DOFade(0, 0.3f));
            countOneSeq.AppendCallback(() =>
            {
                countImage.gameObject.SetActive(false);
            });
        }
        #endregion

        #region 引导手指

        [SerializeField] private RectTransform fingerPanel = null;
        private Sequence guideFingerSeq;
        private void PlayGuideFingerAni()
        {
            Vector3 startPos = Utility.ConvertLocalPosition(DragArea.transform, Vector3.zero, fingerPanel.parent);
            float endY = Utility.ConvertLocalPosition(circleImage.transform, Vector3.zero, fingerPanel.parent).y;
            fingerPanel.gameObject.SetActive(true);
            guideFingerSeq = DOTween.Sequence();
            guideFingerSeq.AppendCallback(() =>
            {
                fingerPanel.SetLocalPosition(startPos);
                fingerPanel.SetLocalRotationZ(0);
            });
            guideFingerSeq.Append(fingerPanel.DOLocalMoveY(endY, 1.0f));
            guideFingerSeq.Join(fingerPanel.DOLocalRotate(new Vector3(0, 0, 30), 1.0f));
            guideFingerSeq.AppendInterval(0.5f);
            guideFingerSeq.SetLoops(-1);
        }
        private void ClearGuideFingerAni()
        {
            guideFingerSeq?.Kill();
            guideFingerSeq = null;
            fingerPanel.gameObject.SetActive(false);
        }

        #endregion

        #region 阶段升级

        private Sequence levelUpAniSeq;
        private void ClearLevelUpAni()
        {
            levelUpAniSeq?.Kill();
            levelUpAniSeq = null;
            levelUpPanel.gameObject.SetActive(false);
        }

        [SerializeField] private TMP_Text levelUpText = null;
        private void StartLevelUpAni()
        {
            ClearLevelUpAni();
            levelUpPanel.gameObject.SetActive(true);
            levelUpText.text = "加时<color=#40F569><size=30>+{0}s</size></color> 训练阶段提升<color=#40F569><size=30>+1</size></color>".SafeFormat(shootGameStageConfig.Second);

            levelUpPanel.SetLocalPositionY(-75f);
            levelUpPanel.localScale = Vector3.one * 5f;
            levelUpPanel.gameObject.SetAlpha(0);
            levelUpAniSeq = DOTween.Sequence();
            levelUpAniSeq.Append(levelUpPanel.gameObject.DOFade(1, 0.3f));
            levelUpAniSeq.Join(levelUpPanel.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack));
            levelUpAniSeq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.BTN_SWITCH); });
            levelUpAniSeq.AppendInterval(0.5f);
            levelUpAniSeq.Append(levelUpPanel.DOLocalMoveY(0, 0.3f));
            levelUpAniSeq.Join(levelUpPanel.gameObject.DOFade(0, 0.3f));

            levelUpAniSeq.Append(goodPanelTrans.DOScale(0.4f, 0.4f).SetEase(Ease.InBack));
            levelUpAniSeq.Join(goodPanelTrans.gameObject.DOFade(0, 0.3f));
            levelUpAniSeq.AppendCallback(() =>
            {
                enterPanel.SetActive(false);
                notEnterPanel.SetActive(false);
            });
        }

        #endregion

        #endregion

        #region Debug面板

        #region 注册事件

        private void RegDebugEvents()
        {
            stopPlayBtn.OnClick += OnClickStopPlayBtn;
            replayBtn.OnClick += OnClickReplayBtn;
            debugBtn.OnClick += OnClickDebugBtn;
            useNewAnimValueBtn.OnClick += OnClickUseNewAnimValueBtn;
        }
        private void UnRegDebugEvents()
        {
            stopPlayBtn.OnClick -= OnClickStopPlayBtn;
            replayBtn.OnClick -= OnClickReplayBtn;
            debugBtn.OnClick -= OnClickDebugBtn;
            useNewAnimValueBtn.OnClick -= OnClickUseNewAnimValueBtn;
        }


        #endregion

        #region 播放控制
        [SerializeField] private BabuButton stopPlayBtn;
        [SerializeField] private BabuButton replayBtn;
        private void OnClickStopPlayBtn(BabuButton sender)
        {
            Clear();
        }
        private void OnClickReplayBtn(BabuButton sender)
        {
            RestartBattle();
        }

        [SerializeField] private BabuButton useNewAnimValueBtn;
        private void OnClickUseNewAnimValueBtn(BabuButton sender)
        {
            StartBallShakeAni();
            StartPingPongAni();
        }
        #endregion

        #region 面板开关
        [SerializeField] private GameObject debugPad;
        [SerializeField] private BabuButton debugBtn;
        private void OnClickDebugBtn(BabuButton sender)
        {
            debugPad.SetActive(!debugPad.activeSelf);
        }

        #endregion

        #endregion
    }
}