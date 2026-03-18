using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class CardSelectIcon : CardIconBase
    {
        [SerializeField] protected Image selectImg;
        [SerializeField] protected Image border;

        [SerializeField] protected TMP_Text achieveText;
        [SerializeField] protected TMP_Text notAchieveText;

        public void SetData(CardModelConfig config, bool isSelect = false, bool showAchieve = false)
        {
            base.SetData(config);
            var isAchieve = Player.CardManager.IsAchieve(config.Id);
            achieveText.gameObject.SetActive(showAchieve && isAchieve);
            notAchieveText.gameObject.SetActive(showAchieve && !isAchieve);

            selectImg.gameObject.SetActive(isSelect);
            border?.gameObject.SetActive(isSelect);
            if (isSelect)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_TARGET);
            }
        }
        public void SetQuality(int quality)
        {
            base.SetQuality(quality);
        }

        public void SetSelect(bool flag)
        {
            selectImg.gameObject.SetActive(flag);
            border.gameObject.SetActive(flag);
        }
    }
}