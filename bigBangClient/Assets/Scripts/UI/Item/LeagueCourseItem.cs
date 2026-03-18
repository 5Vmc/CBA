using Babu;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using Utils;
using static BigBang.BattleManager;
using static BigBang.UI.LeagueCourseUI;

namespace BigBang.UI
{
    public class LeagueCourseItem : MonoBehaviour
    {
        [SerializeField] private Image bgDarkImage = null;
        [SerializeField] private Image bgLightImage = null;
        [SerializeField] private TMP_Text dateText = null;
        [SerializeField] private TMP_Text timeText = null;
        [SerializeField] private TMP_Text timeOnlyText = null;
        [SerializeField] private BabuButton detailBtn = null;
        [SerializeField] private TMP_Text homeTeamNameText = null;
        [SerializeField] private TMP_Text awayTeamNameText = null;
        [SerializeField] private ClubIconItem homeIcon = null;
        [SerializeField] private ClubIconItem awayIcon = null;
        [SerializeField] private Image vSImage = null;
        [SerializeField] private RectTransform scorePanel = null;
        [SerializeField] private Image scoreBgImage = null;
        [SerializeField] private TMP_Text colonText = null;
        [SerializeField] private TMP_Text homeScoreText = null;
        [SerializeField] private TMP_Text awayScoreText = null;

        [SerializeField] private Color nameSelf = new Color();
        [SerializeField] private Color nameOther = new Color();
        [SerializeField] private Color scoreWin = new Color();
        [SerializeField] private Color scoreLose = new Color();

        private LeagueCourseItemData leagueCourseData;

        private void OnEnable()
        {
            detailBtn.OnClick += OnClickDetailBtn;
        }

        private void OnDisable()
        {
            detailBtn.OnClick -= OnClickDetailBtn;
        }
        private BattleEnterType battleEnterType;
        private SubUIID subUIID;
        public void SetData(LeagueCourseItemData data, BattleEnterType battleEnterType, SubUIID subUIID = SubUIID.All)
        {
            leagueCourseData = data;
            this.subUIID = subUIID;
            this.battleEnterType = battleEnterType;
            if (data == null) return;
            if (data.HomeTeam == null || data.AwayTeam == null)
            {
                homeTeamNameText.text = string.Empty;
                awayTeamNameText.text = string.Empty;
                vSImage.gameObject.SetActive(false);
                scorePanel.gameObject.SetActive(false);
                homeScoreText.text = string.Empty;
                awayScoreText.text = string.Empty;
                timeText.text = string.Empty;
                dateText.text = string.Empty;
                timeOnlyText.text = string.Empty;
                homeIcon.SetNone();
                awayIcon.SetNone();
                return;
            }
            // 将我的队伍标记为绿色
            homeTeamNameText.color = data.HomeTeam.TeamId == Player.GbId ? nameSelf : nameOther;
            awayTeamNameText.color = data.AwayTeam.TeamId == Player.GbId ? nameSelf : nameOther;
            // 设置主队名称
            homeTeamNameText.text = data.HomeTeam.TeamName;
            // 设置主队图标
            homeIcon.SetIcon(data.HomeTeam.TeamIcon);
            // 设置客队名称
            awayTeamNameText.text = data.AwayTeam.TeamName;
            // 设置客队图标
            awayIcon.SetIcon(data.AwayTeam.TeamIcon);
            // 设置比分
            SetScoreText(data.HomeGoal, data.AwayGoal);
            // 设置比赛时间

            dateText.gameObject.SetActive(subUIID == SubUIID.All);
            timeText.gameObject.SetActive(subUIID == SubUIID.All);
            timeOnlyText.gameObject.SetActive(subUIID == SubUIID.Mine);
            if (subUIID == SubUIID.All)
            {
                dateText.text = TimeUtils.GetUnixTimeString(data.Time, "MM月dd日");
                timeText.text = TimeUtils.GetUnixTimeString(data.Time, "HH:mm");
            }
            if (subUIID == SubUIID.Mine)
            {
                timeOnlyText.text = TimeUtils.GetUnixTimeString(data.Time, "HH:mm");
            }

            bool isGamePlayed = (leagueCourseData.HomeGoal == -1 || leagueCourseData.AwayGoal == -1) == false;
            detailBtn.gameObject.SetActive(isGamePlayed);
        }

        // 设置比赛分数
        public void SetScoreText(int homeGoal, int awayGoal)
        {
            if (homeGoal == -1 || awayGoal == -1)
            {
                vSImage.gameObject.SetActive(true);
                scorePanel.gameObject.SetActive(false);
            }
            else
            {
                vSImage.gameObject.SetActive(false);
                scorePanel.gameObject.SetActive(true);
                homeScoreText.text = homeGoal.ToString();
                awayScoreText.text = awayGoal.ToString();
                homeScoreText.color = homeGoal > awayGoal ? scoreWin : scoreLose;
                awayScoreText.color = homeGoal < awayGoal ? scoreWin : scoreLose;
            }
        }

        // 设置背景颜色
        public void SetBackground(bool isLight)
        {
            bgLightImage.gameObject.SetActive(isLight);
            bgDarkImage.gameObject.SetActive(!isLight);
        }

        public void OnClickDetailBtn(BabuButton sender)
        {
            if (leagueCourseData.HomeGoal == -1 || leagueCourseData.AwayGoal == -1)
            {
                return;
            }
            UIController.Instance.OpenWindow<LeagueTeamDetailUI>(new LeagueTeamDetailUIProperties(leagueCourseData));
        }
    }
}
