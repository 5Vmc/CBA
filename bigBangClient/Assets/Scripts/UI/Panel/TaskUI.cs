using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using System.Linq;
using BigBang.Animation;
using Utils;
using System.Collections.Generic;
using Babu;
using System;
using Babu.Client.Fsm;

namespace BigBang.UI
{
    public class TaskUIProperties : PanelProperties
    {
        public TaskUI.SubUIID SubUI = TaskUI.SubUIID.Daily;
        public bool isNew = true;
        public TaskUIProperties(TaskUI.SubUIID ui)
        {
            SubUI = ui;
        }
    }
    public class TaskUI : APanelController<TaskUIProperties>
    {
        public enum SubUIID
        {
            Daily = 0,
            Weekly = 1,
            Bounty = 2
        }

        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private BabuToggleGroup bottomToggleGroup;
        [SerializeField] private BabuToggle weekToggle;
        [SerializeField] private BabuToggle dayToggle;
        [SerializeField] private BabuToggle bountyToggle;

        [SerializeField] public TaskUIAnim Anim;
        [SerializeField] public DailyTaskPad dailyTaskPad;
        [SerializeField] public BountyTaskPad bountyTaskPad;
        [SerializeField] public List<Transform> RedDotList;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnColse;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.OnTeamlevelUp, OnLevelUp);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnColse;
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Unregister(EventID.OnTeamlevelUp, OnLevelUp);
        }

        bool isNew = true;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            if (isNew) Properties.isNew = true;
            isNew = false;
            if (Properties != null && Properties.isNew == false)
            {
                //oldPadIndex = -1;
                //bottomToggleGroup.Switch((int)Properties.SubUI);
                int selectedIndex = bottomToggleGroup.EnableIndex;
                changePadAni2(selectedIndex, false);
                Anim.PlayEnter();
            }
            else
            {
                Properties.isNew = false;
                if (Properties == null)
                {
                    oldPadIndex = -1;
                    bottomToggleGroup.Switch(0);
                }
                else
                {
                    oldPadIndex = -1;
                    bottomToggleGroup.Switch((int)Properties.SubUI);
                }
                Anim.PlayEnter();
            }

            OnLevelUp(null);


            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
        }

        private void OnLevelUp(object[] args)
        {
            var isOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Task_Bounty, false);
            bountyToggle.gameObject.SetActive(isOpen);
        }


        private int oldPadIndex = -1;
        private void changePadAni1(int padIndex, Action<int, bool> callback)
        {
            TouchManager.Instance.DisableTouch();
            if (oldPadIndex == 0 || oldPadIndex == 1)
            {
                if (padIndex == 0 || padIndex == 1)
                {
                    dailyTaskPad.Anim.PlayFade(() =>
                    {
                        dailyTaskPad.gameObject.SetActive(false);
                        callback.Invoke(padIndex, true);
                    });
                }
                else
                {
                    dailyTaskPad.Anim.PlayExit(() =>
                    {
                        dailyTaskPad.gameObject.SetActive(false);
                        callback.Invoke(padIndex, true);
                    });
                }
            }
            else if (oldPadIndex == 2)
            {
                bountyTaskPad.Anim.PlayExit(() =>
                {
                    bountyTaskPad.gameObject.SetActive(false);
                    callback.Invoke(padIndex, true);
                });
            }
            else
            {
                dailyTaskPad.gameObject.SetActive(false);
                bountyTaskPad.gameObject.SetActive(false);
                callback.Invoke(padIndex, true);
            }
            oldPadIndex = padIndex;
        }
        private void changePadAni2(int padIndex, bool ani = true)
        {
            switch (padIndex)
            {
                case 0:
                    dailyTaskPad.gameObject.SetActive(true);
                    dailyTaskPad.OnDaySelect();
                    break;
                case 1:
                    dailyTaskPad.gameObject.SetActive(true);
                    dailyTaskPad.OnWeekSelect();
                    break;
                case 2:
                    bountyTaskPad.gameObject.SetActive(true);
                    bountyTaskPad.OnBountySelect(ani);
                    break;
                default:
                    break;
            }
            TouchManager.Instance.EnableTouch();
        }
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            switch (bottomToggleGroup.EnableIndex)
            {
                case 2:
                    var isOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Task_Bounty, true);
                    if (!isOpen)
                    {
                        //oldToggle?.GetComponent<StatusControl>().SetStatus(false);
                        //return;
                        bottomToggleGroup.Switch(oldToggle);
                        newToggle?.GetComponent<StatusControl>().SetStatus(false);
                        return;
                    }
                    break;
            }

            TouchManager.Instance.DisableTouch();
            int selectedIndex = bottomToggleGroup.EnableIndex;
            changePadAni1(selectedIndex, changePadAni2);

            RefreshRedDot();
        }

        public void RefreshRedDot(object[] args = null)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Task, "/2");
            node.IsRed(RedDotList[0]);
            node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Task, "/3");
            node.IsRed(RedDotList[1]);
            node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Task, "/bounty");
            node.IsRed(RedDotList[2]);
        }

        private void OnColse(BabuButton sender)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACKBG);
            TouchManager.Instance.DisableTouch();
            Anim.PlayExit(() =>
            {
                TouchManager.Instance.EnableTouch();

                FsmManager.Instance.ChangeToState<StateHome>(new StateCommonUserData()
                {
                    OpenUIAction = async () =>
                    {
                        UIController.Instance.HidePanel<TaskUI>();
                        //await UIController.Instance.ShowPanel<HomeUI>();
                    }
                });
            });
        }
    }
}