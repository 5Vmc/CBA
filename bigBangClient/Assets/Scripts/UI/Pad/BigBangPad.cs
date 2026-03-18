using Babu;
using BigBang.Animation;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class BigBangPad : MonoBehaviour
    {
        [SerializeField] private BigBangPadComponent com;

        [SerializeField] private BigBangPadAnim anim;

        [SerializeField] private BigBangStartAnim animStartBtn;

        [SerializeField] private GameObject modelPrefab;

        private GameObject model;

        private Coroutine coroutine;
        private bool currentCDState = true;
        private bool currentInfoState = true;
        //处理加速按钮和倒计时激活的时机
        private bool flag = true;

        private AudioSource audioSource;

        private bool startBtnEnable = false;
        private bool canBigBang = false;
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            com.StartBtn.onClick.AddListener(OnStartBtn);
            com.ClearCDBtn.onClick.AddListener(OnClearCd);
            EventManager.Instance.Register(EventID.OnBigBangRefresh, OnBigBangRefresh);
            EventManager.Instance.Register(EventID.OnBigBangStart, OnBigBangStart);
            EventManager.Instance.Register(EventID.OnSuperBigBang, OnSuperBigBang);
            EventManager.Instance.Register(EventID.OnBigBangPadPay, OnBigBangPadPay);
            EventManager.Instance.Register(EventID.OnBigBangResultClose, OnBigBangResultClose);
            EventManager.Instance.Register(EventID.OnRemakeBigBangStartButton, OnRemakeBigBangStartButton);
            coroutine = StartCoroutine(StartTick());

            model = GameObject.Instantiate(modelPrefab);
            model.GetComponent<CameraInitializer>().RegistCameraOnce();
            Debug.Log("Create model = " + model.GetHashCode());

            EnableCameraRender();
            audioSource.Play();
            audioSource.loop = true;
            DOTween.To(value => audioSource.volume = value, 0, 1, 3);

            SetData();
            PlayAnim();
        }

        private void OnDisable()
        {
            com.StartBtn.onClick.RemoveListener(OnStartBtn);
            com.ClearCDBtn.onClick.RemoveListener(OnClearCd);
            EventManager.Instance.Unregister(EventID.OnBigBangRefresh, OnBigBangRefresh);
            EventManager.Instance.Unregister(EventID.OnBigBangStart, OnBigBangStart);
            EventManager.Instance.Unregister(EventID.OnSuperBigBang, OnSuperBigBang);
            EventManager.Instance.Unregister(EventID.OnBigBangPadPay, OnBigBangPadPay);
            EventManager.Instance.Unregister(EventID.OnBigBangResultClose, OnBigBangResultClose);
            EventManager.Instance.Unregister(EventID.OnRemakeBigBangStartButton, OnRemakeBigBangStartButton);
            DisableCameraRender();
            audioSource.Stop();
            audioSource.volume = 0;

            Debug.Log("Destroy model = " + model.GetHashCode());
            GameObject.Destroy(model);
            model = null;

            UIController.Instance.CloseWindow<BigBangConfirmUI>();
        }

        // 启用相机渲染
        private void EnableCameraRender()
        {
            // 获得相机
            var c = CameraManager.Instance.GetCamera(CameraID.BigBangPlayerModel);
            Debug.Log("Enable camera = " + c.GetHashCode());
            // 启用相机
            c.gameObject.SetActive(true);
            // 获得临时渲染纹理
            var temporary = RenderTexture.GetTemporary(450, 800, 24);
            temporary.antiAliasing = 8;
            temporary.autoGenerateMips = false;
            temporary.useMipMap = false;
            // 设置训练纹理
            com.PlayerImg.texture = temporary;
            // 设置相机目标渲染纹理
            c.targetTexture = temporary;
        }

        // 禁用相机渲染
        private void DisableCameraRender()
        {
            // 获得相机
            var c = CameraManager.Instance.GetCamera(CameraID.BigBangPlayerModel);
            if (c == null)
            {
                Debug.Log("Disable camera = null");
                RenderTexture.ReleaseTemporary(com.PlayerImg.texture as RenderTexture);
                return;
            }
            Debug.Log("Disable camera = " + c.GetHashCode());
            // 设置相机目标渲染纹理为空
            c.targetTexture = null;
            // 禁用相机
            c.gameObject.SetActive(false);
            // 释放临时渲染纹理
            RenderTexture.ReleaseTemporary(com.PlayerImg.texture as RenderTexture);
            // 设置训练纹理为空
            com.PlayerImg.texture = null;
        }

        private void OnStartBtn()
        {
            if (!canBigBang)
            {
                var needExp = Player.TrainManager.BigBangController.BigBangNeedTotalExp() - Player.TrainManager.TotalExp;
                Tips.PopTips("还需要" + needExp.ToFormatString() + "经验才能开启超能训练");
            }
            else
            {
                com.StartBtn.enabled = false;
                AudioManager.Instance.PlaySound(AudioNames.BTN_ACTBIGBANG);
                //点击开启按钮 按钮动画
                animStartBtn.StartBtnAnim(() =>
                {
                    com.StartBtn.enabled = true;
                    UIController.Instance.OpenWindow<BigBangConfirmUI>();
                });
            }
        }

        private void OnBigBangStart(object args)
        {
            TouchManager.Instance.DisableTouch();
            flag = false;
            AudioManager.Instance.PlaySound(AudioNames.EVENT_BIGBANG);
            //开启超训动画
            anim.PlayBigBangAnim(() =>
            {
                TouchManager.Instance.EnableTouch();
                Player.TrainManager.BigBangController.DoBigBang(false);
                //Babu.EventManager.Instance.Dispatch(EventID.OnRemakeBigBangStartButton);
            });
        }

        [EditorButton("播放爆炸动画")]
        public void PlayBigBang()
        {
            anim.PlayBigBangAnim(null);
        }

        private void OnSuperBigBang(object[] args)
        {
            TouchManager.Instance.DisableTouch();
            flag = false;
            AudioManager.Instance.PlaySound(AudioNames.EVENT_BIGBANG);
            //开启超训动画
            anim.PlayBigBangAnim(() =>
            {
                TouchManager.Instance.EnableTouch();
                Player.TrainManager.BigBangController.DoBigBang(true);
                // Babu.EventManager.Instance.Dispatch(EventID.OnRemakeBigBangStartButton);
            });
        }

        private void OnBigBangPadPay(object[] args)
        {
            if (Player.TrainManager.BigBangController.CanClearBigBangCD())
            {
                com.ClearCDBtn.gameObject.SetActive(false);
                var cdTime = Player.TrainManager.BigBangController.BigBangCDSecond();
                AudioManager.Instance.PlaySound(AudioNames.ANI_QUICKCD);
                TouchManager.Instance.DisableTouch();
                DOTween.To(value => com.CDText.text = TimeSpan.FromSeconds((int)(value)).ToString(), cdTime, 0, 1.5f).OnComplete(() =>
                {
                    TouchManager.Instance.EnableTouch();
                    //倒计时为0面板动画

                    Player.TrainManager.BigBangController.DoClearBigBangCd();
                });
            }
            else
            {
                Tips.PopError(ErrorID.DiamondNotEnough);
            }
        }

        //加速按钮
        private void OnClearCd()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CDACC);
            if (Player.TrainManager.BigBangController.GetClearBigBangCDDiamond() <= 0)
            {
                Player.TrainManager.BigBangController.DoClearBigBangCd();
            }
            else
            {
                UIController.Instance.OpenWindow<BigBangPayUI>();
            }
        }

        public void SetData()
        {
            UpdateInfo();
        }

        public void PlayAnim()
        {
            anim.Play();
        }

        //持续更新
        private void UpdateInfo()
        {
            com.ForceText.text = Player.TrainManager.Force.ToFormatString();
            com.AdditonText.text = $"X {Player.TrainManager.GetIncomeForceAdd().ToFormatString()}";
            canBigBang = Player.TrainManager.BigBangController.CanBigBang();

            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/BigBang");
            if (canBigBang)
            {
                node.AddValue(1);
                if (!startBtnEnable) EventManager.Instance.Dispatch(EventID.RefreshUIRedDot, 2);
                startBtnEnable = true;
                com.StartBtnText.text = Lang.Get(LangID.EnableText);
            }
            else
            {
                node.AddValue(-1);
                if (startBtnEnable) EventManager.Instance.Dispatch(EventID.RefreshUIRedDot, 2);
                startBtnEnable = false;
                com.StartBtnText.text = "累积经验中...";
            }

            UpdateCDInfo();
            UpdateReadyInfo();
        }

        //在有CD和没CD切换的时候调用的事件
        private void OnCDChanged()
        {
            var isCdOver = Player.TrainManager.BigBangController.IsBigBangCdOver();
            //如果倒计时结束
            if (isCdOver)   //显示开启按钮
            {
                com.CDText.text = TimeSpan.FromSeconds(0).ToString();
                //TouchManager.Instance.DisableTouch();
                //播放倒计时结束动画
                anim.PlayCDOverAnim(() =>
                {
                    //TouchManager.Instance.EnableTouch();
                    //进度条
                    com.Progress.SetActive(false);
                    com.CdPad.gameObject.SetActive(false);
                    com.ClearCDBtn.gameObject.SetActive(false);
                    //加速按钮
                    com.StartBtn.gameObject.SetActive(true);
                    com.StartBtn.enabled = true;
                    //正在启动BIG BANG 设置不可见
                    com.StartingText.SetAlpha(0);
                    //开启
                    com.StartBtnText.SetAlpha(1);
                    com.StartBtn.transform.localScale = new Vector3(1, 1, 1);
                    anim.PlayStartIdle();
                });
            }
            else    //显示倒计时
            {
                if (!flag) return;
                anim.PlayStartIdle();
                com.ClearCDBtn.gameObject.SetAlpha(1);
                com.CdPad.gameObject.SetActive(true);
                com.ClearCDBtn.gameObject.SetActive(true);
                //TouchManager.Instance.DisableTouch();
                //加速按钮
                com.StartBtn.gameObject.SetActive(false);
                anim.PlayCdStartAnim(() =>
                {
                    //TouchManager.Instance.EnableTouch();
                });
            }
        }

        //准备和没有准备切换的时候播放动画
        private void OnReadyChanged()
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_BBBOARDREF);
            anim.PlayInfoText();
        }

        private void OnBigBangResultClose(object[] args)
        {
            flag = true;
            anim.PlayStartIdle();
            com.ClearCDBtn.gameObject.SetAlpha(1);
            com.CdPad.gameObject.SetActive(true);
            com.ClearCDBtn.gameObject.SetActive(true);
            TouchManager.Instance.DisableTouch();
            //加速按钮
            com.StartBtn.gameObject.SetActive(false);
            anim.PlayCdStartAnim(() =>
            {
                TouchManager.Instance.EnableTouch();
            });
        }

        private void UpdateReadyInfo()
        {
            var isReady = Player.TrainManager.BigBangController.IsBigBangExpReady();
            com.Ready.SetActive(isReady);
            com.NotReady.SetActive(!isReady);
            if (currentInfoState != isReady)
            {
                currentInfoState = isReady;
                OnReadyChanged();
            }
            if (isReady)
            {
                SetReadyText();
            }
            else
            {
                SetNotReadyText();
            }
        }

        private void UpdateCDInfo()
        {
            var isCdOver = Player.TrainManager.BigBangController.IsBigBangCdOver();
            //可使用状态机控制
            if (currentCDState != isCdOver)
            {
                currentCDState = isCdOver;
                OnCDChanged();
            }
            if (!isCdOver)
            {
                SetCDText();
            }
        }

        private IEnumerator StartTick()
        {
            while (true)
            {
                UpdateInfo();
                yield return new WaitForSeconds(1);
            }
        }

        private void SetNotReadyText()
        {
            com.LineText.text = Lang.Get(LangID.NextBigBangText);
            var needExp = Player.TrainManager.BigBangController.BigBangNeedTotalExp() - Player.TrainManager.TotalExp;
            com.NeedTotalExpText.text = needExp.ToFormatString();
            var incomePerSecond = Player.TrainManager.IncomePerSecond();
            if (incomePerSecond == 0)
            {
                com.NeedTimeText.text = Player.TrainManager.GetInComeShowString();
            }
            else
            {
                if (needExp < 0) needExp = 0;
                var needTime = needExp / incomePerSecond;
                if (needTime > TimeUtils.Day)
                {
                    //大于24h
                    com.TimeTitle.text = Lang.Get(LangID.ExpOutputText);
                    com.NeedTimeText.text = Player.TrainManager.GetInComeShowString();
                }
                else
                {
                    com.TimeTitle.text = Lang.Get(LangID.ResidueTimeText);
                    com.NeedTimeText.text = TimeUtils.GetTimeString((long)needTime.ToDouble());
                }
            }
        }

        private void SetReadyText()
        {
            com.LineText.text = Lang.Get(LangID.ReadyText);
            com.CanGetForceText.text = Player.TrainManager.BigBangController.GetGiveForce(false).ToFormatString();
            //com.CanGetForceAddText.text = Player.TrainManager.BigBangController.GetIncomeForceAddAfterBigBang().ToFormatString();
            com.CanGetForceAddText.text = Lang.Get(LangID.MakeAdditionTxt).Replace("{value}", (Player.TrainManager.BigBangController.GetIncomeForceAddAfterBigBang() / Player.TrainManager.GetIncomeForceAdd()).ToFormatStrengthString());
        }

        private void SetCDText()
        {
            var cdTime = Player.TrainManager.BigBangController.BigBangCDSecond();
            com.CDText.text = TimeUtils.GetTimeString(cdTime);
            com.ClearBigBangCDDiamonText.text = Player.TrainManager.BigBangController.GetClearBigBangCDDiamond().ToString();
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    StartCoroutine(StartTick());
                }
            }
        }

        public void Stop()
        {
            StopAllCoroutines();
        }

        private void OnBigBangRefresh(object[] args)
        {
            UpdateInfo();
            Player.TrainManager.BigBangController.CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshBigBangUIRedDot);
        }

        //恢复开始按钮
        private void OnRemakeBigBangStartButton(object[] args)
        {
            //进度条
            com.Progress.SetActive(false);
            com.CdPad.gameObject.SetActive(false);
            com.ClearCDBtn.gameObject.SetActive(false);
            //加速按钮
            com.StartBtn.gameObject.SetActive(true);
            com.StartBtn.enabled = true;
            //正在启动BIG BANG 设置不可见
            com.StartingText.SetAlpha(0);
            //开启
            com.StartBtnText.SetAlpha(1);
            com.StartBtn.transform.localScale = new Vector3(1, 1, 1);
            anim.PlayStartIdle();
        }
    }
}