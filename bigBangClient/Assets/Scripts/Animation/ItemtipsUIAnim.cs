using UnityEngine;
using DG.Tweening;
using Utils;

namespace BigBang.Animation
{
    public class ItemtipsUIAnim : AnimBase
    {
        private RectTransform rect;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public override void Init()
        {
            base.Init();
            rect.localScale = Vector3.one * 0.5f;
            rect.gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 面板弹出音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
            rect.DOScale(1, 0.1f);
            rect.gameObject.DOFade(1, 0.1f);
        }
    }
}