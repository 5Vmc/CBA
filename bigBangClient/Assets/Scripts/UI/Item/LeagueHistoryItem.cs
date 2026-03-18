using Babu;
using GameConfig;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.BattleManager;
using static BigBang.UI.LeagueCourseUI;

namespace BigBang.UI
{
    public class LeagueHistoryItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text leagueLevelText = null;
        [SerializeField] private TMP_Text dateText = null;
        [SerializeField] private TMP_Text teamNameText = null;
        [SerializeField] private ClubIconItem teamIcon = null;
        [SerializeField] private Image cupImage1 = null;
        [SerializeField] private Image cupImage2 = null;
        [SerializeField] private Image cupImage3 = null;
        [SerializeField] private TMP_Text rankNumText = null;
        [SerializeField] private TMP_Text winLoseText = null;
        [SerializeField] private TMP_Text downText = null;
        [SerializeField] private ImageFont upImageFont = null;
        [SerializeField] private BabuButton detailBtn = null;

        [SerializeField] private Color rank1Color = new Color();
        [SerializeField] private Color rank2Color = new Color();
        [SerializeField] private Color rank3Color = new Color();
        [SerializeField] private Color rankOtherColor = new Color();

        private LeagueHistoryData data;

        private void OnEnable()
        {
            detailBtn.OnClick += OnClickDetailBtn;
        }

        private void OnDisable()
        {
            detailBtn.OnClick -= OnClickDetailBtn;
        }
        public void SetData(LeagueHistoryData data)
        {
            this.data = data;
            if (data == null) return;

            leagueLevelText.text = "{0}级联赛".SafeFormat(data.LeagueLevel);
            dateText.text = "{0}-{1}".SafeFormat(TimeUtils.GetUnixTimeString(data.StartTime, "yyyy.MM.dd"), TimeUtils.GetUnixTimeString(data.EndTime, "yyyy.MM.dd"));
            teamNameText.text = Player.Name;
            teamIcon.SetIcon(Player.Icon);
            cupImage1.gameObject.SetActive(data.Rank == 1);
            cupImage2.gameObject.SetActive(data.Rank == 2);
            cupImage3.gameObject.SetActive(data.Rank == 3);
            Color rankColor = rankOtherColor;
            if (data.Rank == 1) rankColor = rank1Color;
            if (data.Rank == 2) rankColor = rank2Color;
            if (data.Rank == 3) rankColor = rank3Color;
            string rankColorStr = ColorUtility.ToHtmlStringRGB(rankColor);
            rankNumText.text = "第<size=40><color=#{0}>{1}</color></size>名".SafeFormat(rankColorStr, data.Rank);
            winLoseText.text = "<size=40>{0}</size>胜 <size=40>{1}</size>负".SafeFormat(data.Win, data.Failed);

            bool isUp = data.Rank <= 3 && data.LeagueLevel < Configs.LeagueRewardRank.GetConfigList()[^1].Level;
            bool isDown = data.Rank >= 18 && data.LeagueLevel > Configs.LeagueRewardRank.GetConfigList()[0].Level;
            upImageFont.gameObject.SetActive(isUp);
            downText.gameObject.SetActive(isDown);
            if (isUp) upImageFont.text = "晋级至{0}级联赛".SafeFormat(data.LeagueLevel + 1);
            if (isDown) downText.text = "降级至{0}级联赛".SafeFormat(data.LeagueLevel - 1);
        }

        public void OnClickDetailBtn(BabuButton sender)
        {
            UIController.Instance.OpenWindow<LeagueHistoryDetailUI>(new LeagueHistoryDetailUIProperties(data, false));
        }
    }
}
