using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class AllStarHomeUIAnim : AnimBase
    {
        [SerializeField] private RectTransform top;
        [SerializeField] private List<AllStarHomePlayerAnim> iconAnimListNorth = new();
        [SerializeField] private List<AllStarHomePlayerAnim> nameAnimListNorth = new();
        [SerializeField] private List<AllStarHomePlayerAnim> iconAnimListSouth = new();
        [SerializeField] private List<AllStarHomePlayerAnim> nameAnimListsouth = new();
        [SerializeField] private Image lightImage = null;
        [SerializeField] private Image vsImage = null;
        [SerializeField] private RectTransform contentPanel = null;
        [SerializeField] private RectTransform timeBar = null;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            top.SetAnchoredPositionY(170);

            foreach (var item in iconAnimListNorth)
            {
                item.Init();
            }
            foreach (var item in nameAnimListNorth)
            {
                item.Init();
            }
            foreach (var item in iconAnimListSouth)
            {
                item.Init();
            }
            foreach (var item in nameAnimListsouth)
            {
                item.Init();
            }

            lightImage.SetAlpha(0);
            lightImage.transform.SetLocalScale(0);
            vsImage.SetAlpha(0);
            vsImage.transform.SetLocalScale(0);
            contentPanel.gameObject.SetAlpha(0);
            timeBar.gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 顶部栏下移
            tweens.Add(top.DOAnchorPosY(0, 0.3f));

            tweens.Add(lightImage.DOFade(1f, 1f));
            tweens.Add(lightImage.transform.DOScale(1f, 1f).SetEase(Ease.OutBack));

            float delay = 0.5f;
            for (int i = 0; i < 5; i++)
            {
                iconAnimListNorth[i].PlayEnter(delay);
                iconAnimListSouth[i].PlayEnter(delay);
                nameAnimListNorth[i].PlayEnter(delay + 0.1f);
                nameAnimListsouth[i].PlayEnter(delay + 0.1f);
                delay += 0.2f;
            }

            delay += 0.3f;

            tweens.Add(vsImage.DOFade(1f, 0.2f).SetDelay(delay));
            tweens.Add(vsImage.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetDelay(delay));

            tweens.Add(contentPanel.gameObject.DOFade(1f, 0.5f).SetDelay(0f));
            tweens.Add(timeBar.gameObject.DOFade(1f, 0.5f).SetDelay(0f));
        }
    }
}
