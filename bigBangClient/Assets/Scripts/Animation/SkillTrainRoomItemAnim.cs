using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using System;
using UnityTimer;
using TMPro;
using Coffee.UIEffects;
using BigBang.UI;

namespace BigBang.Animation
{
    public class SkillTrainRoomItemAnim : AnimBase
    {
        [CheckNull]
        [Serializable]
        private class Training
        {
            [SerializeField] public TMP_Text PlayerNameTxt;
            [SerializeField] public TMP_Text SkillNameTxt;
            [SerializeField] public RectTransform PlayerRect;
            [SerializeField] public RectTransform SkillRect;
            [SerializeField] public TMP_Text ProgressText;
            [SerializeField] public RectTransform ClearCdBtn;
            [SerializeField] public TMP_Text TitleTxt;
            [SerializeField] public TMP_Text CDTimeText;
            [SerializeField] public RectTransform ProgressImage;
            [SerializeField] public RectTransform Diamond;
            [SerializeField] public Image TrainLight;
        }

        [CheckNull]
        [Serializable]
        private class NoTraining
        {
            [SerializeField] public TMP_Text ProgressTxt;
            [SerializeField] public Image PlayerImg;
            [SerializeField] public Image SkillImg;
            [SerializeField] public RectTransform LearnButton;
        }

        [SerializeField] private Training training;
        [SerializeField] private NoTraining noTraining;
        [SerializeField] private Image lockImg;
        [SerializeField] private Image lightImg;
        [SerializeField] private TMP_Text costTxt;
        [SerializeField] private Image diamondImg;
        [SerializeField] private UIEffect pad;

        private Sequence lockSequence;
        private Sequence lightSequence;
        private Sequence trainingSequence;

        private Sprite lockSprite;

        private void Awake()
        {
            lockSprite = lockImg.sprite;
        }

        private void Start()
        {
            PlayLockAnim();
            PlayLightAnim();
            PlayBreathTrainingProgress();
        }

        public override void Init()
        {
            base.Init();
        }

        public void FadeIn()
        {
            FadeInTraining();
            FadeInNoTraining();
        }

        private void InitTraining()
        {
            training.PlayerRect.localScale = Vector3.one * 0.8f;
            training.SkillRect.localScale = Vector3.one * 0.8f;
            training.TitleTxt.SetAlpha(0);
            training.ProgressText.SetAlpha(0);
            training.CDTimeText.SetAlpha(0);
            training.Diamond.gameObject.SetAlpha(0);
            training.PlayerRect.gameObject.SetAlpha(0);
            training.SkillRect.gameObject.SetAlpha(0);
        }

        [EditorButton("淡入Training")]
        public void FadeInTraining()
        {
            InitTraining();
            training.PlayerRect.gameObject.DOFade(1, 0.1f);
            training.SkillRect.gameObject.DOFade(1, 0.1f);
            training.PlayerRect.DOScale(1, 0.2f);
            training.SkillRect.DOScale(1, 0.2f);
            training.TitleTxt.DOFade(1, 0.3f);
            // 底板亮起
            //DOTween.To(value => pad.colorFactor = value, 0, 0.1f + 0.1f, 0.1f).SetDelay(0.2f).OnComplete(() =>
            //{
            //    DOTween.To(value => pad.colorFactor = value, 0.1f + 0.1f, 0, 0.2f).SetDelay(0.3f);
            //});
            training.TrainLight.DOFade(1, 0.2f + 0.1f).SetDelay(0.1f).OnComplete(() =>
            {
                training.TrainLight.DOFade(0, 0.2f + 0.1f).SetDelay(0.3f);
            });
            Timer.Register(this.gameObject, 0.1f, () =>
            {
                training.ProgressText.DOFade(1, 0.3f);
                Timer.Register(this.gameObject, 0.1f, () =>
                {
                    training.CDTimeText.DOFade(1, 0.3f);
                    Timer.Register(this.gameObject, 0.1f, () =>
                    {
                        training.Diamond.gameObject.DOFade(1, 0.3f);
                    });
                });
            });
        }

        private void InitNoTraining()
        {
            noTraining.LearnButton.gameObject.SetAlpha(0);
            noTraining.ProgressTxt.SetAlpha(0);
            noTraining.PlayerImg.SetAlpha(0);
            noTraining.SkillImg.SetAlpha(0);
        }

        [EditorButton("淡入NoTraining")]
        public void FadeInNoTraining()
        {
            InitNoTraining();
            noTraining.PlayerImg.DOFade(1, 0.3f);
            noTraining.SkillImg.DOFade(1, 0.3f);
            noTraining.ProgressTxt.DOFade(1, 0.3f);
            noTraining.LearnButton.gameObject.DOFade(1, 0.3f);
        }

        [EditorButton("播放锁动画")]
        public void PlayLockAnim()
        {
            lockSequence?.Kill();
            lockSequence = DOTween.Sequence();
            //锁上移
            lockSequence.Insert(0, lockImg.rectTransform.DoRelativeAnchorPosY(1.5f, 0.2f));
            //锁旋转
            //左30度
            lockSequence.Append(lockImg.rectTransform.DORotate(Vector3.forward * -20, 0.3f));//30
            //右30度
            lockSequence.Append(lockImg.rectTransform.DORotate(Vector3.forward * 20, 0.3f));
            //左20度
            lockSequence.Append(lockImg.rectTransform.DORotate(Vector3.forward * -10, 0.3f));//20
            //右10度
            lockSequence.Append(lockImg.rectTransform.DORotate(Vector3.forward * 5, 0.3f));//10
            //归位
            lockSequence.Append(lockImg.rectTransform.DORotate(Vector3.zero, 0.3f));
            //锁下移
            lockSequence.Insert(1.2f, lockImg.rectTransform.DoRelativeAnchorPosY(-1.5f, 0.2f));
            lockSequence.AppendInterval(1f);
            lockSequence.SetLoops(-1);
        }

        public void StopLockAnim()
        {
            lockSequence?.Kill();
            lockSequence = null;
        }

        [EditorButton("加速动画")]
        public void PlaySpeedUp(Action callback)
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_QUICKCD);
            // 钻石淡出
            training.Diamond.gameObject.DOFade(0, 0.1f);
            // 按钮淡出
            training.ClearCdBtn.gameObject.DOFade(0, 0.1f);
            var progressImg = training.ProgressImage.GetComponent<Image>();
            float speed = 1f;
            float animTime = (1 - progressImg.fillAmount) / speed;
            // 进度百分比
            int progress = GetProgress(training.ProgressText.text);
            // 倒计时总秒数
            int totalSecond = GetTotalSecond(training.CDTimeText.text);
            // 涨进度条
            progressImg.DOFillAmount(SkillTrainRoomItem.MaxShowProgress, animTime).SetEase(Ease.Linear);
            // 倒计时
            DOTween.To(value =>
            {
                TimeSpan t = TimeSpan.FromSeconds(value);
                training.CDTimeText.text = t.ToString(@"hh\:mm\:ss");
            }, totalSecond, 0, animTime).SetEase(Ease.Linear);
            // 减数字
            DOTween.To(value => training.ProgressText.text = ((int)value).ToString() + "%", progress, 100, animTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                Timer.Register(this.gameObject, 0.5f, () =>
                {
                    // 文字闪烁
                    training.ProgressText.DOFlash(1, 0.2f, 0.1f, 0.3f).OnComplete(() =>
                    {
                        Timer.Register(this.gameObject, 1, () =>
                        {
                            training.Diamond.gameObject.SetAlpha(1);
                            training.ClearCdBtn.gameObject.SetAlpha(1);
                            callback();
                        });
                    });
                    Timer.Register(this.gameObject, 0.35f, () => AudioManager.Instance.PlaySound(AudioNames.EVENT_CDEND));
                });
            });
        }

        //进度条呼吸动画
        private void PlayBreathTrainingProgress()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AddTo(this.gameObject);
            //sequence?.Kill();
            sequence.Append(training.ProgressImage.gameObject.DOFade(0.7f, 1f)).SetEase(Ease.InOutQuad);
            sequence.SetLoops(-1, LoopType.Yoyo);
        }

        private int GetProgress(string progress)
        {
            if (int.TryParse(progress.TrimEnd('%'), out var result))
            {
                return result;
            }
            return 0;
        }

        private int GetTotalSecond(string time)
        {
            try
            {
                int hour = int.Parse(time.Split(':')[0]);
                int minute = int.Parse(time.Split(':')[1]);
                int second = int.Parse(time.Split(':')[2]);
                return (int)(TimeSpan.FromHours(hour) + TimeSpan.FromMinutes(minute) + TimeSpan.FromSeconds(second)).TotalSeconds;
            }
            catch
            {
                return 0;
            }
        }

        // 解锁动画
        public void PlayUnlockAnim(Action callback)
        {
            lockSequence.Pause();
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.BTN_UNLOCKSTUDY);
            diamondImg.DOFade(0, 0.3f);
            costTxt.DOFade(0, 0.3f);
            lockImg.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            lockImg.rectTransform.DOScale(1.1f, 0.2f).OnComplete(() =>
            {
                var animator = lockImg.GetComponent<Animator>();
                animator.enabled = true;
                animator.Play("Play", 0, 0);
                Timer.Register(this.gameObject, 1f, () =>
                {
                    lockImg.DOFade(0, 0.2f).OnComplete(() =>
                    {
                        lockImg.SetAlpha(1);
                        lockImg.sprite = lockSprite;
                        lockImg.rectTransform.localScale = Vector3.one * 0.9f;
                        TouchManager.Instance.EnableTouch();
                        // 打开循环动画
                        lockSequence.Restart();
                        animator.enabled = false;
                        diamondImg.SetAlpha(1);
                        costTxt.SetAlpha(1);
                        callback?.Invoke();
                        FadeInNoTraining();
                    });
                });
            });
        }

        public void PlayLightAnim()
        {
            lightSequence?.Kill();
            lightSequence = DOTween.Sequence();
            lightSequence.AppendCallback(() =>
            {
                lightImg.fillAmount = 0;
            });
            lightSequence.AppendInterval(1);
            lightSequence.Append(lightImg.DOFillAmount(1, 1));
            lightSequence.SetLoops(-1);
        }
    }
}