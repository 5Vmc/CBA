using UnityEngine;
using DG.Tweening;
using BigBang.UI;
using Utils;
using System;
using UnityTimer;

namespace BigBang.Animation
{
    public class BigBangPadAnim : MonoBehaviour
    {
        [SerializeField] private BigBangPadComponent com;
        [SerializeField] private BigBangInfoPadAnim infoAnim;
        [SerializeField] private BigBangCDAnim cdAnim;
        [SerializeField] private BigBangStartAnim startAnim;

        private Material playerMaterial;

        private void Init()
        {
            com.TopDomain.gameObject.SetAlpha(0);
            com.BackgroundGraphic.AnimationState.SetEmptyAnimations(0);
            com.BackgroundGraphic.color = new Color(1, 1, 1, 1);
            playerMaterial = GameObjectManager.Instance.GetGameObject(GameObjectID.BigBangPlayerModel).GetComponent<SkinnedMeshRenderer>().material;
            playerMaterial.SetFloat("_dissolve_filter", 1);
            com.StartBtn.gameObject.SetAlpha(0);
            com.StartBtn.GetComponent<RectTransform>().localScale = Vector3.zero;
        }

        public void Play()
        {
            Init();
            // 背景动画
            int count = 1;
            var backgroundAnim = com.BackgroundGraphic.AnimationState.SetAnimation(0, "wu-xunhuan", false);
            backgroundAnim.MixDuration = 0;
            backgroundAnim.Complete += (arg) =>
            {
                foreach (var item in com.BackgroundGraphic.SkeletonData.Animations)
                {
                    if (item.Name.GetHashCode() != "wu-xunhuan".GetHashCode())
                    {
                        com.BackgroundGraphic.AnimationState.SetAnimation(count++, item.Name, true).TimeScale = 0.1f;
                    }
                    if (item.Name.GetHashCode() == "wenzi2".GetHashCode())
                    {
                        com.BackgroundGraphic.AnimationState.SetAnimation(count++, item.Name, true);
                    }
                }
            };
            // 人物溶解动画
            DOTween.To(value => playerMaterial.SetFloat("_dissolve_filter", value), 1, 0, 0.7f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Timer.Register(this.gameObject, 0.6f, () =>
                {
                    startAnim.Play();
                    com.StartBtn.gameObject.DOFade(1, 0.3f);
                    com.StartBtn.GetComponent<RectTransform>().DOScale(1, 0.2f);
                    AudioManager.Instance.PlaySound(AudioNames.ANI_BBBTNPOP);
                    // 高亮扫光
                    DOTween.To(value => playerMaterial.SetFloat("_light_rate", value), 1, 5, 0.3f);
                    DOTween.To(value => playerMaterial.SetFloat("_light_rate", value), 5, 1, 0.3f).SetDelay(2);
                });
            });
            // 顶部栏下移
            com.TopDomain.gameObject.DOFade(1, 0.3f).SetDelay(0.5f);
            infoAnim.Play();
            cdAnim.Play();
            // startAnim.Play();
        }

        public void PlayCDOverAnim(Action callback)
        {
            cdAnim.PlayOver(callback);
        }

        public void PlayStartIdle()
        {
            startAnim.Play();
        }

        public void PlayCdStartAnim(Action callback)
        {
            cdAnim.Play(callback);
        }

        public void PlayBigBangAnim(Action callback)
        {
            startAnim.PlayStart(callback);
        }

        public void PlayInfoText()
        {
            infoAnim.PlayText();
        }
    }
}
