using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using BigBang.UI;
using Utils;
using System;

namespace BigBang.Animation
{
    public class RecruitResultUIAnim : AnimBase
    {
        [SerializeField] private CardAndDebrisItem cardAndDebrisItem;
        [SerializeField] private List<CardAndDebrisItem> cardAndDebrisItems;
        [SerializeField] private GameObject btn1;
        [SerializeField] private GameObject btn2;

        [SerializeField] private List<float> initY = new List<float>();

        [SerializeField] private CanvasGroup freebiePanel = null;

        private void Awake()
        {
            cardAndDebrisItems.ForEach(item =>
            {
                initY.Add(item.GetComponent<RectTransform>().anchoredPosition.y);
            });
        }

        public override void Init()
        {
            base.Init();
            // 初始化位置
            // 卡牌下沉距离
            float distance = 150;
            int index = 0;
            cardAndDebrisItems.ForEach(item =>
            {
                // 显示背面
                item.CardItem.ShowBack();
                var rect = item.GetComponent<RectTransform>();
                rect.SetAnchoredPositionY(initY[index] - distance);
                // 初始化闪光动画
                item.CardItem.Anim.InitLightAnim();
                index++;
            });
            cardAndDebrisItem.CardItem.ShowBack();
            cardAndDebrisItem.CardItem.Anim.InitLightAnim();
            var cardRect = cardAndDebrisItem.GetComponent<RectTransform>();
            cardRect.SetAnchoredPositionY(127 - distance);
            // 初始化透明度
            btn1.SetAlpha(0);
            btn2.SetAlpha(0);
            cardAndDebrisItems.ForEach(item =>
            {
                item.CardItem.gameObject.SetAlpha(0);
                item.DebrisItem.gameObject.SetAlpha(0);
            });
            cardAndDebrisItem.CardItem.gameObject.SetAlpha(0);
            cardAndDebrisItem.DebrisItem.gameObject.SetAlpha(0);
            // 初始化缩放
            cardAndDebrisItems.ForEach(item =>
            {
                item.CardItem.transform.localScale = Vector3.one;
                item.DebrisItem.transform.localScale = Vector3.one * 0.47f;
            });
            cardAndDebrisItem.CardItem.transform.localScale = Vector3.one;
            cardAndDebrisItem.DebrisItem.transform.localScale = Vector3.one * 0.47f;

            cardAndDebrisItem.CardItem.ResetBack();
            cardAndDebrisItems.ForEach(item => item.CardItem.ResetBack());

            freebiePanel.SetAlpha(0);
        }

        [EditorButton("播放抽卡动画")]
        public void PlayEnter(bool isOnce, Action callback)
        {
            base.PlayEnter();
            // 禁用触摸
            TouchManager.Instance.DisableTouch();
            int index = 0;
            cardAndDebrisItems.ForEach(item =>
            {
                index++;
                // 卡片淡入
                tweens.Add(item.CardItem.gameObject.DOFade(1, 0.6f).SetDelay(index * 0.05f));
                // 卡片上移
                tweens.Add(item.GetComponent<RectTransform>().DoRelativeAnchorPosY(150, 0.6f).SetDelay(index * 0.05f));
            });
            // 单张卡片淡入
            tweens.Add(cardAndDebrisItem.CardItem.gameObject.DOFade(1, 0.3f));
            // 单张卡片上移
            tweens.Add(cardAndDebrisItem.GetComponent<RectTransform>().DoRelativeAnchorPosY(150, 0.6f));
            // 卡片旋转
            // 旋转次数
            int count = 2;
            // 旋转时间
            float turnTime = 0.3f;
            index = 0;
            cardAndDebrisItems.ForEach(item =>
            {
                index++;
                Sequence s = DOTween.Sequence();
                for (int i = 0; i < count * 2; i++)
                {
                    s.Append(item.CardItem.Anim.PlayTurn(turnTime / count));
                }
                s.SetDelay(index * 0.05f);
                tweens.Add(s);
            });
            Sequence s = DOTween.Sequence();
            for (int i = 0; i < count * 2; i++)
            {
                s.Append(cardAndDebrisItem.CardItem.Anim.PlayTurn(turnTime / count));
            }
            tweens.Add(s);
            index = 0;
            bool ding = false;

            // 10张翻牌显示正面
            cardAndDebrisItems.ForEach(item =>
            {
                index++;
                tweens.Add(item.CardItem.Anim.PlayTurn(0.5f, item.CardItem.HidBack).SetDelay(turnTime * 2 + 1f).OnComplete(() =>
                {
                    if (item.CardItem.Quality >= QualityType.Orange)
                    {
                        if (item.gameObject.activeInHierarchy && !ding)
                        {
                            AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUT_DING);
                            ding = true;
                        }
                        tweens.Add(item.CardItem.Anim.PlayLight());
                        if (!item.IsDebris)
                        {
                            item.CardItem.Anim.PlayUIParticle();
                        }
                    }
                    if (item.IsDebris)
                    {
                        Babu.DelayTaskService.Instance.Run(this.gameObject, 1f, item.Anim.PlayChangeAnim);
                    }
                }));
            });
            // 1张翻牌显示正面
            tweens.Add(cardAndDebrisItem.CardItem.Anim.PlayTurn(0.5f, cardAndDebrisItem.CardItem.HidBack).OnComplete(() =>
            {
                if (cardAndDebrisItem.CardItem.Quality >= QualityType.Orange)
                {
                    if (cardAndDebrisItem.gameObject.activeInHierarchy && !ding)
                    {
                        AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUT_DING);
                        ding = true;
                    }
                    tweens.Add(cardAndDebrisItem.CardItem.Anim.PlayLight());
                    if (!cardAndDebrisItem.IsDebris)
                    {
                        cardAndDebrisItem.CardItem.Anim.PlayUIParticle();
                    }
                }
                // 如果是碎片，播放转换碎片动画
                if (cardAndDebrisItem.IsDebris)
                {
                    Babu.DelayTaskService.Instance.Run(this.gameObject, 1f, cardAndDebrisItem.Anim.PlayChangeAnim);
                }
            }).SetDelay(turnTime * 2 + 0.5f));

            float btnShowDelayTime = 0f;
            bool isDebris = false;
            if (isOnce)
            {
                btnShowDelayTime = turnTime * 2 + 0.5f + 0.5f;
                if (cardAndDebrisItem.IsDebris) isDebris = true;
                if (isDebris) btnShowDelayTime += 1f + 1f;
            }
            else
            {
                btnShowDelayTime = turnTime * 2 + 1f + 0.5f;
                foreach (var item in cardAndDebrisItems)
                {
                    if (item.IsDebris)
                    {
                        isDebris = true;
                        break;
                    }
                }
                if (isDebris) btnShowDelayTime += 1f + 1f;
            }

            //显示赠品
            freebiePanel.DOFade(1, 0.3f).SetDelay(btnShowDelayTime);

            // 显示按钮
            btn1.DOFade(1, 0.3f).SetDelay(btnShowDelayTime);
            btn2.DOFade(1, 0.3f).SetDelay(btnShowDelayTime).OnComplete(() =>
            {
                // 启用触摸
                TouchManager.Instance.EnableTouch();
                callback?.Invoke();
            });
        }
    }
}