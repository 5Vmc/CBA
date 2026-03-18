
using deVoid.UIFramework;

using UnityEngine;
using UnityEngine.UI;

//using System.Collections;

using BigBang.Animation;
using Google.Protobuf.WellKnownTypes;
using Babu;

namespace BigBang.UI
{
    public class ArenaUIProperties : PanelProperties
    {
        public ArenaUI.SubUIID SubUI = ArenaUI.SubUIID.Arena;
        public bool isNeedOpenGuide2UI = false;
        public ArenaUIProperties(ArenaUI.SubUIID ui = ArenaUI.SubUIID.Arena, bool isNeedOpenGuide2UI = false)
        {
            this.isNeedOpenGuide2UI = isNeedOpenGuide2UI;
            SubUI = ui;
        }
    }

    public class ArenaUI : APanelController<ArenaUIProperties>
    {
        public enum SubUIID
        {
            Unknow = 0,
            Arena = 1,  //竞技场
            Tactic = 2, //阵型
            First = 3,  //首发
        }

        [SerializeField] private Button closeBtn;

        [SerializeField] private Toggle arenaToggleBtn;
        [SerializeField] private Toggle tacticToggleBtn;
        [SerializeField] private Toggle firstToggleBtn;

        [SerializeField] private ToggleGroupSelecter _padSelecter;

        [SerializeField] private ArenaPad arenaPad;
        [SerializeField] private TacticsSetPad tacticsSetPad;
        [SerializeField] private FormationPad formationPad;

        private ArenaUIAnim Anim;
        protected override void Awake()
        {
            this.Anim = GetComponent<ArenaUIAnim>();
        }
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            _padSelecter.onValueChange.AddListener(SelectOne);
            EventManager.Instance.Register(EventID.OnClickArenaPadGotoFormationPad, OnClickArenaPadGotoFormationPad);
        }
        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            _padSelecter.onValueChange.RemoveListener(SelectOne);
            EventManager.Instance.Unregister(EventID.OnClickArenaPadGotoFormationPad, OnClickArenaPadGotoFormationPad);
        }
        private bool isNeedOpenGuide2UI = false;
        protected override void OnPropertiesSet()
        {
            SubUIID subUIID = SubUIID.Unknow;
            if (Properties == null || Properties.SubUI == SubUIID.Unknow)
            {
                subUIID = SubUIID.Arena;
            }
            else
            {
                subUIID = Properties.SubUI;
                Properties.SubUI = SubUIID.Unknow;
            }
            if (Properties == null)
            {
                isNeedOpenGuide2UI = false;
            }
            else
            {
                isNeedOpenGuide2UI = Properties.isNeedOpenGuide2UI;
            }
            _padSelecter.SetValueSelected((int)subUIID);
            ShowPad(subUIID);
        }
        private void OnClose()
        {
            if (_lastShowType == SubUIID.First) formationPad.SaveToServer();
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACKBG);
            UIController.Instance.HidePanel<ArenaUI>();
        }

        private SubUIID _lastShowType = SubUIID.Arena;
        private void SelectOne(int value)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            if (_lastShowType == SubUIID.First)
            {
                formationPad.GetComponent<AnimOut>().Play(() =>
                {
                    ShowPad((SubUIID)value);
                });
            }
            else
            {
                ShowPad((SubUIID)value);
            }
            switch ((SubUIID)value)
            {
                case SubUIID.Arena:
                    CbaLogManager.Instance.AddLog(1056);
                    break;
                case SubUIID.Tactic:
                    CbaLogManager.Instance.AddLog(1057);
                    break;
                case SubUIID.First:
                    CbaLogManager.Instance.AddLog(1058);
                    break;
            }
        }
        private void ShowPad(SubUIID value)
        {
            Debug.Log(value);
            _lastShowType = value;
            arenaPad.gameObject.SetActive(value == SubUIID.Arena);
            tacticsSetPad.gameObject.SetActive(value == SubUIID.Tactic);
            formationPad.gameObject.SetActive(value == SubUIID.First);
            switch (value)
            {
                case SubUIID.Arena:
                    arenaPad.OnShow(isNeedOpenGuide2UI);
                    break;
                case SubUIID.Tactic:
                    tacticsSetPad.OnShow();
                    break;
                case SubUIID.First:
                    formationPad.OnShowArena();
                    break;
            }
            if (value != SubUIID.First) formationPad.SaveToServer();
        }
        private void OnClickArenaPadGotoFormationPad(object[] _)
        {
            _padSelecter.SetValueSelected((int)SubUIID.First);
            ShowPad(SubUIID.First);
        }

    }
}