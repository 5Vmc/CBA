using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using BigBang.Animation;
using Utils;
using GameConfig;

namespace BigBang.UI
{
    public class SkillResultUIProperties : WindowProperties
    {
        public int CardID;
        public int SkillID;

        public SkillResultUIProperties(int cardID, int skillID)
        {
            CardID = cardID;
            SkillID = skillID;
        }
    }

    public class SkillResultUI : AWindowController<SkillResultUIProperties>
    {
        [SerializeField] private Image playerImg;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Button closeBtn;
        [SerializeField] private SkillResultUIAnim Anim;

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

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            var cardCfg = Configs.CardModel.GetConfig(Properties.CardID);
            var skillCfg = Configs.Skill.GetConfig(Properties.SkillID);
            var buffCfg = Configs.SkillAddition.GetConfig(skillCfg.BuffType);
            // 设置球员头像
            playerImg.sprite = await SpriteProxy.GetPlayerPortrait(cardCfg.Portrait);
            descText.text = Lang.Get(LangID.MasterskillText).Replace("{playerName}", PlayerCard.GetFullName(cardCfg)).Replace("{skillName}", skillCfg.Name);
            valueText.text = buffCfg.Name.Replace("{value}", $"<color=#0EDE35>{skillCfg.BuffValue[0]}</color>");
            Anim.PlayEnter();
        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<SkillResultUI>();
        }
    }
}