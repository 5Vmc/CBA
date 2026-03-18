using System;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    [System.Serializable]
    public class FormationProperties : PanelProperties
    {

        public int formationShowType = FormationUI.FormationShowType.LastPad;

        public FormationBase Formation { get; set; }
        public string lastScreenId;
        public bool Navigation;
        public int formationId;

        #region 仅悬赏任务使用
        public Action<FormationBase, bool> afterFormationCallBack;
        public int[] limitIntArr;
        #endregion

        public FormationProperties(FormationBase formation, bool navigation, int formationShowType = FormationUI.FormationShowType.LastPad, int formationId = FormationID.PVE, Action<FormationBase, bool> afterFormationCallBack = null, int[] limitIntArr = null)
        {
            Formation = formation;
            lastScreenId = UIController.Instance.GetCurrentShowPanelName();
            Navigation = navigation;
            Navigation = false;
            this.formationShowType = formationShowType;
            this.formationId = formationId;
            this.afterFormationCallBack = afterFormationCallBack;
            this.limitIntArr = limitIntArr;
        }
    }

    public class FormationUI : APanelController<FormationProperties>
    {
        public class FormationShowType
        {
            public const int LastPad = 0;
            public const int Formation = 1;
            public const int Tactics = 2;
            public const int LineUp = 3;
        }

        [SerializeField] private Button closeBtn;

        [SerializeField] private TacticsSetPad tacticsSetPad;
        [SerializeField] private LineupPad lineupPad;
        [SerializeField] private FormationPad formationPad;
        [SerializeField] private ToggleGroupSelecter _padSelecter;
        [SerializeField] private GameObject bottom;
        //[SerializeField] private NavigationPad navigationPad;

        [SerializeField] private RectTransform bottomItem;
        [SerializeField] private Toggle StartToggleBtn;
        [SerializeField] private Toggle TacticToggleBtn;
        [SerializeField] private StatusControl StartStatus;
        [SerializeField] private StatusControl TacticStatus;

        private FormationBase _formation;

        private int _lastShowType = FormationShowType.Formation;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            _padSelecter.onValueChange.AddListener(SelectOne);
            StartToggleBtn.onValueChanged.AddListener(OnStartToggleBtn);
            TacticToggleBtn.onValueChanged.AddListener(OnTacticToggleBtn);

            EventManager.Instance.Register(EventID.OnLineupChangeSelectModel, OnLineupChangeSelectModel);
        }

        protected override void RemoveListeners()
        {
            if (bottom.activeSelf == false)
            {
                Player.FightManager.FormationController.SaveFormationToServer(_formation);
            }
            closeBtn.onClick.RemoveListener(OnClose);
            _padSelecter.onValueChange.RemoveListener(SelectOne);
            StartToggleBtn.onValueChanged.RemoveListener(OnStartToggleBtn);
            TacticToggleBtn.onValueChanged.RemoveListener(OnTacticToggleBtn);

            EventManager.Instance.Unregister(EventID.OnLineupChangeSelectModel, OnLineupChangeSelectModel);
        }

        protected override void OnPropertiesSet()
        {
            if (Properties.formationId == FormationID.Bounty)
            {
                ShowBounty();
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                if (i == 0) _padSelecter.ToggleList[i].gameObject.SetActive(true);
                if (i == 1) _padSelecter.ToggleList[i].gameObject.SetActive(true);
                if (i == 2) _padSelecter.ToggleList[i].gameObject.SetActive(false);
            }
            _formation = Properties.Formation;

            if (Properties.formationShowType != FormationShowType.LastPad)
            {
                _lastShowType = Properties.formationShowType;
            }
            switch (_lastShowType)
            {
                case FormationShowType.Formation:
                    _lastShowType = FormationShowType.Formation;
                    break;
                case FormationShowType.Tactics:
                    _lastShowType = FormationShowType.Tactics;
                    break;
                case FormationShowType.LineUp:
                    _lastShowType = FormationShowType.LineUp;
                    break;
            }
            _padSelecter.SetValueSelected(_lastShowType);

            PlayEnter();
            //navigationPad.gameObject.SetActive(Properties.Navigation);
            bottomItem.gameObject.SetActive(Properties.Navigation);
            bottom.gameObject.SetActive(!Properties.Navigation);
            if (Properties.Navigation && _lastShowType == FormationShowType.LineUp)
            {
                _lastShowType = FormationShowType.Formation;
            }
            switch (_lastShowType)
            {
                case FormationShowType.Formation:
                    StartToggleBtn.isOn = true;
                    StartStatus.SetStatus(true);
                    TacticStatus.SetStatus(false);
                    ShowPad(FormationShowType.Formation);
                    break;
                case FormationShowType.Tactics:
                    TacticToggleBtn.isOn = true;
                    StartStatus.SetStatus(false);
                    TacticStatus.SetStatus(true);
                    ShowPad(FormationShowType.Tactics);
                    break;
                case FormationShowType.LineUp:
                    ShowPad(_lastShowType);
                    break;
            }
            if (Properties.Navigation) bottomItem.DoRelativeAnchorPosY(-100, 0.25f).From().AddTo(this.gameObject);
        }
        private void ShowBounty()
        {
            for (int i = 0; i < 3; i++)
            {
                if (i == 0) _padSelecter.ToggleList[i].gameObject.SetActive(true);
                if (i == 1) _padSelecter.ToggleList[i].gameObject.SetActive(false);
                if (i == 2) _padSelecter.ToggleList[i].gameObject.SetActive(false);
            }
            _formation = Properties.Formation;
            _padSelecter.SetValueSelected(FormationShowType.Formation);
            PlayEnter();
            //navigationPad.gameObject.SetActive(false);
            bottomItem.gameObject.SetActive(false);
            bottom.gameObject.SetActive(true);
            StartToggleBtn.isOn = true;
            StartStatus.SetStatus(true);
            TacticStatus.SetStatus(false);
            ShowPad(FormationShowType.Formation);
            if (Properties.Navigation) bottomItem.DoRelativeAnchorPosY(-100, 0.25f).From().AddTo(this.gameObject);
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            Player.FightManager.FormationController.SaveFormationToServer(_formation);
            formationPad.CheckBackupClose();
            UIController.Instance.HidePanel<FormationUI>();
        }

        private void OnStartToggleBtn(bool flag)
        {
            if (flag == false) return;
            if (formationPad.gameObject.activeSelf == true) return;
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            //StartStatus.SetStatus(true);
            //TacticStatus.SetStatus(false);
            ShowPad(FormationShowType.Formation);
        }
        private void OnTacticToggleBtn(bool flag)
        {
            if (flag == false) return;
            if (tacticsSetPad.gameObject.activeSelf == true) return;
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            //StartStatus.SetStatus(false);
            //TacticStatus.SetStatus(true);
            ShowPad(FormationShowType.Tactics);
        }

        private void OnLineupChangeSelectModel(object[] args)
        {
            var flag = (bool)args[0];

            bottom.SetActive(!flag);
        }

        private void PlayEnter()
        {
            bottom.GetComponent<RectTransform>().DoRelativeAnchorPosY(-200, 0.3f).From().AddTo(this.gameObject);
        }

        private Action<FormationBase> afterFormationCallBack;
        private void ShowPad(int value)
        {
            _lastShowType = value;
            formationPad.gameObject.SetActive(value == FormationShowType.Formation);
            tacticsSetPad.gameObject.SetActive(value == FormationShowType.Tactics);
            lineupPad.gameObject.SetActive(value == FormationShowType.LineUp);
            switch (value)
            {
                case FormationShowType.Formation:
                    switch (Properties.formationId)
                    {
                        case FormationID.PVE: formationPad.OnShowPVEandPVP(_formation); break;
                        case FormationID.PVP: formationPad.OnShowPVEandPVP(_formation); break;
                        case FormationID.HERO: formationPad.OnShowHero(_formation); break;
                        case FormationID.Bounty: formationPad.OnShowBounty(_formation, Properties.limitIntArr, Properties.afterFormationCallBack); break;
                        case FormationID.TOWER: formationPad.OnShowTower(_formation); break;
                    }
                    formationPad.ResetTeamTotalCombat();
                    break;
                case FormationShowType.Tactics:
                    tacticsSetPad.OnShow(_formation);
                    break;
                case FormationShowType.LineUp:
                    lineupPad.OnShow(_formation);
                    break;
            }
        }

        private void SelectOne(int value)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            if (_lastShowType == FormationShowType.Formation)
            {
                formationPad.GetComponent<AnimOut>().Play(() =>
                {
                    ShowPad(value);
                });
            }
            else
            {
                ShowPad(value);
            }
        }
    }
}
