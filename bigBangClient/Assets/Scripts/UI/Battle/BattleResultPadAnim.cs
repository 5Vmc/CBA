using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class BattleResultPadAnim : MonoBehaviour
    {
        public Action onPlayEnd;


        [SerializeField] private TMP_Text successText = null;
        [SerializeField] private RectTransform icon = null;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            icon.gameObject.SetAlpha(0);
            icon.localScale = Vector3.one * 0.7f * 0.46f;
        }

        public void Play()
        {
            Kill();
            Init();

            AudioManager.Instance.PlaySound(AudioNames.TECHBOARD_POP);

            float duration = 0.5f;
            tweens.Add(icon.DOScale(Vector3.one * 0.46f, duration));
            tweens.Add(icon.gameObject.DOFade(1f, duration).OnComplete(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.ANI_QUICKCD);
            }));

            float typetime = successText.text.Length * 0.15f;
            //胜利文字打字机效果
            tweens.Add(successText.DOText(successText.text, typetime).SetEase(Ease.Linear).SetDelay(0.7f).OnComplete(() =>
            {
                onPlayEnd?.Invoke();
            }));
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}