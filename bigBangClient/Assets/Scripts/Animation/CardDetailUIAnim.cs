using DG.Tweening;
using UnityEngine;
using Utils;
using UnityEngine.UI;
using BigBang.UI;
using System.Collections.Generic;
using TMPro;

namespace BigBang.Animation
{
    public class CardDetailUIAnim : AnimBase
    {
        [SerializeField] private GameObject center;
        [SerializeField] private Image backgroundImg;
        //[SerializeField] private CardTrainInfoItem itemlist;
        //[SerializeField] private List<Image> icons;

        public override void Init()
        {
            base.Init();
            // 初始缩放
            center.transform.localScale = Vector3.one * 0.9f;
            // 初始化透明度
            center.SetAlpha(0);
            backgroundImg.SetAlpha(0);
            //itemlist.InitTurnAnim();
            //icons.ForEach(item =>
            //{
            //    item.rectTransform.localScale = Vector3.one * 1.2f;
            //    item.SetAlpha(0);
            //});
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 背景淡入
            tweens.Add(backgroundImg.DOFade(1, 0.3f));
            // 放大
            tweens.Add(center.transform.DOScale(Vector3.one, 0.3f));
            // 淡入
            tweens.Add(center.DOFade(1, 0.3f).OnComplete(() =>
            {
                //for (int i = 0; i < icons.Count; i++)
                //{
                //    // 淡入
                //    tweens.Add(icons[i].DOFade(1, 0.3f).SetDelay(i * 0.1f));
                //    // 缩小
                //    tweens.Add(icons[i].rectTransform.DOScale(1, 0.3f).SetDelay(i * 0.1f));
                //}
                //itemlist.PlayTurnAnim();
            }));
        }
    }
}
