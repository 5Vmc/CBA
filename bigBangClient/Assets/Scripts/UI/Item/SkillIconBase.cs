using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class SkillIconBase : MonoBehaviour
    {
        [SerializeField] protected TMP_Text nameText;
        [SerializeField] protected TMP_Text descText;
        [SerializeField] protected Image skillImg;
        [SerializeField] protected Image qualityImg;

        public async void SetData(SkillConfig config)
        {
            if (nameText != null) nameText.text = config.Name;
            if (skillImg != null) skillImg.sprite = await SpriteProxy.GetSkillIcon(config.Icon);
            if (qualityImg != null) SpriteManager.GetSprite(AtlasNames.Skill, "quality_" + config.Quality, s => qualityImg.sprite = s);
        }
    }


}