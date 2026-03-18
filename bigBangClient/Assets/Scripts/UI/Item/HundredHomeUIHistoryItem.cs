using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HundredHomeUIHistoryItem : MonoBehaviour
{
    [SerializeField] private BabuButton hundredHomeUIHistoryItem = null;
    [SerializeField] private ClubIconItem iconImage = null;
    [SerializeField] private TMP_Text nameText = null;
    [SerializeField] private Color nameTextColorNormal = new();
    [SerializeField] private Color nameTextColorSelf = new();

    private void OnEnable()
    {
        hundredHomeUIHistoryItem.OnClick += OnClick;
    }
    private void OnDisable()
    {
        hundredHomeUIHistoryItem.OnClick -= OnClick;
    }

    public CourseTeamData courseTeamData = null;
    public void SetData(CourseTeamData courseTeamData)
    {
        this.courseTeamData = courseTeamData;
        bool hasData = courseTeamData != null;
        this.gameObject.SetActive(hasData);
        if (!hasData) return;
        iconImage.SetIcon(courseTeamData.Team.TeamIcon);
        nameText.text = courseTeamData.Team.TeamName;
        bool isAwayMine = courseTeamData.Team.TeamId == Player.GbId;
        nameText.color = isAwayMine ? nameTextColorSelf : nameTextColorNormal;
    }

    private void OnClick(BabuButton _)
    {
        UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties("", true, CompitionID.Hundred, "", courseTeamData));
    }
}
