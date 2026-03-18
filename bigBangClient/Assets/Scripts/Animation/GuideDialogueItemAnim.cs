using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

namespace BigBang.Animation
{
    public class GuideDialogueItemAnim : AnimBase
    {
        [SerializeField] private bool isLeftItem;
        [SerializeField] private TMP_Text content;
        [SerializeField] private RectTransform bubble;

        private string sourceTxt;

        public override void Init()
        {
            base.Init();
            content.maxVisibleCharacters = 0;
            bubble.localScale = new Vector3(0, 1, 1);
            sourceTxt = content.text;
        }

        public void PlayAnim(Action callback)
        {
            Init();
            AudioManager.Instance.PlaySound(AudioNames.MEETING_CHATPOP);
            // 气泡展开
            bubble.DOScaleX(1, 0.4f).OnComplete(() =>
            {
                float doTextTime = Utils.Utility.KeepInRange(ReplaceHtmlTag(sourceTxt).Length * 0.05f, 0.5f, 3f);//限制最短和最长时间
                content.DOText(sourceTxt, doTextTime).SetEase(Ease.Linear).OnComplete(() =>
                {
                    // 触发说话完成事件
                    callback?.Invoke();
                });
            });
        }

        /// <summary>
        /// 去除html标签
        /// </summary>
        public static string ReplaceHtmlTag(string html)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");
            return strText;
        }

    }
}