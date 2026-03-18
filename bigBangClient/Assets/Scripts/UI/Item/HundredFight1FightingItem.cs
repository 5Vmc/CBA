using System;
using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using Coffee.UIEffects;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.UI.HundredFormationUI;

public class HundredFight1FightingItem : MonoBehaviour
{
    [SerializeField] private RectTransform fightingPanel = null;
    [SerializeField] private UIShiny fightingBgImage = null;
    [SerializeField] private Image fightingTextImage = null;
    [SerializeField] private Image waitFightTextImage = null;
    [SerializeField] private TMP_Text waitTipText = null;
    [SerializeField] private LeftTimeComponent leftTimeComponent = null;
    [SerializeField] private ClubIconItem leftClubIconImage = null;
    [SerializeField] private ClubIconItem rightClubIconImage = null;
    [SerializeField] private TMP_Text leftClubNameText = null;
    [SerializeField] private TMP_Text rightClubNameText = null;

    [SerializeField] private Color nameTextColorLightNormal = new();
    [SerializeField] private Color nameTextColorLightSelf = new();

    public LeagueCourseItemData data = null;
    public Action OnTimeEnd = null;
    public int leftTime = -1;
    public void SetData(LeagueCourseItemData data, Action OnTimeEnd)
    {
        this.data = data;
        this.OnTimeEnd = OnTimeEnd;
        Refresh();
    }
    private void Refresh()
    {
        int fightingLeftTime = (int)(data.Time - Utils.DataConvUtil.ServerTime) + 5;
        int waitFightLeftTime = (int)(data.Time - Utils.DataConvUtil.ServerTime) - 3 * 60 + 5;
        bool isWaiting = waitFightLeftTime > 0;//true:等待开战,false:战斗中
        fightingBgImage.enabled = !isWaiting;
        fightingBgImage.effectFactor = 0;
        SetLeftTime(isWaiting ? waitFightLeftTime : fightingLeftTime);
        waitFightTextImage.gameObject.SetActive(isWaiting);
        waitTipText.gameObject.SetActive(!isWaiting);
        fightingTextImage.gameObject.SetActive(!isWaiting);
        bool mineIsAway = data.AwayTeam.TeamId == Player.GbId;
        PlayerTeamData playerTeamDataLeft = mineIsAway ? data.AwayTeam : data.HomeTeam;
        PlayerTeamData playerTeamDataRight = mineIsAway ? data.HomeTeam : data.AwayTeam;

        leftClubIconImage.SetIcon(playerTeamDataLeft.TeamIcon);
        rightClubIconImage.SetIcon(playerTeamDataRight.TeamIcon);
        leftClubNameText.text = playerTeamDataLeft.TeamName;
        leftClubNameText.color = playerTeamDataLeft.TeamId == Player.GbId ? nameTextColorLightSelf : nameTextColorLightNormal;
        rightClubNameText.text = playerTeamDataRight.TeamName;
        rightClubNameText.color = playerTeamDataRight.TeamId == Player.GbId ? nameTextColorLightSelf : nameTextColorLightNormal;
    }

    private void OnEnable()
    {
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
    }
    private void OnDisable()
    {
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
    }


    private void RefreshLeftTimeOneSec()
    {
        if (leftTime > 0)
        {
            leftTime--;
            if (leftTime <= 0)
            {
                OnTimeEnd?.Invoke();
            }
            leftTimeComponent.SetLeftTimeText(leftTime);
        }
    }
    private void SetLeftTime(int leftTime)
    {
        this.leftTime = leftTime;
        leftTimeComponent.SetLeftTimeText(leftTime);
    }
}
