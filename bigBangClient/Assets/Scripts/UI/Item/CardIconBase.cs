using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class CardIconBase : MonoBehaviour
    {
        // 球员姓名
        [SerializeField] private TMP_Text nameText;
        // 球员头像
        [SerializeField] private Image qualityImg;
        [SerializeField] protected Image playerImg;

        [SerializeField] private PeakImage peakImage = null;

        public async void SetData(CardModelConfig config)
        {
            // 设置球员头像
            playerImg.sprite = await SpriteProxy.GetPlayerPortrait(config.Portrait);
            // 设置品质
            qualityImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Icon, config.Quality);
            // 设置球员姓名
            if (nameText != null)
            {
                nameText.text = config.Name;
            }

            peakImage.SetData(config);
        }
        public async void SetQuality(int quality)
        {
            // 设置品质
            qualityImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.Icon, quality);
        }
    }
}