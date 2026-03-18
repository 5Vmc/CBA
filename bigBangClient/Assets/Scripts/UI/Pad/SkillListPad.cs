using Babu;
using BigBang.Animation;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class SkillListPad : MonoBehaviour
    {
        [SerializeField] private SkillGridAdapter skillAdapter;

        [SerializeField] private SelectedSkillPad selectedSkillPad;
        [SerializeField] private SelectedSkillPadAnim selectAnim;
        [SerializeField] private BabuToggleGroup toggleGroup;

        private Skill _selectSkill;
        private List<Skill> _showList;

        private void OnEnable()
        {
            skillAdapter.OnSelect += OnSelect;
            selectedSkillPad.OnUnlock += OnUnlock;

            toggleGroup.OnValueChanged += toggleGroup_OnValueChanged;
        }

        private void toggleGroup_OnValueChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            oldToggle?.GetComponent<StatusControl>().SetStatus(false);
            newToggle?.GetComponent<StatusControl>().SetStatus(true);
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            List<Skill> list = new();
            switch (toggleGroup.EnableIndex)
            {
                case 0:
                    list = Player.CardManager.SkillController.GetSkillList();
                    break;
                case 1:
                    list = Player.CardManager.SkillController.GetSkillList().Where(item => item.Config.Type == SkillType.Atk).ToList();
                    break;
                case 2:
                    list = Player.CardManager.SkillController.GetSkillList().Where(item => item.Config.Type == SkillType.Def).ToList();
                    break;
                case 3:
                    list = Player.CardManager.SkillController.GetSkillList().Where(item => item.Config.Type == SkillType.Assist).ToList();
                    break;
                case 4:
                    list = Player.CardManager.SkillController.GetCanUnlockSkillList();
                    break;


            }
            SetSkillList(list);
        }

        private void OnDisable()
        {
            skillAdapter.OnSelect -= OnSelect;
            selectedSkillPad.OnUnlock -= OnUnlock;
            toggleGroup.OnValueChanged -= toggleGroup_OnValueChanged;
        }

        // 选中的元素
        private void OnSelect(Skill selectSkill)
        {
            selectedSkillPad.SetData(selectSkill);
        }

        private void OnUnlock()
        {
            var skillIcon = skillAdapter.GetSelectItem().GetComponentInChildren<SkillIcon>();
            skillIcon.UpdateStateInfo(true);
            skillIcon.Anim.PlayUnlockAnim();
            selectedSkillPad.skillIcon.UpdateStateInfo(true);
        }

        public void OnShow()
        {
            toggleGroup.Switch(0);

            //默认打开所有
            if (CardUI.isTurnSkillOnce)
            {
                skillAdapter.PlayAnim();
                //学习特技框淡入动画
                selectAnim.PlayEnterAnim();
                CardUI.isTurnSkillOnce = false;
            }


        }

        private void SetSkillList(List<Skill> list)
        {
            //todo: 这里应该自动选择可以解锁的技能。
            _showList = list;
            Skill selectSkill = null;
            if (list.Count > 0)
            {
                selectSkill = list[0];
            }
            skillAdapter.SetData(list, selectSkill);
        }

    }
}