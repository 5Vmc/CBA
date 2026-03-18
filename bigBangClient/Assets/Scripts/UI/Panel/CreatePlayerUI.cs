using Babu.SDK;
using BigBang.Animation;
using Coffee.UIEffects;
using deVoid.UIFramework;
using DG.Tweening;
using Protocol;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class CreatePlayerUI : APanelController
    {
        [SerializeField] private CreatePlayerUICreateNamePad createNamePad;
        [SerializeField] private CreatePlayerUICreateIconPad createIconPad;
        [SerializeField] private CreatePlayerUISelectClothesPad selectClothesPad;
        [SerializeField] private CreatePlayerUISelectPlayerPad selectPlayerPad;
        [SerializeField] private GameObject confirmPad;
        [SerializeField] private LongPressTrigger confirmBtn;
        [SerializeField] private TMP_Text clubNameTxt;
        [SerializeField] private CardItem cardItem;
        [SerializeField] private BabuButton backBtn;
        [SerializeField] private ClubIconItem clubIconItem;
        [SerializeField] private JerseyItem homeJersey;
        [SerializeField] private JerseyItem awayJersey;
        [SerializeField] private JerseyItem alternativeJersey;
        [SerializeField] private Image pressLight;
        [SerializeField] private Image circle1;
        [SerializeField] private Image circle2;
        [SerializeField] private Image circle3;
        [SerializeField] private RectTransform needlePoint;
        [SerializeField] private List<Image> needles;
        [SerializeField] private UITransitionEffect whitebg;

        [SerializeField] public CreatePlayerUIAnim Anim;

        public static bool IsCreate = false;

        private bool needleAnim = false;
        private AudioSource soundItem;


        // 创建角色请求
        private CreatePlayerRequest request = new CreatePlayerRequest();

        private float triggerTime = 1.8f;
        private bool lockAnim = false;

        protected override void Awake()
        {
            base.Awake();
            // 长按触发时间
            confirmBtn.TriggerTime = triggerTime;
        }

        protected override void AddListeners()
        {
            backBtn.OnClick += OnBack;
            selectPlayerPad.OnFinished += OnFinished;
            confirmBtn.OnLongPress += OnConfirm;
            confirmBtn.OnRelease += OnRelease;
            confirmBtn.OnPressStart += OnPressStart;
        }

        protected override void RemoveListeners()
        {
            backBtn.OnClick -= OnBack;
            selectPlayerPad.OnFinished -= OnFinished;
            confirmBtn.OnLongPress -= OnConfirm;
            confirmBtn.OnRelease -= OnRelease;
            confirmBtn.OnPressStart -= OnPressStart;
            whitebg.effectFactor = 0;
        }

        protected override void OnPropertiesSet()
        {
            GotoStart();
        }
        private void GotoStart()
        {
            createNamePad.gameObject.SetActive(true);
            createIconPad.gameObject.SetActive(false);
            selectClothesPad.gameObject.SetActive(false);
            selectPlayerPad.gameObject.SetActive(false);
            confirmPad.SetActive(false);
            createNamePad.Anim.PlayEnter();
            IsCreate = false;
            whitebg.effectFactor = 0;
        }

        private void OnRelease()
        {
            needleAnim = false;
            confirmBtn.gameObject.DOFade(0.2f, 0.1f);
            pressLight.DOFade(0, 0.1f);
            circle1.DOFade(0, 0.1f);
            circle2.DOFade(0, 0.1f);
            circle3.DOFade(0, 0.1f);
            Anim.StartConfirmBtnAnim();
            AudioManager.Instance.StopSound(soundItem);
        }

        private void OnPressStart()
        {
            Anim.StopConfirmBtnAinm();
            AudioManager.Instance.StopMusic();
        }

        private void Update()
        {
            if (confirmBtn.IsDown)
            {
                confirmBtn.gameObject.SetAlpha(confirmBtn.Progress + 0.2f);
                pressLight.SetAlpha(PressLightAlphaFunc(confirmBtn.Progress));
                circle1.SetAlpha(Circle1AlphaFunc(confirmBtn.Progress));
                circle2.SetAlpha(Circle2AlphaFunc(confirmBtn.Progress));
                circle3.SetAlpha(Circle3AlphaFunc(confirmBtn.Progress));
                if (needleAnim == false)
                {
                    // TODO：改成不赋值给soundItem，可能会有问题
                    //soundItem = AudioManager.Instance.PlaySound(AudioNames.EVENT_BIGBANG);
                    AudioManager.Instance.PlaySound(AudioNames.EVENT_BIGBANG);
                    needleAnim = true;
                    needles.ForEach(item => PlayNeedleAnim(item));
                }
            }
            circle1.rectTransform.Rotate(Vector3.forward, Time.deltaTime * 100 * SpeedUpFunc(confirmBtn.Progress));
            circle2.rectTransform.Rotate(-1 * Vector3.forward, Time.deltaTime * 100 * SpeedUpFunc(confirmBtn.Progress));
            circle3.rectTransform.Rotate(Vector3.forward, Time.deltaTime * 100 * SpeedUpFunc(confirmBtn.Progress));
        }

        private float Circle1AlphaFunc(float rate)
        {
            return Mathf.Clamp01(rate * 6);
        }
        private float Circle2AlphaFunc(float rate)
        {
            if (rate * 3 < 0.5f) return 0;
            if (rate * 3 < 1) return Mathf.Clamp01((rate - 0.5f / 3) * 6);
            return 1;
        }
        private float Circle3AlphaFunc(float rate)
        {
            if (rate * 3 < 1) return 0;
            if (rate * 3 < 1.5f) return Mathf.Clamp01((rate - 1f / 3) * 6);
            return 1;
        }

        private float PressLightAlphaFunc(float rate)
        {
            return Mathf.Clamp01(rate * 2);
        }

        // 加速函数
        // f'(x)=ax+b
        // f(0)=0
        private float SpeedUpFunc(float x)
        {
            float a = 1;
            float b = 10;
            return 1 / (a + 1) * Mathf.Pow(x, a + 1) + b * x;
        }


        public void PlayNeedleAnim(Image needle)
        {
            // 初始化
            needle.rectTransform.localScale = Vector3.one * Random.Range(0.3f, 1f);
            needle.SetAlpha(0);
            needle.rectTransform.DOKill();
            float length = 300;
            var startPos = (Random.Range(0f, 1f) > 0.5f ? 1 : -1) * AngleToVector2(Random.Range(0f, 360f)) * length + needlePoint.anchoredPosition;
            needle.rectTransform.anchoredPosition = startPos;
            needle.rectTransform.up = needle.rectTransform.anchoredPosition - needlePoint.anchoredPosition;
            float animTime = Random.Range(0.5f, 1f);
            float delayTime = Random.Range(0, 0.5f);
            needle.rectTransform.DOAnchorPos(needlePoint.anchoredPosition, animTime).OnStart(() => needle.SetAlpha(1)).SetDelay(delayTime);
            needle.rectTransform.DOScale(0, animTime).SetDelay(delayTime).OnComplete(() =>
            {
                if (needleAnim)
                {
                    PlayNeedleAnim(needle);
                }
            });
        }

        private UnityEngine.Vector2 AngleToVector2(float angle)
        {
            return new UnityEngine.Vector2(1, Mathf.Tan(angle * Mathf.PI / 180)).normalized;
        }

        private void OnBack(BabuButton sender)
        {
            if (lockAnim) return;
            lockAnim = true;
            TouchManager.Instance.DisableTouch();
            Anim.PlayResultBack(() =>
            {
                lockAnim = false;
                TouchManager.Instance.EnableTouch();
                createNamePad.gameObject.SetActive(true);
                createIconPad.gameObject.SetActive(false);
                selectClothesPad.gameObject.SetActive(false);
                selectPlayerPad.gameObject.SetActive(false);
                confirmPad.SetActive(false);
                createNamePad.Anim.PlayEnter();
            });
        }

        public void OnFinished()
        {
            cardItem.SetData(new PlayerCard(selectPlayerPad.starCardId));
            clubIconItem.SetIcon(createIconPad.ClubIcon);
            clubNameTxt.text = createNamePad.ClubName;
            homeJersey.SetIcon(selectClothesPad.HomeValue);
            awayJersey.SetIcon(selectClothesPad.AwayValue);
            alternativeJersey.SetIcon(selectClothesPad.AlternativeValue);
        }

        private void OnConfirm()
        {
            isCreatePlayerSuccess = false;
            TouchManager.Instance.DisableTouch();
            IsCreate = true;
            // 闪白效果
            DOTween.To(value => whitebg.effectFactor = value, 0, 1, 0.5f).OnComplete(() =>
            {
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    // 俱乐部名称
                    request.Name = createNamePad.ClubName;
                    // 俱乐部Icon
                    request.ClubIcon = createIconPad.ClubIcon.Value;
                    // 主场队服
                    request.HomeJersey = selectClothesPad.HomeValue.Value;
                    // 客场队服
                    request.AwayJersey = selectClothesPad.AwayValue.Value;
                    // 客场备选队服
                    request.AlternativeJersey = selectClothesPad.AlternativeValue.Value;
                    // 清空
                    request.InitialSquad.Clear();
                    // 初始阵容
                    request.InitialSquad.AddRange(selectPlayerPad.Result);
                    // 创角
                    NetworkManager.Instance.CreatePlayer(request, OnCreatePlayer);
                });
                SocketBreakTimer = Timer.Register(this.gameObject, 1, AvoidSocketBreak).AddTo(this.gameObject);
            });
        }
        Timer SocketBreakTimer = null;
        private void AvoidSocketBreak()
        {
            if (!isCreatePlayerSuccess)
            {
                DoSilenceReLoginAndCreatePlayer();
            }
        }

        private bool isCreatePlayerSuccess = false;
        private void DoSilenceReLoginAndCreatePlayer()
        {
            if (isCreatePlayerSuccess)
            {
                return;
            }
            Debug.Log("DoSilenceReLoginAndCreatePlayer");
            isCreatePlayerSuccess = false;
            LoginManager.Instance.isDoingSilenceReLoginByCreatePlayer = true;
            LoginManager.Instance.silenceReLoginByCreatePlayerCallback = AfterSilenceReLogin;
            LoginManager.Instance.DoSilenceReLogin();
        }
        public void AfterSilenceReLogin()
        {
            Debug.Log("AfterSilenceReLoginAndNeedCreatePlayer");
            NetworkManager.Instance.CreatePlayer(request, OnCreatePlayer);
        }


        private void OnCreatePlayer(CreatePlayerResponse createPlayerResponse)
        {
            Timer.Cancel(SocketBreakTimer);
            if (createPlayerResponse.Player == null)
            {
                isCreatePlayerSuccess = false;
                TouchManager.Instance.EnableTouch();
                GotoStart();
                Debug.LogWarning("OnCreatePlayer , createPlayerResponse.Player == null");
            }

            TouchManager.Instance.EnableTouch();
            isCreatePlayerSuccess = true;

            BasicPlayerInfoNotify basicPlayerInfoNotify = createPlayerResponse.Player;
            Player.GbId = basicPlayerInfoNotify.Gbid;
            Player.Name = basicPlayerInfoNotify.Name;
            Player.UpLevel = basicPlayerInfoNotify.Level;
            Player.UpStrengh = basicPlayerInfoNotify.Strength;
            Player.UpCreateTime = basicPlayerInfoNotify.CreateTime;

            LoginManager.Instance.EnterGame(createPlayerResponse.Player.Gbid);

            ByteDanceManager.Instance.ReportRegister();
        }
    }
}