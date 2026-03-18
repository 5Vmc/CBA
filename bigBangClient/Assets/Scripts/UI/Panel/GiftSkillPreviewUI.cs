using System.Collections.Generic;
using System.Linq;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class GiftSkillPreviewProperties : WindowProperties
    {
        public int cardId { get; set; }
        public PlayerCard card;

        public GiftSkillPreviewProperties(int _cardId)
        {
            cardId = _cardId;
            card = Player.CardManager.GetCard(cardId);
        }
    }
    public class GiftSkillPreviewUI : AWindowController<GiftSkillPreviewProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private List<SkillGiftItem> skillItemList;

        protected override void AddListeners()
        {
            base.AddListeners();

            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }


        protected override void OnPropertiesSet()
        {
            List<SkillGiftItemData> list = Player.CardManager.GetGiftSkill(Properties.cardId);

            for (var index = 0; index < 4; index++)
            {
                if (list.Count > index)
                {
                    skillItemList[index].SetData(list[index], index);
                    skillItemList[index].gameObject.SetActive(true);
                }
                else
                {
                    skillItemList[index].gameObject.SetActive(false);
                }

            }

        }

        public void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<GiftSkillPreviewUI>();
        }
    }
}