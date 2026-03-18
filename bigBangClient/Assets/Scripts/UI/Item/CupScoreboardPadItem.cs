using System;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CupScoreboardPadItemData
    {
        public string FightID;
        public string ClubID;
        public string Name;
        public int Score;
        public int ClubIcon;

        public CupScoreboardPadItemData(string fightID, string clubId, string name, int score, int icon)
        {
            FightID = fightID;
            ClubID = clubId;
            Name = name;
            Score = score;
            ClubIcon = icon;
        }

        public GetHundredCourseResponse getHundredCourseResponse;
        public int CourseId;
        public string TeamId;
    }

    public class CupScoreboardPadItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Image noScoreImage = null;
        [SerializeField] private ClubIconItem clubIcon;
        [SerializeField] private Button clickBtn;
        [SerializeField] private LinkItem linkItem;

        [SerializeField] private Image supportImage = null;

        //private Color whiteColor = new Color(0f / 255f, 252f / 255f, 255f / 255f, 1);
        //private Color grayColor = new Color(89f / 255f, 105f / 255f, 109f / 255f, 1);
        //private Color selfColor = new Color(7f / 255f, 255f / 255f, 61f / 255f, 1);
        //private Color defaultColor = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1);

        [SerializeField] private Color nameBgColorLight = new();
        [SerializeField] private Color nameBgColorDark = new();
        [SerializeField] private Color scoreBgColorLight = new();
        [SerializeField] private Color scoreBgColorDark = new();
        [SerializeField] private Color nameTextColorLightNormal = new();
        [SerializeField] private Color nameTextColorLightSelf = new();
        [SerializeField] private Color nameTextColorDarkNormal = new();
        [SerializeField] private Color nameTextColorDarkSelf = new();
        [SerializeField] private Color scoreTextColorLightNormal = new();
        [SerializeField] private Color scoreTextColorLightSelf = new();
        [SerializeField] private Color scoreTextColorDarkNormal = new();
        [SerializeField] private Color scoreTextColorDarkSelf = new();
        [SerializeField] private Color lineColorLightNormal = new();
        [SerializeField] private Color lineColorLightSelf = new();
        [SerializeField] private Color lineColorDark = new();
        [SerializeField] private float clubIconLightAlpha = 1.0f;
        [SerializeField] private float clubIconDarkAlpha = 0.7f;

        [SerializeField] private BabuButton detailButton = null;

        [SerializeField] private Image nameBgImage = null;
        [SerializeField] private Image scoreBgImage = null;

        public CupScoreboardPadItemData dataProvider;

        private void OnEnable()
        {
            clickBtn.onClick.AddListener(OnClick);
            detailButton.OnClick += OnClickDetailButton;
        }

        private void OnDisable()
        {
            clickBtn.onClick.RemoveListener(OnClick);
            detailButton.OnClick -= OnClickDetailButton;
        }

        private void OnClickDetailButton(BabuButton button)
        {
            OnClickDetail?.Invoke(this);
        }

        public Action<CupScoreboardPadItem> OnClickItem = null;
        public Func<CupScoreboardPadItem, bool> NeedShowDetail = null;
        public Action<CupScoreboardPadItem> OnClickDetail = null;
        public int lineId;
        public int index;
        public string fightId;
        public bool isAway;
        public void SetData(CupScoreboardPadItemData data, bool showDetailButton, Action<CupScoreboardPadItem> OnClickItem, Func<CupScoreboardPadItem, bool> NeedShowDetail, Action<CupScoreboardPadItem> OnClickDetail, int lineId, int index, string fightId, bool isAway)
        {
            dataProvider = data;
            this.OnClickItem = OnClickItem;
            this.NeedShowDetail = NeedShowDetail;
            this.OnClickDetail = OnClickDetail;
            this.lineId = lineId;
            this.index = index;
            this.fightId = fightId;
            this.isAway = isAway;
            bool outDataCanShow = NeedShowDetail?.Invoke(this) ?? false;
            detailButton.gameObject.SetActive(showDetailButton && outDataCanShow);
            if (data == null)
            {
                nameText.gameObject.SetActive(false);
                scoreText.gameObject.SetActive(false);
                noScoreImage.gameObject.SetActive(false);
                clubIcon.gameObject.SetActive(false);
                return;
            }
            nameText.gameObject.SetActive(true);
            clubIcon.gameObject.SetActive(true);
            nameText.color = data.ClubID == Player.GbId ? nameTextColorLightSelf : nameTextColorLightNormal;
            scoreText.color = data.ClubID == Player.GbId ? scoreTextColorLightSelf : scoreTextColorLightNormal;
            // 设置队名
            nameText.text = data.Name;
            // 设置球队图标
            clubIcon.SetIcon(data.ClubIcon);
            // 设置比分
            if (data.Score == -1)
            {
                scoreText.gameObject.SetActive(false);
                noScoreImage.gameObject.SetActive(true);
            }
            else
            {
                scoreText.gameObject.SetActive(true);
                noScoreImage.gameObject.SetActive(false);
                scoreText.text = data.Score.ToString();
            }
            supportImage.gameObject.SetActive(HundredManager.Instance.IsSupported(data));
        }

        public void OnClick()
        {
            OnClickItem?.Invoke(this);
        }

        public void SetAsWin()
        {
            Color LineColor = dataProvider.ClubID == Player.GbId ? lineColorLightSelf : lineColorLightNormal;
            linkItem.SetLineColor(LineColor, LineColor, LineColor);
            nameText.color = dataProvider.ClubID == Player.GbId ? nameTextColorLightSelf : nameTextColorLightNormal;
            clubIcon.Img.SetAlpha(clubIconLightAlpha);
            scoreText.color = dataProvider.ClubID == Player.GbId ? scoreTextColorLightSelf : scoreTextColorLightNormal;
            nameBgImage.color = nameBgColorLight;
            scoreBgImage.color = scoreBgColorLight;
        }

        public void SetAsFailed(bool isGroupOtherIsMine)
        {
            linkItem.SetLineColor(lineColorDark, lineColorDark, isGroupOtherIsMine ? lineColorLightSelf : lineColorLightNormal);
            nameText.color = dataProvider.ClubID == Player.GbId ? nameTextColorDarkSelf : nameTextColorDarkNormal;
            clubIcon.Img.SetAlpha(clubIconDarkAlpha);
            scoreText.color = dataProvider.ClubID == Player.GbId ? scoreTextColorDarkSelf : scoreTextColorDarkNormal;
            nameBgImage.color = nameBgColorDark;
            scoreBgImage.color = scoreBgColorDark;
        }

        public void SetAsNotFight()
        {
            linkItem.SetLineColor(lineColorDark, lineColorDark, lineColorDark);
            nameText.color = dataProvider.ClubID == Player.GbId ? nameTextColorLightSelf : nameTextColorLightNormal;
            clubIcon.Img.SetAlpha(clubIconLightAlpha);
            noScoreImage.color = dataProvider.ClubID == Player.GbId ? scoreTextColorLightSelf : scoreTextColorLightNormal;
            nameBgImage.color = nameBgColorLight;
            scoreBgImage.color = scoreBgColorLight;
        }

        public void SetAsNone()
        {
            linkItem.SetLineColor(lineColorDark, lineColorDark, lineColorDark);
            nameText.color = nameTextColorDarkNormal;
            clubIcon.Img.SetAlpha(clubIconDarkAlpha);
            scoreText.color = scoreTextColorDarkNormal;
            nameBgImage.color = nameBgColorDark;
            scoreBgImage.color = scoreBgColorDark;
        }
    }
}
