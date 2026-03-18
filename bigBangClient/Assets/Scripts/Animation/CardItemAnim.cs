using System;
using System.Collections.Generic;
using BigBang.UI;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using YooAsset;

namespace BigBang.Animation
{
    public class CardItemAnim : MonoBehaviour
    {
        [SerializeField] private StarListItem starList;
        [SerializeField] private RectTransform selfRect;
        [SerializeField] private ParticleSystem cardStar;
        [SerializeField] private Image backImg;
        [SerializeField] private Image colorImg;
        [SerializeField] private Image whiteImg;
        [SerializeField] private Image fadeBackground;
        private UIParticle particle;

        private List<Tween> tweens = new List<Tween>();

        public bool needResetOnDisable = false;
        private void OnDisable()
        {
            ClearAnim();
            if (needResetOnDisable)
            {
                ResetScale();
            }
        }
        public void ResetScale()
        {
            // 初始化缩放
            transform.localScale = Vector3.one * 1.0f;
            // 初始化透明度
            gameObject.SetAlpha(1);
        }

        public void Init()
        {
            ClearAnim();
            // 初始化缩放
            transform.localScale = Vector3.one * 1.1f;
            // 初始化透明度
            gameObject.SetAlpha(0);
            particle?.Stop();
        }

        public void PlayEnter(float delay)
        {
            Init();
            tweens.Add(gameObject.DOFade(1, 0.15f).SetDelay(delay).SetEase(Ease.Linear));
            tweens.Add(selfRect.DOScale(Vector3.one, 0.15f).SetDelay(delay));
        }

        // 播放升星动画
        public void PlayUpgrade()
        {
            starList?.UpgradeAndPlayAnim();
            if (cardStar != null) cardStar?.Play();
        }

        // 播放翻牌动画
        public Tween PlayTurn(float time)
        {
            transform.localRotation = Quaternion.Euler(0, 180, 0);
            return transform.DORotate(new Vector3(0, -180, 0), time, RotateMode.LocalAxisAdd).SetEase(Ease.Linear);
        }

        // 翻牌，翻到中间带回调事件
        public Tween PlayTurn(float time, TweenCallback midcallback)
        {
            Sequence s = DOTween.Sequence();
            s.AppendCallback(() =>
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
            });
            s.Append(transform.DORotate(new Vector3(0, -90, 0), time / 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            s.AppendCallback(midcallback);
            s.Append(transform.DORotate(new Vector3(0, -90, 0), time / 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            return s;
        }

        public Tween PlayReverse(float time, TweenCallback midcallback)
        {
            Sequence s = DOTween.Sequence();
            s.AppendCallback(() =>
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            });
            s.Append(transform.DORotate(new Vector3(0, -90, 0), time / 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            s.AppendCallback(midcallback);
            s.Append(transform.DORotate(new Vector3(0, -90, 0), time / 2f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }));
            return s;
        }

        // 初始化闪光动画
        public void InitLightAnim()
        {
            whiteImg.SetAlpha(0);
            colorImg.SetAlpha(0);
            particle?.Stop();
        }

        public void InitFadeBackground()
        {
            ClearAnim();
            fadeBackground.rectTransform.localScale = Vector3.one * 0.7f;
            fadeBackground.SetAlpha(0);
        }

        // 播放闪光动画
        [EditorButton("播放闪光动画")]
        public Tween PlayLight()
        {
            InitLightAnim();
            colorImg.rectTransform.localScale = new Vector3(1, 0.8f, 1);
            whiteImg.rectTransform.localScale = new Vector3(1, 1, 1);
            // 白光淡入
            whiteImg.DOFade(1, 0.2f).OnComplete(() =>
            {
                // 白光淡出
                whiteImg.DOFade(0, 0.3f).SetDelay(0.1f).OnComplete(() =>
                {
                    whiteImg.rectTransform.localScale = Vector3.one;
                });
            });
            // 彩光拉长
            colorImg.rectTransform.DOScaleY(1.2f, 0.2f).SetDelay(0.3f);
            // 彩光淡入
            colorImg.DOFade(0.8f, 0.2f).SetDelay(0.3f);
            // 白光拉长
            whiteImg.rectTransform.DOScaleY(1.3f, 1f);
            return null;
        }

        public void HidColorImg()
        {
            colorImg.SetAlpha(0);
        }

        public void PlayUIParticle()
        {
            if (particle == null)
            {
#if !UNITY_WEBGL
                LoadUIParticle(() => Timer.Register(this.gameObject, 1, particle.Play));
#else
                 LoadUIParticleAsync(() => Timer.Register(this.gameObject, 1, particle.Play));
#endif
            }
            else
            {
                Timer.Register(this.gameObject, 1, particle.Play);
            }
        }

        void LoadUIParticle(Action callback)
        {
            var h = YooAssets.LoadAssetSync<GameObject>("Prefabs/FX/CardAndDebrisParticle.prefab");
            particle = h.InstantiateSync(gameObject.transform).GetComponent<UIParticle>();
            particle.transform.SetAsFirstSibling();
            callback();
        }

        async void LoadUIParticleAsync(Action callback)
        {
            var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/FX/CardAndDebrisParticle.prefab");
            await h.Task;
            particle = h.InstantiateSync(gameObject.transform).GetComponent<UIParticle>();
            particle.transform.SetAsFirstSibling();
            callback();
        }

        [EditorButton("播放浅色牌淡出放大动画")]
        public void PlayFadeBackground()
        {
            InitFadeBackground();
            tweens.Add(fadeBackground.DOFade(1, 0.1f).OnComplete(() =>
            {
                tweens.Add(fadeBackground.DOFade(0, 0.3f));
            }));
            tweens.Add(fadeBackground.transform.DOScale(1f, 0.4f));
        }

        public void PlayFlash()
        {
            starList.GetNextLevelStar()?.PlayFlash();
            ClearNoLightStar();
        }

        public void ClearNoLightStar()
        {
            foreach (var item in starList.GetNoLightStar())
            {
                item.StopFlash();
                item.HidLightStar();
            }
        }

        private void ClearAnim()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}