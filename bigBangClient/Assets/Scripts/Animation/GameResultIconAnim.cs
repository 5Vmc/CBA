using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;

namespace BigBang.Animation
{
    public class GameResultIconAnim : AnimBase
    {
        private Image _img;
        private Image img
        {
            get
            {
                if(_img == null)
                {
                    _img = GetComponent<Image>();
                }
                return _img;
            }
        }

        private Sprite source;

        public override async void Init()
        {
            base.Init();
            // 保存原来的图片
            source = img.sprite;
            // 设置为灰色图片
            img.sprite = await SpriteProxy.GetGameResult(GameResultType.None);
            // 初始化缩放
            img.rectTransform.localScale = Vector3.one;
            // 初始化透明度
            //img.SetAlpha(0);
        }

        public void Play(float delay)
        {
            ClearAnim();
            //// 淡入
            //img.DOFade(1, 0.1f).SetDelay(delay).OnComplete(() =>
            //{
                
            //});
            // 翻转
            img.rectTransform.DOScale(new Vector3(1, 0, 1), 0.05f).SetEase(Ease.Linear).SetDelay(delay).OnComplete(() =>
            {
                img.sprite = source;
                img.rectTransform.DOScale(Vector3.one, 0.05f).SetEase(Ease.Linear);
            });
        }
    }
}