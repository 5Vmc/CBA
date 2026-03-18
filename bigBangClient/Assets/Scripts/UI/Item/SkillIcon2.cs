using BigBang.Animation;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Coffee.UIEffects;
using TMPro;
using UnityTimer;

namespace BigBang.UI
{
    public class SkillIcon2 : SkillIconBase
    {
        [SerializeField] private TMP_Text levelText;
       // [SerializeField] private TMP_Text nameText;
       // [SerializeField] private TMP_Text descText;


        private Skill _skill;

        public SkillIconAnim Anim;

        public void SetData(Skill skill)
        {
           
            _skill = skill;
            base.SetData(_skill.Config);
            skillImg.gameObject.SetActive(true);

            descText.text = _skill.Config.Desc;

            levelText.text = $"Lv. {skill.Level}";

        }


       
    }
}