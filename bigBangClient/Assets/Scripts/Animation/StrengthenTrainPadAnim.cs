using System;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    public class StrengthenTrainPadAnim : MonoBehaviour
    {
        [SerializeField] private StrengthenTrainPadComponent com;

        public static bool isPlaying = false;

        //侧边滑入动画
        public void Play()
        {
            com.TrainAllBtn.gameObject.SetAlpha(0);
            com.TrainAllBtn.GetComponent<RectTransform>().SetAnchoredPositionY(270);
            for (int i = 0; i < com.ItemList.Count; i++)
            {
                var anim = com.Content.GetChild(i).GetComponent<StrengthenItemAnim>();
                anim.Play(i * 0.05f);
            }

            //按钮上移
            com.TrainAllBtn.GetComponent<RectTransform>().DoRelativeAnchorPosY(-25, 0.25f).From().SetDelay(0.5f);
            DOTween.To(value => com.TrainAllBtn.gameObject.SetAlpha(value), 0, 1, 0.25f).SetDelay(0.5f);
        }

        public void PlayTrainAll(Action callback)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < com.ItemList.Count; i++)
            {
                com.Content.GetChild(i).GetComponent<StrengthenItemComponent>().BtnAnim.Play(delay: i * 0.05f, playAudio: false);
                com.Content.GetChild(i).GetComponent<StrengthenItemAnim>().PlayClick(i * 0.05f);
                if (i == com.ItemList.Count - 1)
                {
                    com.Content.GetChild(i).GetComponent<StrengthenItemAnim>().PlayClick(i * 0.05f, callback);
                }
            }
        }
    }
}
