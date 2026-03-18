using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;

namespace BigBang.UI
{
    public class LeagueIntroductionUIProperties : WindowProperties
    {
        public int CompitionID;
        public LeagueIntroductionUIProperties(int compitionID)
        {
            CompitionID = compitionID;
        }
    }

    public class LeagueIntroductionUI : AWindowController<LeagueIntroductionUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text titleText1;
        [SerializeField] private TMP_Text titleText2;
        [SerializeField] private TMP_Text titleText3;
        [SerializeField] private TMP_Text titleText4;
        [SerializeField] private TMP_Text titleText5;
        [SerializeField] private TMP_Text leagueFormat;
        [SerializeField] private TMP_Text rankingRules;
        [SerializeField] private TMP_Text upAndDownRule;
        [SerializeField] private TMP_Text championshipQualification;
        [SerializeField] private TMP_Text suspensionRules;

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

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
            string title = string.Empty;
            string title1 = string.Empty;
            string title2 = string.Empty;
            string title3 = string.Empty;
            string title4 = string.Empty;
            string title5 = string.Empty;
            string format = string.Empty;
            string ranking = string.Empty;
            string upAndDown = string.Empty;
            string championship = string.Empty;
            string suspension = string.Empty;
            if (Properties.CompitionID == CompitionID.League)
            {
                title = Lang.Get(LangID.LeagueNameText) + Lang.Get(LangID.IntroduceText);
                title1 = Lang.Get(LangID.LeagueIntroduceTitle1);
                title2 = Lang.Get(LangID.LeagueIntroduceTitle2);
                title3 = Lang.Get(LangID.LeagueIntroduceTitle3);
                title4 = Lang.Get(LangID.LeagueIntroduceTitle4);
                title5 = Lang.Get(LangID.LeagueIntroduceTitle5);
                format = Lang.Get(LangID.LeagueIntroduceValue1);
                ranking = Lang.Get(LangID.LeagueIntroduceValue2);
                upAndDown = Lang.Get(LangID.LeagueIntroduceValue3);
                championship = Lang.Get(LangID.LeagueIntroduceValue4);
                suspension = Lang.Get(LangID.LeagueIntroduceValue5);
            }
            if (Properties.CompitionID == CompitionID.Cup)
            {
                title = Lang.Get(LangID.CupNameText) + Lang.Get(LangID.IntroduceText);
                title1 = Lang.Get(LangID.CupIntroduceTitle1);
                title2 = Lang.Get(LangID.CupIntroduceTitle2);
                title3 = Lang.Get(LangID.CupIntroduceTitle3);
                title4 = Lang.Get(LangID.CupIntroduceTitle4);
                title5 = Lang.Get(LangID.CupIntroduceTitle5);
                format = Lang.Get(LangID.CupIntroduceValue1);
                ranking = Lang.Get(LangID.CupIntroduceValue2);
                upAndDown = Lang.Get(LangID.CupIntroduceValue3);
                championship = Lang.Get(LangID.CupIntroduceValue4);
                suspension = Lang.Get(LangID.CupIntroduceValue5);
            }

            titleText.text = title;
            titleText1.text = title1;
            titleText2.text = title2;
            titleText3.text = title3;
            titleText4.text = title4;
            titleText5.text = title5;
            leagueFormat.text = format;
            rankingRules.text = ranking;
            upAndDownRule.text = upAndDown;
            championshipQualification.text = championship;
            suspensionRules.text = suspension;
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<LeagueIntroductionUI>();
        }
    }
}