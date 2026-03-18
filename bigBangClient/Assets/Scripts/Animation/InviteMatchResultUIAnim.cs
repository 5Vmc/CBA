using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.Animation
{
    public class InviteMatchResultUIAnim : MonoBehaviour
    {
        [SerializeField] private Image blackImg;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private Image bloomImg;
        [SerializeField] private Image lightImg;
        [SerializeField] private Image iconImg;
        [SerializeField] private Image diamondImg;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private TMP_Text resultContentText;

        [SerializeField] private RectTransform videoBtn;

        [SerializeField] private RectTransform diamondBtn;
        [SerializeField] private RectTransform rewardRect;

        [SerializeField] private RectTransform confirmBtn;
        [SerializeField] private Image pointImg;
        [SerializeField] private GameObject particle;

        private List<Tween> tweens = new List<Tween>();

        //初始化
        private void Init()
        {
            //设置位置
            resultContentText.rectTransform.SetAnchoredPositionY(66);
            confirmBtn.SetAnchoredPositionY(-117);
            videoBtn.SetAnchoredPositionY(-117);
            diamondBtn.SetAnchoredPositionY(-117);
            diamondImg.rectTransform.SetAnchoredPositionY(0);
            diamondImg.rectTransform.SetAnchoredPositionX(-24);
            //设置缩放
            rewardRect.localScale = Vector3.one * 3;
            iconImg.rectTransform.localScale = Vector3.one * 0.1f;
            lightImg.rectTransform.localScale = Vector3.zero;
            diamondImg.rectTransform.localScale = Vector3.one * 1.2f;
            //设置透明度
            lightImg.SetAlpha(0);
            diamondImg.SetAlpha(0);
            blackImg.SetAlpha(0);
            backgroundImg.SetAlpha(0);
            rewardText.SetAlpha(0);
            rewardRect.gameObject.SetAlpha(0);
            resultContentText.SetAlpha(0);
            confirmBtn.gameObject.SetAlpha(0);
            videoBtn.gameObject.SetAlpha(0);
            diamondBtn.gameObject.SetAlpha(0);
        }

        //播放进入动效
        public void Play()
        {
            Kill();
            Init();
            //背景淡入
            tweens.Add(blackImg.DOFade(1, 0.1f));
            tweens.Add(backgroundImg.DOFade(1, 0.1f));
            // ⚠临时
            int random = UnityEngine.Random.Range(0, 3);
            if (random == 0)
            {
                AudioManager.Instance.PlaySound(AudioNames.RSLT_WIN_01);
            }
            else if (random == 1)
            {
                AudioManager.Instance.PlaySound(AudioNames.RSLT_WIN_02);
            }
            else
            {
                AudioManager.Instance.PlaySound(AudioNames.RSLT_WIN_03);
            }
            //砖石袋出现
            tweens.Add(iconImg.DOFade(1, 0.3f).OnComplete(() =>
            {
                //背光闪烁
                tweens.Add(lightImg.DOFlash(1, 1f, 1f, 0, 0.5f, 1).SetLoops(-1));
                //背光旋转
                tweens.Add(lightImg.rectTransform.DORotate(Vector3.forward * 90f, 10, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1));
                //标题出现
                tweens.Add(resultContentText.rectTransform.DoRelativeAnchorPosY(-50, 0.3f).From());
                Timer.Register(this.gameObject, 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.RSLT_INFO));
                tweens.Add(resultContentText.DOFade(1, 0.3f).OnComplete(() =>
                {
                    //计算砖石位置
                    diamondImg.rectTransform.SetAnchoredPositionX(-diamondImg.rectTransform.rect.width / 2f - rewardText.rectTransform.rect.width / 2f);
                    //文字砸入
                    tweens.Add(rewardRect.DOScale(1, 0.2f).SetDelay(0.3f).OnComplete(() =>
                    {
                        // 按钮上移淡入
                        tweens.Add(confirmBtn.DoRelativeAnchorPosY(-25, 0.3f).From());
                        tweens.Add(confirmBtn.gameObject.DOFade(1, 0.3f));
                        tweens.Add(videoBtn.DoRelativeAnchorPosY(-25, 0.3f).From());
                        tweens.Add(videoBtn.gameObject.DOFade(1, 0.3f));
                        tweens.Add(diamondBtn.DoRelativeAnchorPosY(-25, 0.3f).From());
                        tweens.Add(diamondBtn.gameObject.DOFade(1, 0.3f));
                    }));
                    //文字淡入
                    tweens.Add(rewardText.DOFade(1, 0.2f).SetDelay(0.3f));
                    tweens.Add(rewardRect.gameObject.DOFade(1, 0.2f).SetDelay(0.3f));
                    //砖石淡入
                    tweens.Add(diamondImg.DOFade(1, 0.2f));
                }));
            }));
            //背光缩放
            tweens.Add(lightImg.rectTransform.DOScale(1, 0.3f));
            //背光出现
            tweens.Add(lightImg.DOFade(1, 0.3f));
            tweens.Add(iconImg.rectTransform.DOScale(1, 0.3f));
            //泛光淡入
            tweens.Add(bloomImg.DOFade(1, 0.3f));
        }

        //播放钻石移动效果
        public void PlayDiamondMove(TweenCallback callback)
        {
            Kill();
            //背光淡出
            tweens.Add(lightImg.DOFade(0, 0.3f));
            tweens.Add(bloomImg.DOFade(0, 0.3f));
            //文字淡出
            tweens.Add(rewardText.DOFade(0, 0.3f));
            tweens.Add(resultContentText.DOFade(0, 0.3f));
            //钻石袋淡出
            tweens.Add(iconImg.DOFade(0, 0.3f));
            //按钮淡出
            tweens.Add(confirmBtn.gameObject.DOFade(0, 0.3f));
            tweens.Add(videoBtn.gameObject.DOFade(0, 0.3f));
            tweens.Add(diamondBtn.gameObject.DOFade(0, 0.3f));
            //背景淡出
            tweens.Add(blackImg.DOFade(0, 0.3f));
            tweens.Add(backgroundImg.DOFade(0, 0.3f));
            //砖石淡出
            tweens.Add(diamondImg.DOFade(0, 0.3f));
            //砖石移动
            FlyDiamond();
            // tweens.Add(diamondImg.rectTransform.DOMove(TrainUI.SpeedDiamond.position, 0.5f));
            // 目标跳动
            tweens.Add(TrainUI.ExpText.transform.DOScale(1.2f, 0.05f).OnStart(() => AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_1)).SetDelay(0.3f));//0.4f
            tweens.Add(TrainUI.ExpText.transform.DOScale(1, 0.05f).SetDelay(0.35f));
            tweens.Add(TrainUI.ExpText.transform.DOScale(1.2f, 0.05f).OnStart(() => AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_1)).SetDelay(0.4f));
            tweens.Add(TrainUI.ExpText.transform.DOScale(1, 0.05f).SetDelay(0.45f));
            tweens.Add(TrainUI.ExpText.transform.DOScale(1.2f, 0.05f).OnStart(() => AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_1)).SetDelay(0.5f));
            tweens.Add(TrainUI.ExpText.transform.DOScale(1, 0.05f).SetDelay(0.55f));
            tweens.Add(TrainUI.ExpText.transform.DOScale(1.2f, 0.05f).OnStart(() => AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_1)).SetDelay(0.6f));
            tweens.Add(TrainUI.ExpText.transform.DOScale(1, 0.05f).SetDelay(0.65f));
            Sequence sequence = DOTween.Sequence();
            sequence.Append(TrainUI.SpeedDiamond.DOScale(1.2f, 0.05f).SetDelay(0.3f));
            sequence.Append(TrainUI.SpeedDiamond.DOScale(1, 0.05f));

            sequence.Append(TrainUI.SpeedDiamond.DOScale(1.2f, 0.05f));
            sequence.Append(TrainUI.SpeedDiamond.DOScale(1, 0.05f));

            sequence.Append(TrainUI.SpeedDiamond.DOScale(1.2f, 0.05f));
            sequence.Append(TrainUI.SpeedDiamond.DOScale(1, 0.05f));

            sequence.OnComplete(callback);

            tweens.Add(diamondImg.rectTransform.DOScale(Vector3.one, 0.5f));
        }

        //经验飘动动画
        private void FlyDiamond()
        {
            var clone = Instantiate(pointImg.gameObject, pointImg.transform.parent);
            var cloneParticle = Instantiate(particle);

            clone.transform.position = pointImg.transform.position;
            clone.transform.SetParent(pointImg.transform.parent.parent.parent, false);
            clone.transform.SetAsLastSibling();
            cloneParticle.transform.SetParent(clone.transform, false);
            cloneParticle.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            cloneParticle.SetActive(true);
            clone.transform.DOMove(TrainUI.SpeedDiamond.position, 0.3f).SetEase(Ease.Linear);
            //缩小
            clone.transform.DOScale(1f, 0.3f).OnComplete(() =>
            {
                //层级设置
                cloneParticle.transform.SetParent(transform.parent, false);
                clone.SetActive(false);
                Destroy(cloneParticle, 3);
                Destroy(clone.gameObject, 3);
            });
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}