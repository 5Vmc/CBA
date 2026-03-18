using System;
using System.Collections.Generic;
using Babu;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class SkillGridAdapter : GridAdapter<SkillInfoParams, SkillGridViewsHolder>
    {
        public SimpleDataHelper<Skill> Data { get; private set; }

        private List<Skill> _skillList;
        private Skill _selectSkill;

        public event Action<Skill> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<Skill>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<SkillGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        protected override void OnEnable()
        {
            base.OnEnable();
            AddListeners();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            RemoveListeners();
        }

        private void AddListeners()
        {
            // 监听元素选中事件
            EventManager.Instance.Register(EventID.OnSkillUISelectSkill, SelectOneSkill);
        }

        private void RemoveListeners()
        {
            EventManager.Instance.Unregister(EventID.OnSkillUISelectSkill, SelectOneSkill);
        }

        // 元素选中事件
        public void SelectOneSkill(object[] args)
        {
            var skillId = (int)args[0];
            var skill = Player.CardManager.SkillController.GetSkill(skillId);
            if (skill == null) return;
            _selectSkill = skill;
            
            OnSelect?.Invoke(_selectSkill);
        }

        // 获得选中的元素
        public GameObject GetSelectItem()
        {
            // 自上而下依次淡入
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);
                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    if (item.Skill == _selectSkill)
                    {
                        return item.root.gameObject;
                    }
                }
            }
            return null;
        }

        public void SetData(List<Skill> skillList, Skill skill = null)
        {
            if (!IsInitialized)
            {
                Init();
            }
            _skillList = skillList;
            _selectSkill = skill;
            skillList ??= new List<Skill>();

            Data.ResetItems(skillList);
            OnSelect?.Invoke(_selectSkill);
        }

        public void PlayAnim()
        {
            // 自上而下依次淡入
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);
                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    item?.PlayEnter((i + 1) * 0.1f);
                }
            }
        }

        protected override void UpdateCellViewsHolder(SkillGridViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, _selectSkill);
        }
    }

    [System.Serializable]
    public class SkillInfoParams : GridParams
    {
    }

    public class SkillGridViewsHolder : CellViewsHolder
    {
        //点击按钮
        private Button btn;
        public Skill Skill;
        private SkillIcon _skillIcon;

        public override void CollectViews()
        {
            base.CollectViews();
            _skillIcon = root.GetComponent<SkillIcon>();

            btn = root.GetComponent<Button>();
            btn.onClick.AddListener(OnClickSkill);
        }

        private void OnClickSkill()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            // 触发元素选中事件
            EventManager.Instance.Dispatch(EventID.OnSkillUISelectSkill, Skill.Id);
        }

        public void UpdateViews(Skill skill, Skill selectSkill)
        {
            this.Skill = skill;
            if (this.Skill == null) return;
            _skillIcon.SetData(this.Skill, false, true, 1);
            if (selectSkill != null && this.Skill.Id == selectSkill.Id)
            {
                _skillIcon.Anim.ShowBorder();
            }
            else
            {
                _skillIcon.Anim.HidBorder();
            }
        }

        public void PlayEnter(float delay)
        {
            _skillIcon.Anim.PlayEnter(delay);
        }

        public Skill GetSkillData()
        {
            return Skill;
        }
    }
}