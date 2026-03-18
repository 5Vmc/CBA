using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class HundredGuessHistoryItem : MonoBehaviour
{
    [SerializeField] private Image bgTopImageWin = null;
    [SerializeField] private Image bgTopImageNormal = null;
    [SerializeField] private ClubIconItem leftClubIconImage = null;
    [SerializeField] private ClubIconItem rightClubIconImage = null;
    [SerializeField] private TMP_Text leftClubNameText = null;
    [SerializeField] private TMP_Text rightClubNameText = null;
    [SerializeField] private RectTransform scorePanel = null;
    [SerializeField] private TMP_Text leftScoreText = null;
    [SerializeField] private TMP_Text rightScoreText = null;
    [SerializeField] private Image leftSupportedImage = null;
    [SerializeField] private Image rightSupportedImage = null;
    [SerializeField] private RectTransform leftWinLosePanel = null;
    [SerializeField] private Image leftWinImage = null;
    [SerializeField] private Image leftLoseImage = null;
    [SerializeField] private RectTransform rightWinLosePanel = null;
    [SerializeField] private Image rightWinImage = null;
    [SerializeField] private Image rightLoseImage = null;
    [SerializeField] private BabuButton detailButton = null;
    [SerializeField] private TMP_Text titleText = null;
    [SerializeField] private BabuButton addGoodsButton = null;
    [SerializeField] private Image waitImage = null;
    [SerializeField] private BabuButton leftClubButton = null;
    [SerializeField] private BabuButton rightClubButton = null;

    private void OnEnable()
    {
        detailButton.OnClick += OnDetailButtonClick;
        addGoodsButton.OnClick += OnAddGoodsButtonClick;
        leftClubButton.OnClick += OnLeftButtonClick;
        rightClubButton.OnClick += OnRightButtonClick;
    }
    private void OnDisable()
    {
        detailButton.OnClick -= OnDetailButtonClick;
        addGoodsButton.OnClick -= OnAddGoodsButtonClick;
        leftClubButton.OnClick -= OnLeftButtonClick;
        rightClubButton.OnClick -= OnRightButtonClick;
    }
    private void OnDetailButtonClick(BabuButton _)
    {
        UIController.Instance.OpenWindow<HundredTeamDetailUI>(new HundredTeamDetailUIProperties(data, HundredProgress.NotOpen));
    }
    private void OnAddGoodsButtonClick(BabuButton _)
    {
        ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Goods, GoodsId.HundredGuessWhistle, Player.PackageManager.GetGoodsNumber(GoodsId.HundredGuessWhistle));
        itemtipsUIProperties.SetPos(addGoodsButton.transform, new Vector3(0, -20f, 0));
        UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
    }
    private void OnLeftButtonClick(BabuButton _)
    {
        UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(data.FightId, true, CompitionID.Hundred, data.AwayTeam.TeamId));
    }
    private void OnRightButtonClick(BabuButton _)
    {
        UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(data.FightId, false, CompitionID.Hundred, data.HomeTeam.TeamId));
    }

    private LeagueCourseItemData data = null;
    public void SetData(LeagueCourseItemData data)
    {
        this.data = data;
        leftClubIconImage.SetIcon(data.AwayTeam.TeamIcon);
        rightClubIconImage.SetIcon(data.HomeTeam.TeamIcon);
        leftClubNameText.text = data.AwayTeam.TeamName;
        rightClubNameText.text = data.HomeTeam.TeamName;
        leftScoreText.text = data.AwayGoal.ToString();
        rightScoreText.text = data.HomeGoal.ToString();
        RefreshInfo();
    }
    private readonly string newTitleFight2 = "{0}新星赛区{1}";
    private readonly string normalFight2 = "{0}第{1}赛区{2}";
    private void RefreshInfo()
    {
        bool isPlayOff = HundredManager.Instance.guessSupportInfo.SupportPlayoffCourses.Contains(data);
        HundredProgress hundredProgress = isPlayOff ? HundredProgress.Fight2 : HundredProgress.Fight3;
        if (hundredProgress == HundredProgress.Fight2)
        {
            if (HundredManager.Instance.guessCourseInfo.SupportZone == 9)
            {
                titleText.text = newTitleFight2.SafeFormat(HundredManager.Instance.GetStageName(hundredProgress), HundredManager.Instance.GetRoundMatchTitle(true, data.Round));
            }
            else
            {
                titleText.text = normalFight2.SafeFormat(HundredManager.Instance.GetStageName(hundredProgress), HundredManager.Instance.guessCourseInfo.SupportZone.ToChinese(), HundredManager.Instance.GetRoundMatchTitle(true, data.Round));
            }
        }
        else
        {
            titleText.text = "{0}{1}".SafeFormat(HundredManager.Instance.GetStageName(hundredProgress), HundredManager.Instance.GetRoundMatchTitle(false, data.Round));
        }

        bool isSupportedAwayWin = HundredManager.Instance.IsSupported(data, true);
        leftSupportedImage.gameObject.SetActive(isSupportedAwayWin);
        rightSupportedImage.gameObject.SetActive(!isSupportedAwayWin);

        if (data.AwayGoal <= 0 && data.HomeGoal <= 0)
        {
            bgTopImageWin.gameObject.SetActive(false);
            bgTopImageNormal.gameObject.SetActive(true);
            scorePanel.gameObject.SetActive(false);
            leftWinLosePanel.gameObject.SetActive(false);
            rightWinLosePanel.gameObject.SetActive(false);
            detailButton.gameObject.SetActive(false);
            addGoodsButton.gameObject.SetActive(false);
            waitImage.gameObject.SetActive(true);
            return;
        }
        scorePanel.gameObject.SetActive(true);
        detailButton.gameObject.SetActive(true);
        waitImage.gameObject.SetActive(false);
        leftScoreText.text = data.AwayGoal.ToString();
        rightScoreText.text = data.HomeGoal.ToString();
        leftWinLosePanel.gameObject.SetActive(isSupportedAwayWin);
        rightWinLosePanel.gameObject.SetActive(!isSupportedAwayWin);
        bool isAwayWin = data.AwayGoal > data.HomeGoal;
        bool isHomeWin = data.AwayGoal < data.HomeGoal;
        bool isGuessRight = isAwayWin && isSupportedAwayWin || isHomeWin && !isSupportedAwayWin;
        if (isSupportedAwayWin)
        {
            leftWinImage.gameObject.SetActive(isAwayWin);
            leftLoseImage.gameObject.SetActive(!isAwayWin);
        }
        else
        {
            rightWinImage.gameObject.SetActive(isHomeWin);
            rightLoseImage.gameObject.SetActive(!isHomeWin);
        }
        bgTopImageWin.gameObject.SetActive(isGuessRight);
        bgTopImageNormal.gameObject.SetActive(!isGuessRight);
        addGoodsButton.gameObject.SetActive(isGuessRight);
    }
}
