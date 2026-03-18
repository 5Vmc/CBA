using System;
using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using Utils;
using UnityEngine.UI;
using Coffee.UIEffects;
using UnityTimer;

namespace BigBang.Animation
{
    /// <summary>
    /// 超能训练启动动画
    /// </summary>
    public class BigBangStartAnim : MonoBehaviour
    {
        [SerializeField] private BigBangPadComponent com;

        private List<Tween> tweens = new List<Tween>();

        public void Play()
        {
            Kill();

            var inner1 = com.Inner1.GetComponent<RectTransform>();
            var outter1 = com.Outter1.GetComponent<RectTransform>();
            var outter2 = com.Outter2.GetComponent<RectTransform>();
            var inner1Img = com.Inner1.GetComponent<Image>();
            outter1.localScale = Vector3.zero;
            outter2.localScale = Vector3.zero;
            com.StartBtnText.SetAlpha(0);
            com.BallImg.SetAlpha(0);
            com.StartBackground.SetAlpha(0);
            inner1Img.SetAlpha(0);

            // 内圈闪烁
            tweens.Add(com.BallImg.DOFlash(2, 0.03f, 0.03f, 0.03f));
            tweens.Add(inner1Img.DOFlash(2, 0.03f, 0.03f, 0.03f));

            // 显示文字
            tweens.Add(com.StartBtnText.DOFade(1, 0.2f).SetDelay(0.2f).OnComplete(() =>
            {
                tweens.Add(com.StartBtnText.DOBreath(1, 1.1f, 1, 0.2f).SetLoops(-1));
            }));
            // 背景淡入
            tweens.Add(com.StartBackground.DOFade(1, 0.5f));
            // 外圈放大
            tweens.Add(outter1.DOScale(1, 0.7f));
            tweens.Add(outter2.DOScale(1, 0.7f));
            AudioManager.Instance.PlaySound(AudioNames.ANI_BBBTNPOP);
            // 内圈旋转
            com.Inner1.angularVelocity = 45f;
            // 外圈旋转
            com.Outter1.angularVelocity = -45f;
            com.Outter2.angularVelocity = -45f;
        }

        [EditorButton("播放充能动作")]
        private void PlayChargeAnim()
        {
            var pos = CameraManager.Instance.GetCamera(CameraID.BigBangPlayerModel).transform;
            // 人物上下移动
            Sequence s = DOTween.Sequence();
            s.Append(pos.DORelativePositionY(-0.05f, 0.6f));
            s.Append(pos.DORelativePositionY(0, 0.6f));
            for (int i = 0; i < 2; i++)
            {
                s.Append(pos.DORelativePositionY(-0.03f, 0.5f));
                s.Append(pos.DORelativePositionY(0, 0.5f));
            }
            GameObjectManager.Instance.GetGameObject(GameObjectID.BigBangPlayerModel).GetComponentInParent<Animator>().Play("Charge");
        }

        public void PlayStart(Action callback)
        {
            Kill();
            // 初始化
            com.StartBtnText.SetAlpha(1);
            com.StartingText.SetAlpha(0);
            com.ProgressValue.fillAmount = 0;
            com.Progress.SetAlpha(1);
            Babu.DelayTaskService.Instance.Run(this.gameObject, 1, PlayChargeAnim);
            // 内圈旋转
            DOTween.To(value => com.Inner1.angularVelocity = value, 45f, 360 * 2, 1f).SetEase(Ease.InSine);
            // 外圈旋转
            DOTween.To(value => com.Outter1.angularVelocity = value, -45, -360 * 2, 1f).SetEase(Ease.InSine);
            DOTween.To(value => com.Outter2.angularVelocity = value, -45, -360 * 2, 1f).SetEase(Ease.InSine);
            // Spine动画加速
            BackgroundAnimSpeedUp();

            // 开启 淡出
            com.StartBtnText.SetAlpha(0);
            com.StartingText.SetAlpha(1);
            // 启动BIG BANG淡入
            com.Progress.SetActive(true);
            // 蓝光呼吸
            Sequence blueLightSequence = DOTween.Sequence();
            for (int i = 0; i < 4; i++)
            {
                blueLightSequence.Append(DOTween.To(value => com.LightBorder.SetAlpha(value), 0, 0.6f, 0.4f));
                blueLightSequence.Append(DOTween.To(value => com.LightBorder.SetAlpha(value), 0.6f, 0, 0.4f));
            }
            // 进度条填满
            tweens.Add(com.ProgressValue.DOFillAmount(1, 1.2f).OnComplete(() =>
            {
                // 进度条闪烁
                tweens.Add(com.Progress.DOFlash(3, 0.1f, 0.1f, 0.2f, 0.2f, 1).OnComplete(() =>
                {
                    // 爆炸粒子特效
                    Babu.DelayTaskService.Instance.Run(this.gameObject, 0.15f, com.Explosion.Play);
                }));
                // 文字闪烁
                tweens.Add(com.StartingText.DOFlash(3, 0.1f, 0.1f, 0.2f, 0.2f, 1));
                Timer.Register(this.gameObject, 0.1f, () => com.WhiteBlackGround.SetAlpha(1));
                // 白屏
                tweens.Add(DOTween.To(value => com.WhiteBlackGround.GetComponent<UITransitionEffect>().effectFactor = value, 0, 1, 1.5f).SetEase(Ease.InQuint).SetDelay(0.1f).OnComplete(() =>
                {
                    // Spine动画恢复速度
                    RecoverAnimSpeed();
                    // 加速按钮
                    com.StartBtn.gameObject.SetActive(false);
                    tweens.Add(com.WhiteBlackGround.DOFade(0, 0.5f).SetDelay(0.5f));
                    Babu.DelayTaskService.Instance.Run(this.gameObject, 0.5f, () => callback?.Invoke());
                    GameObjectManager.Instance.GetGameObject(GameObjectID.BigBangPlayerModel).GetComponentInParent<Animator>().Play("Idle");
                }));
            }));
        }

        //开启按钮动画
        public void StartBtnAnim(Action callback)
        {
            Kill();
            tweens.Add(com.StartBtnText.GetComponent<IllusionAnim>().Play(2.5f, 0, 0.5f));
            tweens.Add(com.StartBtnText.DOFade(0, 0.3f).SetDelay(0.1f));
            tweens.Add(com.StartBtn.transform.DOScale(new Vector3(0, 0, 0), 0.3f).SetDelay(0.2f).OnComplete(() =>
            {
                //打开栏目
                callback?.Invoke();
            }));

        }

        [EditorButton("Spine动画加速")]
        public void BackgroundAnimSpeedUp()
        {
            DOTween.To(value => com.BackgroundGraphic.AnimationState.TimeScale = value, 1, 20, 1.5f);
        }

        [EditorButton("Spine恢复速度")]
        public void RecoverAnimSpeed()
        {
            com.BackgroundGraphic.AnimationState.TimeScale = 1;
        }



        private void Kill()
        {
            com.WhiteBlackGround.SetAlpha(0);
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }

        private void OnDisable()
        {
            Kill();
        }
    }
}