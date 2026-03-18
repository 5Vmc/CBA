using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using Coffee.UIEffects;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class SkillIconAnim : AnimBase
    {
        [SerializeField] private Image borderImg;
        [SerializeField] private Image lockbg;
        [SerializeField] private List<UIShiny> shiny;
        [SerializeField] private Image lockImg;
        [SerializeField] private Image qualityImg;
        [SerializeField] private Image skillImg;
        [SerializeField] private RectTransform lightBorder;
        [SerializeField] private UIEffect uIEffect;


        public override void Init()
        {
            base.Init();
            // 初始缩放
            transform.localScale = Vector3.one * 1.1f;
            // 初始化透明度
            gameObject.SetAlpha(0);
        }

        public void PlayEnter(float delay)
        {
            ClearAnim();
            Init();
            tweens.Add(gameObject.DOFade(1, 0.3f).SetDelay(delay));
            tweens.Add(transform.DOScale(1, 0.3f).SetDelay(delay));
        }

        // 以漩涡旋转的方式变亮
        [EditorButton("以漩涡旋转的方式变亮")]
        public void PlayLightUp()
        {
            lockbg.gameObject.SetActive(true);
            var effect = lockbg.GetComponent<UIDissolve>();
            DOTween.To(value => effect.effectFactor = value, 0, 1, 0.3f).OnComplete(() =>
            {
                lockbg.gameObject.SetActive(false);
            });
        }

        [EditorButton("边框扫光")]
        public void PlayLightBorder()
        {
            shiny.ForEach(item => item.Play());
        }

        [EditorButton("播放解锁动画")]
        public void PlayUnlockAnim()
        {
            TouchManager.Instance.DisableTouch();
            // 放大
            lockbg.rectTransform.DOScale(1.05f, 0.1f);
            qualityImg.rectTransform.DOScale(1.05f, 0.1f);
            skillImg.rectTransform.DOScale(1.05f, 0.1f);
            lightBorder.DOScale(1.05f, 0.1f).OnComplete(() =>
            {
                // 锁icon	向上一点然后掉落
                lockImg.GetComponent<Animator>().Play("Play", 0, 0);
                // 锁的水平位移
                lockImg.rectTransform.DoRelativeAnchorPosX(-5, 0.6f).SetDelay(0.3f);
                // 锁向上移动
                lockImg.rectTransform.DoRelativeAnchorPosY(10, 0.3f).SetDelay(0.3f).OnComplete(() =>
                {
                    // 锁淡出
                    lockImg.DOFade(0, 0.3f);
                    // 锁向下移动
                    lockImg.rectTransform.DoRelativeAnchorPosY(-10, 0.3f).OnComplete(() =>
                    {
                        PlayLightUp();
                        PlayLightBorder();
                        // 变亮
                        DOTween.To(value => uIEffect.colorFactor = value, 0, 1, 0.3f).OnComplete(() =>
                        {
                            DOTween.To(value => uIEffect.colorFactor = value, 1, 0, 0.3f).OnComplete(() =>
                            {
                                // 恢复大小
                                lockbg.rectTransform.DOScale(1, 0.1f);
                                qualityImg.rectTransform.DOScale(1, 0.1f);
                                skillImg.rectTransform.DOScale(1, 0.1f);
                                lightBorder.DOScale(1, 0.1f);
                                TouchManager.Instance.EnableTouch();
                            });
                        });
                    });
                });
            });
        }

        // 显示边框
        public void ShowBorder()
        {
            TouchManager.Instance.DisableTouch();
            borderImg.DOFade(1, 0.2f).OnComplete(() =>
            {
                TouchManager.Instance.EnableTouch();
            });
        }

        // 隐藏选中边框
        public void HidBorder()
        {
            borderImg.SetAlpha(0);
        }


        private Sequence enoughAnim = null;
        public void CanUnlockTipAnim()
        {
            if(enoughAnim == null){
                enoughAnim = DOTween.Sequence();
                //锁上移
                enoughAnim.Insert(0, lockImg.rectTransform.DoRelativeAnchorPosY(1.5f, 0.1f));
                //锁旋转
                //左30度
                enoughAnim.Append(lockImg.rectTransform.DORotate(Vector3.forward * -30, 0.15f));
                //右30度
                enoughAnim.Append(lockImg.rectTransform.DORotate(Vector3.forward * 30, 0.15f));
                //左20度
                enoughAnim.Append(lockImg.rectTransform.DORotate(Vector3.forward * -20, 0.15f));
                //右10度
                enoughAnim.Append(lockImg.rectTransform.DORotate(Vector3.forward * 10, 0.15f));
                //归位
                enoughAnim.Append(lockImg.rectTransform.DORotate(Vector3.zero, 0.15f));
                //锁下移
                enoughAnim.Insert(0.6f, lockImg.rectTransform.DoRelativeAnchorPosY(-1.5f, 0.1f));
                enoughAnim.AppendInterval(1f);
                enoughAnim.SetLoops(-1);
               // enoughAnim.Pause();
            }
        }
    }
}