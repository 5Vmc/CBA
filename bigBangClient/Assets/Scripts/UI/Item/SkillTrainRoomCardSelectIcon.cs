using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BigBang.Animation;
using Utils;

namespace BigBang.UI
{
    public class SkillTrainRoomCardSelectIcon : CardSelectIcon
    {
        [SerializeField] private Image stateImage;

        public RectTransform PlayerImgRect { get => playerImg.rectTransform; }

        public async void SetState(SkillTrainSelectCardState state)
        {
            if (state == SkillTrainSelectCardState.Normal)
            {
                stateImage.gameObject.SetActive(false);
            }
            else
            {
                stateImage.gameObject.SetActive(true);
                stateImage.sprite = await SpriteProxy.GetSkillStateImage((int)state);
            }
        }

        public void ShowSelectAnim()
        {
            selectImg.rectTransform.gameObject.SetActive(true);
            border.gameObject.SetActive(true);
            border.SetAlpha(0);
            selectImg.gameObject.SetAlpha(0);
            selectImg.gameObject.DOFade(1, 0.15f);
            border.DOFade(1, 0.15f);
        }
    }
}