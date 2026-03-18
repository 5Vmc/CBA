using BigBang.Animation;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class DebrisItem : MonoBehaviour
    {
        [SerializeField] private DebrisIconItem iconItem;
        [SerializeField] private Image debrisLightImg;
        [SerializeField] private Image debrisBorder;
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text countText;
        [SerializeField] public DebrisItemAnim Anim { get; private set; }
        [SerializeField] private PeakImage peakImage = null;

        private void Awake()
        {
            Anim = GetComponent<DebrisItemAnim>();
        }

        public async void SetData(CardModelConfig cfg, int count)
        {
            debrisLightImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.DebrisLight, cfg.Quality);
            debrisBorder.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.DebrisBorder, cfg.Quality);
            iconItem.SetData(cfg.Portrait, cfg.Quality);
            peakImage.SetData(cfg);
            // 设置球员姓名
            playerName.text = cfg.Name;
            // 设置碎片数量
            countText.text = $"×{count}";
        }
    }
}