using deVoid.UIFramework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BigBang.UI
{
    [Serializable]
    public class PlayerPosHelpProperties : WindowProperties
    {
        public PlayerCard playerCard;
        public PlayerPosHelpProperties(PlayerCard playerCard)
        {
            this.playerCard = playerCard;
        }
    }
    public class PlayerPosHelpUI : AWindowController<PlayerPosHelpProperties>
    {
        [SerializeField] Button closeBtn;
        [SerializeField] List<RectTransform> PosLogoList;
        [SerializeField] List<TMP_Text> PosNumText;

        [SerializeField] private HorizontalLayoutGroup nameLayout = null;
        [SerializeField] private PeakImage peakImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text nameNextText = null;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            nameText.text = Properties.playerCard.Config.Name;
            peakImage.SetData(Properties.playerCard);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameNextText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameLayout.transform as RectTransform);

            foreach (var item in PosLogoList)
            {
                item.gameObject.SetActive(false);
            }
            foreach (var posId in Properties.playerCard.Config.AdaptPosition)
            {
                PosLogoList[posId - 1].gameObject.SetActive(true);
                PosNumText[posId - 1].text = Properties.playerCard.GetStarCombatEffectiveness(Properties.playerCard.Star, Properties.playerCard.Quality, posId).ToString();
            }
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<PlayerPosHelpUI>();
        }
    }
}
