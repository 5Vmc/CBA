using System;
using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class HundredHomeUIFight1Item : MonoBehaviour
{
    [SerializeField] private Image resultBgImage = null;
    [SerializeField] private ClubIconItem leftClubIconImage = null;
    [SerializeField] private ClubIconItem rightClubIconImage = null;
    [SerializeField] private TMP_Text leftClubNameText = null;
    [SerializeField] private TMP_Text rightClubNameText = null;
    [SerializeField] public BabuButton detailButton = null;
    [SerializeField] private RectTransform scorePanel = null;
    [SerializeField] private TMP_Text leftScoreText = null;
    [SerializeField] private TMP_Text rightScoreText = null;
    [SerializeField] private RectTransform losePanel = null;
    [SerializeField] private RectTransform winPanel = null;

    [SerializeField] private Color nameTextColorLightNormal = new();
    [SerializeField] private Color nameTextColorLightSelf = new();

    public LeagueCourseItemData data = null;
    public bool forceMineLeft = false;
    public void SetData(LeagueCourseItemData data, bool forceMineLeft = false)
    {
        this.data = data;
        this.forceMineLeft = forceMineLeft;
        Refresh();
    }
    private void Refresh()
    {
        bool mineIsAway = data.AwayTeam.TeamId == Player.GbId;
        PlayerTeamData playerTeamDataLeft = mineIsAway ? data.AwayTeam : data.HomeTeam;
        PlayerTeamData playerTeamDataRight = mineIsAway ? data.HomeTeam : data.AwayTeam;
        if (!forceMineLeft)
        {
            playerTeamDataLeft = data.AwayTeam;
            playerTeamDataRight = data.HomeTeam;
        }

        leftClubIconImage.SetIcon(playerTeamDataLeft.TeamIcon);
        rightClubIconImage.SetIcon(playerTeamDataRight.TeamIcon);
        leftClubNameText.text = playerTeamDataLeft.TeamName;
        leftClubNameText.color = playerTeamDataLeft.TeamId == Player.GbId ? nameTextColorLightSelf : nameTextColorLightNormal;
        rightClubNameText.text = playerTeamDataRight.TeamName;
        rightClubNameText.color = playerTeamDataRight.TeamId == Player.GbId ? nameTextColorLightSelf : nameTextColorLightNormal;

        int goalLeft = mineIsAway ? data.AwayGoal : data.HomeGoal;
        int goalRight = mineIsAway ? data.HomeGoal : data.AwayGoal;
        if (!forceMineLeft)
        {
            goalLeft = data.AwayGoal;
            goalRight = data.HomeGoal;
        }
        leftScoreText.text = goalLeft.ToString();
        rightScoreText.text = goalRight.ToString();

        if (!forceMineLeft)
        {
            losePanel.gameObject.SetActive(false);
            winPanel.gameObject.SetActive(false);
        }
        else
        {
            losePanel.gameObject.SetActive(goalLeft < goalRight);
            winPanel.gameObject.SetActive(goalLeft > goalRight);
        }
    }

    private void OnEnable()
    {
        detailButton.OnClick += OnClickDetailButton;
    }
    private void OnDisable()
    {
        detailButton.OnClick -= OnClickDetailButton;
    }

    private void OnClickDetailButton(BabuButton button)
    {
        if (string.IsNullOrEmpty(data.FightId))
        {
            Debug.Log("比赛未进行");
            return;
        }
        UIController.Instance.OpenWindow<HundredTeamDetailUI>(new HundredTeamDetailUIProperties(data, HundredProgress.Fight1, true));
    }
}
