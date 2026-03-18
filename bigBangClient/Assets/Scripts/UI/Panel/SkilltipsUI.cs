using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using System;

namespace BigBang.UI
{
    public class SkilltipsUIProperties : WindowProperties
    {
        public Skill Skill { get; set; }
        public SkilltipsUIProperties(Skill skill)
        {
            Skill = skill;

        }
    }

    public class SkilltipsUI : AWindowController<SkilltipsUIProperties>
    {
        [SerializeField] private Image skillImg;
        [SerializeField] private Image qualityImg;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private Button closeBtn;
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

        private void OnClose()
        {
            UIController.Instance.CloseWindow<SkilltipsUI>();
        }

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            nameText.text = Properties.Skill.Config.Name + " Lv." + Properties.Skill.Level;
            descText.text = Properties.Skill.Config.Desc;
            skillImg.sprite = await Properties.Skill.GetIcon();
            SpriteManager.GetSprite(AtlasNames.Skill, "quality_" + Properties.Skill.Config.Quality, (s) => { qualityImg.sprite = s; });
        }
    }
}