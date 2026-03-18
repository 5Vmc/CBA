using BigBang.UI;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityTimer;
using Utils;

namespace BigBang.Animation
{
    public class CardUpgradeUIAnim : AnimBase
    {
        [SerializeField] private RectTransform cardRect4Star;

        [SerializeField] private RectTransform qualityLowCardRect;
        [SerializeField] private RectTransform qualityHighCardRect;

        //[SerializeField] private RectTransform upgradeStarBtn;
        //[SerializeField] private RectTransform upgradeQualityBtn;
        [SerializeField] private RectTransform info;
        [SerializeField] private TMP_Text sort;
        [SerializeField] private CardItemAnim cardAnim;
        [SerializeField] private CardTrainInfoItem cardTrainInfoItem;
        [SerializeField] private TMP_Text fightAddText;
        [SerializeField] private TMP_Text fightText;
        //[SerializeField] private GameObject propPad;


        [SerializeField] private GameObject[] maxInfoItems; //全满的时候

        public event Action PlayTxtAnim;

        private CardUpgradeType upgradeType;

        public void SetUpgradeType(CardUpgradeType type)
        {
            this.upgradeType = type;
        }

        public override void Init()
        {
            base.Init();
            // 初始化位置
            //info.SetAnchoredPositionY(-5);

            qualityLowCardRect.gameObject.SetAlpha(1.0f);

            // 初始化缩放卡片
            if (this.upgradeType == CardUpgradeType.UpgradeStar)
            {
                cardRect4Star.localScale = Vector3.one * 0.5f;
            }


            cardTrainInfoItem.InitTurnAnim();
            // 初始化透明度
            //info.gameObject.SetAlpha(0);
        }


        private Tween PlayTurn(Transform trans, float playTime, float delayTime)
        {
            trans.eulerAngles = new Vector3(-90, 0, 0);
            return trans.DORotate(new Vector3(0, 0, 0), playTime).SetDelay(delayTime).SetEase(Ease.Linear);
        }
        public void PlayMaxInfoItemsAnim()
        {
            // 旋转时间
            float turnTime = 0.3f;

            int half = this.maxInfoItems.Length / 2;
            for (int i = 0; i < half; i++)
            {
                this.PlayTurn(this.maxInfoItems[i].transform, turnTime / half, (i + 1) * 0.05f);
            }

            for (int i = half; i < this.maxInfoItems.Length; i++)
            {
                this.PlayTurn(this.maxInfoItems[i].transform, turnTime / half, (i + 1 - half) * 0.05f);
            }
        }

        public void ChangeUpgradeType(CardUpgradeType changeType)
        {
            if (this.upgradeType == changeType)
                return;
            this.ClearAnim();
            this.upgradeType = changeType;
            // 初始化缩放卡片
            if (this.upgradeType == CardUpgradeType.UpgradeStar)
            {
                cardRect4Star.localScale = Vector3.one * 0.5f;
            }

            if (this.upgradeType == CardUpgradeType.UpgradeStar)
                tweens.Add(cardRect4Star.DOScale(Vector3.one * 1.5f, 0.2f));

        }

        // 播放进入动画
        public override void PlayEnter()
        {
            base.PlayEnter();
            // 卡片放大出现
            //if (this.upgradeType == CardUpgradeType.UpgradeStar)
            tweens.Add(cardRect4Star.DOScale(Vector3.one * 1.5f, 0.2f).OnComplete(cardTrainInfoItem.PlayTurnAnim));

            // 信息上浮淡入
            //tweens.Add(info.DoRelativeAnchorPosY(100, 0.2f).SetDelay(0.1f));
        }

        // 播放升星动画
        public void PlayUpStar(Action callback)
        {
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.EVENT_UPSTAR);
            // 播放升星动画
            cardAnim.PlayUpgrade();
            Babu.DelayTaskService.Instance.Run(this.gameObject, 0.5f, () =>
            {
                FightPointAni(callback);
            });
            // 材料向下扣除淡出
            PlayTxtAnim?.Invoke();
        }

        private void FightPointAni(Action callback)
        {
            // 字体变大
            tweens.Add(DOTween.To(value => sort.fontSize = (int)value, 44, 55, 0.1f).OnComplete(() =>
            {
                // 加数值动画
                cardTrainInfoItem.PlayAddScoreAnim();
                var value1 = int.Parse(fightText.text.TrimStart('+'));
                var value2 = int.Parse(fightAddText.text.TrimStart('+'));
                tweens.Add(DOTween.To(value => fightText.text = ((int)value).ToString(), value1, value1 + value2, 1f).SetEase(Ease.Linear));
                //tweens.Add(DOTween.To(value => fightAddText.text = "+" + ((int)value).ToString(), value2, 0, 1f).SetEase(Ease.Linear));
                // 字体变小
                tweens.Add(DOTween.To(value => sort.fontSize = (int)value, 55, 44, 0.05f).SetDelay(1.5f).OnComplete(() =>
                {
                    Timer.Register(this.gameObject, 0.8f, () =>
                    {
                        TouchManager.Instance.EnableTouch();
                        callback?.Invoke();
                    });
                }));

            }));
        }

        public void PlayUpGrade(Action callback)
        {
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.EVENT_UPSTAR);
            Sequence seq = DOTween.Sequence();
            seq.Insert(0.5f, qualityLowCardRect.gameObject.DOFade(0, 0.5f));
            seq.Insert(1.2f, qualityHighCardRect.gameObject.DOFade(0, 0.5f).From());
            seq.Insert(1.2f, qualityHighCardRect.DOScale(3, 0.5f).SetEase(Ease.OutBack).From());
            seq.OnComplete(() =>
            {
                FightPointAni(callback);
            });
        }
    }
}
