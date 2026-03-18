using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class SkillTrainRoomSkillItem : MonoBehaviour
    {
        [SerializeField] private Image noneImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descText;

        public async void SetData(SkillConfig config, bool needDesc = false)
        {
            noneImage.gameObject.SetActive(config == null);
            iconImage.gameObject.SetActive(config != null);
            nameText.gameObject.SetActive(config != null);
            descText.gameObject.SetActive(needDesc && config != null);
            if (config != null)
            {
                iconImage.sprite = await SpriteProxy.GetSkillIcon(config.Icon);
                nameText.text = config.Name;
                descText.text = config.Desc;
            }
        }
    }
}