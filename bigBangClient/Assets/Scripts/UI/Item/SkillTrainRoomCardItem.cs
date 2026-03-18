using Babu;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class SkillTrainRoomCardItem : MonoBehaviour
    {
        [SerializeField] private Image noneImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;

        public async void SetData(CardModelConfig config)
        {
            noneImage.gameObject.SetActive(config == null);
            iconImage.gameObject.SetActive(config != null);
            nameText.gameObject.SetActive(config != null);
            if (config != null)
            {
                // 设置球员头像
                iconImage.sprite = await SpriteProxy.GetPlayerPortrait(config.Portrait);
                // 设置球员姓名
                nameText.text = PlayerCard.GetFullName(config);
            }
        }
        public void SetQuality(int quality)
        {
            nameText.color = CBAColorUtil.Instance.GetColor(quality);
        }
    }
}