using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using BigBang.UI;

namespace BigBang.Animation
{
    public class RegularTrainItemUnlockAnim : MonoBehaviour
    {
        [SerializeField] private RegularTrainItemComponent com;

        private Sequence enoughAnim;

        private PlayerTrainItem item = null;

        private bool isPlaying = false;

        public void Awake()
        {
            
        }

        private void OnEnable()
        {
            Babu.EventManager.Instance.Register(EventID.OnExpChanged, OnExpChanged);
        }

        private void OnDisable()
        {
            Babu.EventManager.Instance.Unregister(EventID.OnExpChanged, OnExpChanged);
        }

        private bool isInitCanLockPlay = false;
        //抖动动画
        public void InitCanLockPlay()
        {
            if (isInitCanLockPlay == true) return;
            isInitCanLockPlay = true;

            enoughAnim = DOTween.Sequence();
            item = GetComponent<RegularTrainItem>().Item;
            //锁上移
            enoughAnim.Insert(0, com.LockImg.rectTransform.DoRelativeAnchorPosY(1.5f, 0.1f));
            //锁旋转
            //左30度
            enoughAnim.Append(com.LockImg.rectTransform.DORotate(Vector3.forward * -30, 0.15f));
            //右30度
            enoughAnim.Append(com.LockImg.rectTransform.DORotate(Vector3.forward * 30, 0.15f));
            //左20度
            enoughAnim.Append(com.LockImg.rectTransform.DORotate(Vector3.forward * -20, 0.15f));
            //右10度
            enoughAnim.Append(com.LockImg.rectTransform.DORotate(Vector3.forward * 10, 0.15f));
            //归位
            enoughAnim.Append(com.LockImg.rectTransform.DORotate(Vector3.zero, 0.15f));
            //锁下移
            enoughAnim.Insert(0.6f, com.LockImg.rectTransform.DoRelativeAnchorPosY(-1.5f, 0.1f));
            enoughAnim.AppendInterval(1f);
            enoughAnim.SetLoops(-1);
            enoughAnim.Pause();
            enoughAnim.AddTo(this.gameObject);
        }

        //解锁动画
        public void PlayUnlock(Action callback = null)
        {
            if (isPlaying) return;

            isPlaying = true;
            //暂停抖动动画
            com.LockImg.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
            enoughAnim.Pause();
            var rectTransform = GetComponent<RectTransform>();
            com.LockImg.DOFade(0, 0.3f).AddTo(this.gameObject); ;
            com.FlashImg.DOFade(1, 2).OnComplete(() => com.FlashImg.DOFade(0, 0.5f)).AddTo(this.gameObject); ;
            com.LockImg.rectTransform.DOScale(1.1f, 2).AddTo(this.gameObject); ;
            //黑色背景淡出
            com.BlackBackground.DOFade(0, 0.3f).AddTo(this.gameObject); ;
            com.UnlockBG.DOFade(0, 0.3f).AddTo(this.gameObject); ;
            //钻石图片淡出
            com.DiamondImg.DOFade(0, 0.3f).AddTo(this.gameObject); ;
            //文本淡出
            com.CostText.DOFade(0, 0.3f).AddTo(this.gameObject); ;
            //item震动
            rectTransform.DOShakeAnchorPos(1, 25, 40).AddTo(this.gameObject); ;
            //曝闪
            DOTween.To(value => com.Effect.colorFactor = value, 0, 1, 1.2f).OnComplete(() =>
            {
                callback?.Invoke();
                DOTween.To(value => com.Effect.colorFactor = value, 1, 0, 1).OnComplete(() =>
                {
                    isPlaying = false;
                }).AddTo(this.gameObject); ;
                com.TrainImg.gameObject.SetActive(true);
                com.Effect.GetComponent<Image>().DOFade(0, 0.5f).AddTo(this.gameObject); ;
                //外轮廓变白
                DOTween.To(value => com.OutlineEffect.colorFactor = value, 0, 1, 1).AddTo(this.gameObject); ;
            }).AddTo(this.gameObject); ;
        }

        public void OnExpChanged(object[] args)
        {
            if (item is null) return;

            if (Player.TrainManager.CanUpgrade(item.ConfigId))
            {
                if (!enoughAnim.IsPlaying())
                {
                    //锁放大
                    com.LockImg.rectTransform.DOScale(0.7f, 0.5f).AddTo(this.gameObject); ;
                    //圆圈缩小
                    com.Background.rectTransform.DOScale(0, 0.5f).AddTo(this.gameObject); ;
                    enoughAnim.Play();
                }
            }
            else
            {
                com.LockImg.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
                enoughAnim.Pause();
                //锁缩小
                com.LockImg.rectTransform.DOScale(0.5f, 0.5f).AddTo(this.gameObject); ;
                //圆圈缩小
                com.Background.rectTransform.DOScale(1, 0.5f).AddTo(this.gameObject); ;
            }
        }
    }
}