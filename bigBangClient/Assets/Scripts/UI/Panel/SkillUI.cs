using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using UnityTimer;
using Babu;

namespace BigBang.UI
{
    public class SkillUI : APanelController
    {
        private class ShowPadType
        {
            public const int SkillListPad = 0;
            public const int SkillTrainRoomPad = 1;
        }

        [SerializeField] private Button closeBtn;

        [SerializeField] private SkillListPad skillListPad;
        [SerializeField] private SkillTrainRoomPad skillTrainRoomPad;
        [SerializeField] private Image dotNodeImgUnlockSkill = null;
        [SerializeField] private Image dotNodeImgTrainRoom = null;
        [SerializeField] private BabuToggleGroup bottomToggleGroup = null;

        private Timer timer;

        public SkillUIAnim Anim;

        public class RoomInfo
        {
            public int CardID;
            public int SkillID;

            public RoomInfo(int cardID, int skillID)
            {
                CardID = cardID;
                SkillID = skillID;
            }
        }

        public static List<RoomInfo> roomInfo = new List<RoomInfo>();

        protected override void Awake()
        {
            base.Awake();
            timer = Timer.Register(this.gameObject, 0.1f, Check, isLooped: true, autoDestroyOwner: this);
        }

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);

            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            EventManager.Instance.Register(EventID.OnStudySkill, SwitchToLearn);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            timer.Resume();
        }
        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
            EventManager.Instance.Unregister(EventID.OnStudySkill, SwitchToLearn);
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            timer.Pause();
        }

        private void SwitchToLearn(object[] args)
        {
            bottomToggleGroup.Switch((int)ShowPadType.SkillTrainRoomPad);
            ShowPad(ShowPadType.SkillTrainRoomPad);
        }

        private void RefreshRedDot(object[] args)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_SkillTrain, "/UnlockSkill");
            node.IsRed(dotNodeImgUnlockSkill.transform);
            node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_SkillTrain, "/TrainRoom");
            node.IsRed(dotNodeImgTrainRoom.transform);
        }

        private void Check()
        {
            if (roomInfo.Count > 0 && skillTrainRoomPad.gameObject.activeInHierarchy)
            {
                var cardID = roomInfo[0].CardID;
                var skillID = roomInfo[0].SkillID;
                UIController.Instance.OpenWindow<SkillResultUI>(new SkillResultUIProperties(cardID, skillID));
                roomInfo.RemoveAt(0);
            }
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            //默认打开所有

            bottomToggleGroup.Switch((int)ShowPadType.SkillListPad);
            Anim.PlayEnter();
            Player.CardManager.SkillController.CheckRedDot();
            RefreshRedDot(null);
        }

        private void OnClose()
        {
            UIController.Instance.HidePanel<SkillUI>();
        }

        public void ShowRoom(Action callback)
        {
            Anim.PlayExit(() =>
            {
                bottomToggleGroup.Switch((int)ShowPadType.SkillTrainRoomPad);
                skillTrainRoomPad.gameObject.SetActive(true);
                skillListPad.gameObject.SetActive(false);
                skillTrainRoomPad.gameObject.SetAlpha(1);
                skillTrainRoomPad.SetData();
                Babu.DelayTaskService.Instance.Run(this.gameObject, 0.15f, () => callback?.Invoke());
            });
        }

        public void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            Debug.Log($"select value = {selectedIndex}");
            ShowPad(selectedIndex);
        }

        private void ShowPad(int type)
        {
            skillListPad.gameObject.SetActive(type == ShowPadType.SkillListPad);
            skillTrainRoomPad.gameObject.SetActive(type == ShowPadType.SkillTrainRoomPad);
            switch (type)
            {
                case ShowPadType.SkillListPad:
                    skillListPad.OnShow();
                    return;
                case ShowPadType.SkillTrainRoomPad:
                    Anim.PlayEnterII();
                    skillTrainRoomPad.SetData();
                    return;
                default:
                    return;
            }
        }
    }
}