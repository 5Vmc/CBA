using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Linq;
using TMPro;
using static BigBang.BattleManager;

namespace BigBang.UI
{
    public class CupUIProperties : PanelProperties
    {
        public int CupID { get; set; }
        public string CupName { get; set; }
        public int CupLevel { get; set; }

        public CupUIProperties(int cupID, string cupName, int cupLevel)
        {
            CupID = cupID;
            CupName = cupName;
            CupLevel = cupLevel;
        }
    }

    public class CupUI : APanelController<CupUIProperties>
    {
        // 杯赛积分榜
        [SerializeField] private CupScoreboardPad cupScoreboardPad;
        // 赛程榜
        [SerializeField] private LeagueCoursePad leagueCoursePad;
        // 球员榜
        [SerializeField] private LeaguePlayerIntegralPad leaguePlayerIntegralPad;

        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle playerToggle;
        [SerializeField] private BabuToggle scheduleToggle;
        [SerializeField] private BabuToggle integralToggle;
        [SerializeField] private TMP_Text cupName;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            playerToggle.OnSelect += OnPlayerSelect;
            playerToggle.OnDeselect += OnPlayerDeselect;
            scheduleToggle.OnSelect += OnCourseSelect;
            scheduleToggle.OnDeselect += OnCourseDeselect;
            integralToggle.OnSelect += OnIntegralSelect;
            integralToggle.OnDeselect += OnIntegralDeselect;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            playerToggle.OnSelect -= OnPlayerSelect;
            playerToggle.OnDeselect -= OnPlayerDeselect;
            scheduleToggle.OnSelect -= OnCourseSelect;
            scheduleToggle.OnDeselect -= OnCourseDeselect;
            integralToggle.OnSelect -= OnIntegralSelect;
            integralToggle.OnDeselect -= OnIntegralDeselect;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            cupName.text = Properties.CupName;

            if (Player.BattleManager.battleEnterType == BattleManager.BattleEnterType.CupUI_Course)
            {
                toggleGroup.Switch(scheduleToggle);
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Unknown;
            }
            else
            {
                // 默认打开积分榜
                toggleGroup.Switch(integralToggle);
                NetworkManager.Instance.GetLeagueCourse(CompitionID.Cup, Properties.CupID, GetLeagueCourseType.All, (System.Action<Protocol.GetLeagueCourseResponse>)(response =>
                {
                    // 打开积分榜
                    cupScoreboardPad.gameObject.SetActive(true);
                    Debug.Log("response.LeagueCourseItemList.Count=" + response.LeagueCourseItemList.Count);
                    var list = response.LeagueCourseItemList.OrderBy(item => item.CourseId);
                    var data32 = list.Where(item => item.Round == 1).ToList();
                    var data16 = list.Where(item => item.Round == 2).ToList();
                    var data8 = list.Where(item => item.Round == 3).ToList();
                    var data4 = list.Where(item => item.Round == 4).ToList();
                    var data2 = list.Where(item => item.Round == 5).ToList();
                    var data1 = list.Where(item => item.Round == 6).ToList();
                    cupScoreboardPad.CupLevel = Properties.CupLevel;
                    var dataProvider = new SpiderMap64Data(data32, data16, data8, data4, data2, data1);
                    cupScoreboardPad.SetData(dataProvider);
                }));
            }
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            UIController.Instance.HidePanel<CupUI>();
        }

        // 打开球员榜
        private void OnPlayerSelect()
        {
            leaguePlayerIntegralPad.gameObject.SetActive(true);
            leaguePlayerIntegralPad.InitAnim();
            NetworkManager.Instance.GetLeagueCardRank(CompitionID.Cup, Properties.CupID, response =>
            {
                leaguePlayerIntegralPad.SetData(response, Properties.CupName);
            });
        }

        // 打开赛程榜
        private void OnCourseSelect()
        {
            leagueCoursePad.gameObject.SetActive(true);
            NetworkManager.Instance.GetLeagueCourse(CompitionID.Cup, Properties.CupID, GetLeagueCourseType.All, response =>
            {
                leagueCoursePad.SetData(response, Properties.CupName, CompitionID.Cup, BattleEnterType.CupUI_Course);
            });
            leagueCoursePad.Anim.Init();
        }

        // 打开积分榜
        private void OnIntegralSelect()
        {
            cupScoreboardPad.gameObject.SetActive(true);
            NetworkManager.Instance.GetLeagueCourse(CompitionID.Cup, Properties.CupID, GetLeagueCourseType.All, (System.Action<Protocol.GetLeagueCourseResponse>)(response =>
            {
                var data32 = response.LeagueCourseItemList.Where(item => item.Round == 1).ToList();
                var data16 = response.LeagueCourseItemList.Where(item => item.Round == 2).ToList();
                var data8 = response.LeagueCourseItemList.Where(item => item.Round == 3).ToList();
                var data4 = response.LeagueCourseItemList.Where(item => item.Round == 4).ToList();
                var data2 = response.LeagueCourseItemList.Where(item => item.Round == 5).ToList();
                var data1 = response.LeagueCourseItemList.Where(item => item.Round == 6).ToList();
                var dataProvider = new SpiderMap64Data(data32, data16, data8, data4, data2, data1);
                cupScoreboardPad.SetData(dataProvider);
            }));
            Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.CupUI_Integral;
        }

        // 关闭球员榜
        private void OnPlayerDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            leaguePlayerIntegralPad.gameObject.SetActive(false);
        }

        // 关闭赛程榜
        private void OnCourseDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            leagueCoursePad.gameObject.SetActive(false);
        }

        // 关闭积分榜
        private void OnIntegralDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            cupScoreboardPad.gameObject.SetActive(false);
        }
    }
}