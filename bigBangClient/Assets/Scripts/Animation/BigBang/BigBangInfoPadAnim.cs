using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using UnityTimer;
using Utils;

namespace BigBang.Animation
{
    public class BigBangInfoPadAnim : MonoBehaviour
    {
        [SerializeField] private BigBangPadComponent com;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            com.ReadyCircle.SetAlpha(0);
            com.ReadyLine.fillAmount = 0;
            com.Pad.SetAnchoredPositionY(200);
            com.LineText.SetAlpha(0);
            com.ReadyFlashImg.SetAlpha(0);
            com.UnReadyFlashImg0.SetAlpha(0);
            com.UnReadyFlashImg1.SetAlpha(0);
            com.PadText.ForEach(item => item.maxVisibleCharacters = 0);
        }

        public void Play()
        {
            Kill();
            Init();
            //圆圈淡入
            tweens.Add(com.ReadyCircle.DOFade(1, 0.1f).SetDelay(0.5f).OnComplete(() =>
            {
                Timer.Register(this.gameObject, 0.25f, () => AudioManager.Instance.PlaySound(AudioNames.ANI_BBBOARDREF));
                //线变长
                tweens.Add(com.ReadyLine.DOFillAmount(1, 0.3f));
                //标题出现
                tweens.Add(com.LineText.DOFade(1, 0.3f).SetDelay(0.15f));
                //板子滑下来
                tweens.Add(com.Pad.DOAnchorPosY(0, 0.3f).SetDelay(0.25f).OnComplete(() =>
                {
                    PlayText();
                    com.ReadyFlashImg.SetAlpha(1);
                    com.UnReadyFlashImg0.SetAlpha(1);
                    com.UnReadyFlashImg1.SetAlpha(1);
                }));
            }));
        }

        public void PlayText()
        {
            //打字机效果
            com.PadText.ForEach(item =>
            {
                item.DOText(item.text, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    item.maxVisibleCharacters = 50;
                });
            });
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }

        public void PlaySwitch()
        {
            DOTween.To(value => com.BackgroundGraphic.color = new Color(1, 1, 1, value), 1, 0, 0.3f);
        }

        private void OnDisable()
        {
            Kill();
        }
    }
}
