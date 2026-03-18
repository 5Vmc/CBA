using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityTimer;
using Utils;
using TMPro;
using BigBang.UI;
using System.Collections.Generic;
using Coffee.UIEffects;

namespace BigBang.Animation
{
    public class TaskProgressItemAnim : AnimBase
    {
        [SerializeField] private Image progressValue;
        [SerializeField] private TMP_Text pointTxt;
        [SerializeField] private List<BabuButton> rewardList;
        [SerializeField] private Image icon;
        [SerializeField] private Image icon2;

        [SerializeField] private List<UIShiny> shinys;

        public override void Init()
        {
            base.Init();
            progressValue.fillAmount = 0;
            pointTxt.text = "0";
            gameObject.SetAlpha(0);
            rewardList.ForEach(item => item.gameObject.SetAlpha(0));
        }

        public void PlayAnim(int point)
        {
            // 初始化动画
            Init();
            tweens.Add(gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {
                // 进度条拉长
                progressValue.DOFillAmount(point / 100f, 0.3f).SetDelay(0.4f);
                // 数字涨动
                pointTxt.DOChangeNumber(point, 0.3f).SetDelay(0.4f);
                //pointTxt.DOChangeNumberEx(point, 0.3f, 2f).SetDelay(0.4f);
            }));
            Timer.Register(this.gameObject, 0.15f, () =>
            {
                for (int i = 0; i < rewardList.Count; i++)
                {
                    var scale = rewardList[i].transform.localScale.x;
                    // 淡入
                    tweens.Add(rewardList[i].gameObject.DOFade(1, 0.3f).SetDelay(i * 0.08f));
                    // 放大
                    tweens.Add(rewardList[i].transform.DOScale(scale + 0.5f, 0.15f).SetDelay(i * 0.08f));
                    // 缩小
                    tweens.Add(rewardList[i].transform.DOScale(scale, 0.15f).SetDelay(0.15f + i * 0.08f));
                }
            });
        }

        public void PlayAnim(int currStar, int maxStar, float delay = 0.4f)
        {
            // 初始化动画
            Init();
            tweens.Add(gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {
                // 进度条拉长
                progressValue.DOFillAmount(currStar / (float)maxStar, 0.3f).SetDelay(delay);
                // 数字涨动
                //DOTween.To(value => pointTxt.text = ((int)value).ToString(), 0, currStar, 0.3f).SetDelay(delay);
                pointTxt.DOChangeNumber(currStar, 0.3f).SetDelay(0.4f);
            }));
            Timer.Register(this.gameObject, 0.15f, () =>
            {
                for (int i = 0; i < rewardList.Count; i++)
                {
                    var scale = rewardList[i].transform.localScale.x;
                    // 淡入
                    tweens.Add(rewardList[i].gameObject.DOFade(1, 0.3f).SetDelay(i * 0.08f));
                    // 放大
                    tweens.Add(rewardList[i].transform.DOScale(scale + 0.5f, 0.15f).SetDelay(i * 0.08f));
                    // 缩小
                    tweens.Add(rewardList[i].transform.DOScale(scale, 0.15f).SetDelay(0.15f + i * 0.08f));
                }
            });
        }

        // 播放进度条动画
        public void PlayProgressValueAnim(float oldValue, float newValue)
        {
            DisableShiny();
            DOTween.To(value =>
            {
                // 刷新文本
                pointTxt.text = ((int)value).ToString();
                // 涨进度
                progressValue.fillAmount = value / 100f;
            }, oldValue, newValue, 1.5f);
            icon2.DOFade(1, 0.2f);
            Sequence s1 = DOTween.Sequence();
            Sequence s2 = DOTween.Sequence();
            Sequence s3 = DOTween.Sequence();
            // 进度条变亮
            var progressEffect = progressValue.GetComponent<UIEffect>();
            DOTween.To(value => progressEffect.colorFactor = PeriodicFunction.Trigonometric(value) * 0.5f, 0, 1, 1.2f).
                SetEase(Ease.Linear).OnStart(() =>
                {
                    AudioManager.Instance.PlaySound(AudioNames.COLLECT_POINTS);
                });
            // 活跃点图标跳动,闪亮
            for (int i = 0; i < 6; i++)
            {
                // 跳动
                s1.AppendCallback(() => icon.transform.localScale = Vector3.one * 1.05f);
                s1.Append(icon.transform.DOScale(1, 0.2f));
                s2.AppendCallback(() => icon2.transform.localScale = Vector3.one * 1.05f);
                s2.Append(icon2.transform.DOScale(1, 0.2f));
                // 闪烁
                s3.AppendCallback(() => icon2.SetAlpha(1));
                if (i < 5)
                {
                    s3.Append(icon2.DOFade(0.5f, 0.2f));
                }
            }
            s3.AppendCallback(() => icon2.SetAlpha(1));
            s3.Append(icon2.DOFade(0, 0.2f));
            s3.OnComplete(EnableShiny);
        }

        // 开启闪烁循环动画
        public void EnableShiny()
        {
            shinys.ForEach(item => item.Play());
        }

        // 关闭闪烁循环动画
        public void DisableShiny()
        {
            shinys.ForEach(item =>
            {
                item.effectFactor = 0;
                item.Stop();
            });
        }

        public override void PlayExit()
        {
            tweens.Add(gameObject.DOFade(0, 0.2f));
        }
    }
}