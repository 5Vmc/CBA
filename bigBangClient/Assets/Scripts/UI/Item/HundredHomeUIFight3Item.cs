using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BigBang;
using BigBang.UI;
using Protocol;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.UI.HundredFormationUI;

public class HundredHomeUIFight3Item : MonoBehaviour
{
    [SerializeField] private ClubIconItem awayClubIconImage = null;
    [SerializeField] private Image awayNoTeamImage = null;
    [SerializeField] private TMP_Text awayClubNameText = null;
    [SerializeField] private ClubIconItem homeClubIconImage = null;
    [SerializeField] private Image homeNoTeamImage = null;
    [SerializeField] private TMP_Text homeClubNameText = null;
    [SerializeField] private Image awayLineStartImage = null;
    [SerializeField] private Image homeLineStartImage = null;
    [SerializeField] private Image awayLineMidImage = null;
    [SerializeField] private Image homeLineMidImage = null;
    [SerializeField] private Image lineEndImage = null;
    [SerializeField] private BabuButton detailButton = null;
    [SerializeField] private BabuButton awayButton = null;
    [SerializeField] private BabuButton homeButton = null;
    [SerializeField] private SkeletonGraphic homeNoTeamSpine = null;
    [SerializeField] private SkeletonGraphic awayNoTeamSpine = null;

    [SerializeField] private Color nameTextColorLightNormal = new();
    [SerializeField] private Color nameTextColorLightSelf = new();
    [SerializeField] private Color nameTextColorDarkNormal = new();
    [SerializeField] private Color nameTextColorDarkSelf = new();
    [SerializeField] private Color lineColorLightNormal = new();
    [SerializeField] private Color lineColorLightSelf = new();
    [SerializeField] private Color lineColorDark = new();

    [SerializeField] private Image homeBgImage = null;
    [SerializeField] private Image awayBgImage = null;
    [SerializeField] private Image homeDarkImage = null;
    [SerializeField] private Image awayDarkImage = null;

    public LeagueCourseItemData data = null;
    public int index = 0;
    public bool isNearFight;
    public void SetData(LeagueCourseItemData data, int index, bool isNearFight)
    {
        this.data = data;
        this.index = index;
        this.isNearFight = isNearFight;

        Refresh();
    }
    private void Refresh()
    {
        homeBgImage.enabled = !isNearFight;
        awayBgImage.enabled = !isNearFight;
        homeNoTeamSpine.gameObject.SetActive(isNearFight);
        awayNoTeamSpine.gameObject.SetActive(isNearFight);

        awayLineStartImage.color = lineColorDark;
        homeLineStartImage.color = lineColorDark;
        awayLineMidImage.color = lineColorDark;
        homeLineMidImage.color = lineColorDark;
        lineEndImage.color = lineColorDark;
        awayDarkImage.gameObject.SetActive(false);
        homeDarkImage.gameObject.SetActive(false);

        if (data == null || data.AwayTeam == null)
        {
            awayNoTeamImage.gameObject.SetActive(true);
            awayClubIconImage.gameObject.SetActive(false);
            awayClubNameText.text = "";
        }
        else
        {
            awayNoTeamImage.gameObject.SetActive(false);
            awayClubIconImage.gameObject.SetActive(true);
            awayClubNameText.text = data.AwayTeam.TeamName;
            awayClubIconImage.SetIcon(data.AwayTeam.TeamIcon);
            bool isAwayMine = data.AwayTeam.TeamId == Player.GbId;
            awayClubNameText.color = isAwayMine ? nameTextColorLightSelf : nameTextColorLightNormal;
        }
        if (data == null || data.HomeTeam == null)
        {
            homeNoTeamImage.gameObject.SetActive(true);
            homeClubIconImage.gameObject.SetActive(false);
            homeClubNameText.text = "";
        }
        else
        {
            homeNoTeamImage.gameObject.SetActive(false);
            homeClubIconImage.gameObject.SetActive(true);
            homeClubNameText.text = data.HomeTeam.TeamName;
            homeClubIconImage.SetIcon(data.HomeTeam.TeamIcon);
            bool isHomeMine = data.HomeTeam.TeamId == Player.GbId;
            homeClubNameText.color = isHomeMine ? nameTextColorLightSelf : nameTextColorLightNormal;
        }
        if (data == null || data.AwayTeam == null || data.HomeTeam == null || data.AwayGoal == -1 || data.HomeGoal == -1)
        {
            detailButton.gameObject.SetActive(false);
            if (index == 6)
            {
                awayLineMidImage.gameObject.SetActive(false);
                homeLineMidImage.gameObject.SetActive(false);
            }
        }
        else
        {
            detailButton.gameObject.SetActive(true);

            bool isAwayWin = data.AwayGoal > data.HomeGoal;
            bool isHomeWin = data.AwayGoal < data.HomeGoal;
            bool isAwayMine = data.AwayTeam.TeamId == Player.GbId;
            bool isHomeMine = data.HomeTeam.TeamId == Player.GbId;

            awayDarkImage.gameObject.SetActive(!isAwayWin);
            homeDarkImage.gameObject.SetActive(isAwayWin);
            awayClubNameText.color = isAwayWin ? (isAwayMine ? nameTextColorLightSelf : nameTextColorLightNormal) : (isAwayMine ? nameTextColorDarkSelf : nameTextColorDarkNormal);
            homeClubNameText.color = isHomeWin ? (isHomeMine ? nameTextColorLightSelf : nameTextColorLightNormal) : (isHomeMine ? nameTextColorDarkSelf : nameTextColorDarkNormal);

            awayLineStartImage.color = isAwayWin ? (isAwayMine ? lineColorLightSelf : lineColorLightNormal) : lineColorDark;
            homeLineStartImage.color = isHomeWin ? (isHomeMine ? lineColorLightSelf : lineColorLightNormal) : lineColorDark;
            awayLineMidImage.color = isAwayWin ? (isAwayMine ? lineColorLightSelf : lineColorLightNormal) : lineColorDark;
            homeLineMidImage.color = isHomeWin ? (isHomeMine ? lineColorLightSelf : lineColorLightNormal) : lineColorDark;
            lineEndImage.color = (isAwayMine || isHomeMine) ? lineColorLightSelf : lineColorLightNormal;

            if (index == 6)
            {
                awayLineMidImage.gameObject.SetActive(true);
                homeLineMidImage.gameObject.SetActive(true);
                awayLineMidImage.color = isAwayWin ? (isAwayMine ? lineColorLightSelf : lineColorLightNormal) : lineColorDark;
                homeLineMidImage.color = isHomeWin ? (isHomeMine ? lineColorLightSelf : lineColorLightNormal) : lineColorDark;
            }
        }
    }

    private void OnEnable()
    {
        detailButton.OnClick += OnClickDetailButton;
        awayButton.OnClick += OnClickAwayButton;
        homeButton.OnClick += OnClickHomeButton;
    }
    private void OnDisable()
    {
        detailButton.OnClick -= OnClickDetailButton;
        awayButton.OnClick -= OnClickAwayButton;
        homeButton.OnClick -= OnClickHomeButton;
    }

    private void OnClickAwayButton(BabuButton button)
    {
        if (data == null || data.AwayTeam == null) return;
        UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(data.FightId, true, CompitionID.Hundred, data.AwayTeam.TeamId));
    }

    private void OnClickHomeButton(BabuButton button)
    {
        if (data == null || data.HomeTeam == null) return;
        UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(data.FightId, false, CompitionID.Hundred, data.HomeTeam.TeamId));
    }

    private void OnClickDetailButton(BabuButton button)
    {
        if (string.IsNullOrEmpty(data.FightId))
        {
            Debug.Log("比赛未进行");
            return;
        }
        UIController.Instance.OpenWindow<HundredTeamDetailUI>(new HundredTeamDetailUIProperties(data, HundredProgress.Fight3));
    }
}
