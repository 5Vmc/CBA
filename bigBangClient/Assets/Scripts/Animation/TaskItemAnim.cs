using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;
using UnityTimer;
using BigBang.UI;

namespace BigBang.Animation
{
    public class TaskItemAnim : AnimBase
    {
        [SerializeField] private RectTransform obtainBtn;
        [SerializeField] private Image completedImg;
        [SerializeField] private Image pointImg;
        [SerializeField] private GameObject particle;
        [SerializeField] private RectMask2D maskLayer;

        [SerializeField] private Sprite sprite1; // 不缺角的印章
        [SerializeField] private Sprite sprite2; // 缺角的印章

        private Sequence obtainSequence;
        private Action obtainCallback;
        // 播放领取动画
        public void PlayObtain(Action callback)
        {
            obtainCallback = callback;
            obtainSequence?.Kill();
            maskLayer.enabled = false;
            //TouchManager.Instance.DisableTouch();
            // 设置成不缺角的章
            completedImg.sprite = sprite1;
            // 初始化缩放
            completedImg.rectTransform.localScale = Vector3.one * 2;
            // 初始化透明度
            obtainBtn.gameObject.SetAlpha(1);
            AudioManager.Instance.PlaySound(AudioNames.COLLECT_TASK);
            // 领取按钮淡出
            FlyTxt();
            obtainSequence = DOTween.Sequence();
            obtainSequence.Append(obtainBtn.gameObject.DOFade(0, 0.3f));
            obtainSequence.AppendCallback(() =>
            {
                completedImg.gameObject.SetActive(true);
            });
            obtainSequence.Append(completedImg.rectTransform.DOScale(1, 0.5f).SetEase(Ease.InExpo)); // 完成印章砸入
            obtainSequence.Join(completedImg.DOFade(1, 0.5f));// 完成印章淡入
            obtainSequence.AppendCallback(() =>
            {
                maskLayer.enabled = true;
                // 章盖下去后变成缺角的章
                //completedImg.sprite = sprite2;
                obtainCallback?.Invoke();
                obtainCallback = null;
                //TouchManager.Instance.EnableTouch();
            });
        }
        public void StopPlayObtainAnim()
        {
            obtainSequence?.Kill();
            if (obtainCallback != null)
            {
                obtainCallback?.Invoke();
                obtainCallback = null;
                obtainBtn.gameObject.SetAlpha(0);
                completedImg.rectTransform.SetLocalScale(1);
                completedImg.SetAlpha(1);
                maskLayer.enabled = true;
            }
            //TouchManager.Instance.EnableTouch();
        }

        [EditorButton("活跃点动画")]
        private void FlyTxt()
        {
            var clone = Instantiate(pointImg.gameObject, pointImg.transform.parent);
            var cloneParticle = Instantiate(particle);

            clone.transform.position = pointImg.transform.position;
            clone.transform.SetParent(TaskProgressItem.PointImgPos.parent.parent.parent);
            //clone.transform.parent = TaskProgressItem.PointImgPos.parent.parent.parent;
            clone.transform.SetAsLastSibling();

            cloneParticle.transform.SetParent(clone.transform);

            //cloneParticle.transform.parent = clone.transform;
            cloneParticle.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            cloneParticle.SetActive(true);
            clone.transform.DOMove(TaskProgressItem.PointImgPos.position, 0.3f).SetEase(Ease.Linear);
            // 活跃点缩小
            clone.transform.DOScale(0.8f, 0.3f).OnComplete(() =>
            {
                // 层级设置到TaskUI
                cloneParticle.transform.SetParent(transform.parent.parent.parent.parent, false);
                clone.SetActive(false);
                Destroy(cloneParticle, 3);
                Destroy(clone.gameObject, 3);
            });
        }
    }
}