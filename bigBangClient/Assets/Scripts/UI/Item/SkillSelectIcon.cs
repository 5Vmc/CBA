using DG.Tweening;
using GameConfig.Config;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class SkillSelectIcon : SkillIconBase
    {
        [SerializeField] private Image border;
        [SerializeField] private Image selectSign;
        [SerializeField] private Image stateImage;

        public RectTransform SkillImgRect { get => skillImg.rectTransform; }

        public void SetData(SkillConfig config)
        {
            base.SetData(config);
            border.gameObject.SetActive(false);
            selectSign.gameObject.SetActive(false);
        }

        public async void SetState(SkillTrainSelectSkillState state)
        {
            if (state == SkillTrainSelectSkillState.Normal)
            {
                stateImage.gameObject.SetActive(false);
            }
            else
            {
                stateImage.gameObject.SetActive(true);
                stateImage.sprite = await SpriteProxy.GetSkillStateImage((int)state);
            }
        }

        public void SetSelect(bool flag)
        {
            border.SetAlpha(1);
            selectSign.SetAlpha(1);
            border.gameObject.SetActive(flag);
            selectSign.gameObject.SetActive(flag);
        }

        public void ShowSelectAnim()
        {
            selectSign.gameObject.SetActive(true);
            border.gameObject.SetActive(true);
            selectSign.SetAlpha(0);
            border.SetAlpha(0);
            selectSign.DOFade(1, 0.15f);
            border.DOFade(1, 0.15f);
        }
    }
}