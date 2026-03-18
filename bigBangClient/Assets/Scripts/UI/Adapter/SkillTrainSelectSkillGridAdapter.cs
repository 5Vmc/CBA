using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class SkillTrainSelectSkillGridAdapter : GridAdapter<GridParams, SkillTrainSelectSkillGridViewsHolder>
    {
        public SimpleDataHelper<Skill> Data { get; private set; }

        private List<Skill> _skillList;

        private int _selectSkillId;
        private int _selectCardId;
        private Action<Skill> _selectAction;
        // private Func<SkillConfig, bool> _checkAction;

        protected override void Awake()
        {
            base.Awake();

            Data = new SimpleDataHelper<Skill>(this);
        }

        protected override void Start()
        {
            base.Start();
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<SkillTrainSelectSkillGridViewsHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetData(List<Skill> cardList, int selectSkillId = 0, int selectCardId = 0)
        {
            if (!IsInitialized)
            {
                Init();
            }
            _skillList = cardList;
            _selectSkillId = selectSkillId;
            _selectCardId = selectCardId;
            cardList ??= new List<Skill>();

            Data.ResetItems(cardList);
        }

        protected override void UpdateCellViewsHolder(SkillTrainSelectSkillGridViewsHolder viewsHolder)
        {
            var model = Data[viewsHolder.ItemIndex];
            var isSelect = _selectSkillId != 0 && _selectSkillId == model.Id;
            var showState = GetShowStateType(model.Id);
            viewsHolder.UpdateViews(model, isSelect, showState, SelectSkill);
        }

        private void SelectSkill(Skill selectSkill)
        {
            _selectSkillId = selectSkill.Config.Id;
            Refresh();
            _selectAction(selectSkill);
        }

        public void SelectActionRegister(Action<Skill> selectAction)
        {
            _selectAction = selectAction;
        }

        private SkillTrainSelectSkillState GetShowStateType(int skillId)
        {
            return Player.CardManager.SkillController.GetSelectSkillShowState(skillId, _selectCardId);
        }
    }

    public class SkillTrainSelectSkillGridViewsHolder : CellViewsHolder
    {
        //点击按钮
        private Button btn;
        private Skill _skill;
        private SkillSelectIcon skillIcon;
        private SkillTrainSelectSkillState _showState;

        private Action<Skill> _selectAction;

        public override void CollectViews()
        {
            base.CollectViews();
            skillIcon = root.GetComponent<SkillSelectIcon>();

            btn = root.GetComponent<Button>();
            btn.onClick.AddListener(OnSelect);
        }

        private void OnSelect()
        {
            switch (_showState)
            {
                case SkillTrainSelectSkillState.Normal:
                    PlayMoveAnim(() => _selectAction(_skill));
                    break;
                case SkillTrainSelectSkillState.DoTraining:
                    Tips.PopError(ErrorID.SkillTrainSelectSkillDoTraining);
                    break;
                case SkillTrainSelectSkillState.CanNotTrain:
                    Tips.PopError(ErrorID.SkillTrainSelectSkillCanNotTrain);
                    break;
                case SkillTrainSelectSkillState.HaveBeenTrain:
                    Tips.PopError(ErrorID.SkillTrainSelectSkillHaveBeenTrain);
                    break;
            }
        }

        // 技能卡牌移动到技能空位
        private void PlayMoveAnim(Action callback)
        {
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.BTN_SELECT_LOAD);
            // 显示勾勾
            skillIcon.ShowSelectAnim();

            var skillRect = skillIcon.SkillImgRect;
            var skillClone = SkillTrainRoomSelectUI.SkillCloneStatic;
            var targetPos = SkillTrainRoomSelectUI.SkillTargetPosStatic.position;
            // 初始化起始位置
            skillClone.position = skillRect.position;
            // 初始化起始缩放
            skillClone.localScale = Vector3.one * 0.8f;
            // 启用组件
            skillClone.gameObject.SetActive(true);
            // 设置技能图片
            skillClone.GetComponent<Image>().sprite = skillRect.GetComponent<Image>().sprite;
            skillClone.DOMove(targetPos, 0.15f).OnComplete(() =>
            {
                TouchManager.Instance.EnableTouch();
                
                skillClone.gameObject.SetActive(false);
                callback?.Invoke();
            });
            skillClone.DOScale(1, 0.15f);
        }

        public void UpdateViews(Skill skill, bool isSelect, SkillTrainSelectSkillState showState, Action<Skill> selectAction)
        {
            _skill = skill;
            _selectAction = selectAction;
            _showState = showState;

            skillIcon.SetData(skill.Config);
            skillIcon.SetState(showState);

            skillIcon.SetSelect(isSelect);
        }
    }
}