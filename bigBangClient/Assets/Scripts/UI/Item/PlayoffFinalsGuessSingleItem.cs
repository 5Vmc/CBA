using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.PlayoffFinalsGuessManager;
using GameItem = Utils.GameItem.GameItem;
using RewardType = BigBang.PlayoffFinalsGuessManager.RewardType;

public class PlayoffFinalsGuessSingleItem : MonoBehaviour
{
    public FinalsGuessCourseConfig finalsGuessCourseConfig = null;
    public void SetData(FinalsGuessCourseConfig finalsGuessCourseConfig)
    {
        this.finalsGuessCourseConfig = finalsGuessCourseConfig;
        RefreshUI();
    }

    [SerializeField] private Image normalBgImage = null;
    [SerializeField] private Image specialBgImage = null;
    [SerializeField] private TMP_Text fightStartTimeText = null;
    [SerializeField] private TMP_Text guessResultText = null;
    [SerializeField] private PlayoffFinalsGuessHomePadProgressItem progressPanel = null;
    [SerializeField] private RectTransform supportTeamPanel = null;
    [SerializeField] private RectTransform teamInfoPanel = null;
    [SerializeField] private RectTransform luckyNumberPanel = null;
    [SerializeField] private RectTransform scorePanel = null;
    [SerializeField] private RectTransform getRewardPanel = null;
    [SerializeField] private RectTransform supportedTeamPanel = null;
    [SerializeField] private RectTransform rewardGotPanel = null;
    private void HideAll()
    {
        normalBgImage.gameObject.SetActive(false);
        specialBgImage.gameObject.SetActive(false);
        fightStartTimeText.gameObject.SetActive(false);
        guessResultText.gameObject.SetActive(false);
        progressPanel.gameObject.SetActive(false);
        supportTeamPanel.gameObject.SetActive(false);
        teamInfoPanel.gameObject.SetActive(false);
        luckyNumberPanel.gameObject.SetActive(false);
        scorePanel.gameObject.SetActive(false);
        getRewardPanel.gameObject.SetActive(false);
        supportedTeamPanel.gameObject.SetActive(false);
        rewardGotPanel.gameObject.SetActive(false);
    }

    private bool isSpecial = false;
    private bool isCourseMatchEnd = false;
    private FinalsGuessCourse guessCourse = null;
    private FinalsGuessTeam leftGuessTeam = null;
    private FinalsGuessTeam rightGuessTeam = null;
    private MyFinalsGuess guessSingle = null;
    private bool isGuessSingle = false;
    private MyFinalsGuess guessLuckyNumber = null;
    private bool isGuessLuckyNumber = false;
    private bool isInCanGuessTime = false;
    private bool hasGuessRewardNotGet = false;
    public void RefreshUI()
    {
        HideAll();

        isSpecial = finalsGuessCourseConfig.Id == 7;
        normalBgImage.gameObject.SetActive(!isSpecial);
        specialBgImage.gameObject.SetActive(isSpecial);

        fightStartTimeText.gameObject.SetActive(true);
        guessResultText.gameObject.SetActive(true);
        fightStartTimeText.text = "第{0}场：{1}".SafeFormat(finalsGuessCourseConfig.Id.ToChinese(), TimeUtils.ToDateTime(finalsGuessCourseConfig.MatchTime).ToString("MM月dd日 HH:mm"));
        isCourseMatchEnd = PlayoffFinalsGuessManager.Instance.IsCourseMatchEnd(finalsGuessCourseConfig.Id);
        guessCourse = PlayoffFinalsGuessManager.Instance.GetCourse(finalsGuessCourseConfig.Id);
        leftGuessTeam = guessCourse?.Teams.FirstOrDefault(t => t.TeamId == (int)PlayoffFinalsGuessManager.Team.Left);
        rightGuessTeam = guessCourse?.Teams.FirstOrDefault(t => t.TeamId == (int)PlayoffFinalsGuessManager.Team.Right);
        guessSingle = PlayoffFinalsGuessManager.Instance.GetGuessSingle(finalsGuessCourseConfig.Id);
        isGuessSingle = guessSingle != null;
        guessLuckyNumber = PlayoffFinalsGuessManager.Instance.GetGuessLuckyNumber(finalsGuessCourseConfig.Id);
        isGuessLuckyNumber = guessLuckyNumber != null;
        isInCanGuessTime = finalsGuessCourseConfig.MatchTime > Utils.DataConvUtil.ServerTime;

        if (isInCanGuessTime == true)
        {
            if (isGuessSingle == false || isGuessLuckyNumber == false)
            {
                RefreshTime();
            }
            else
            {
                guessResultText.text = "等待结果";
            }
        }
        else
        {
            if (isCourseMatchEnd == false)
            {
                if (isGuessSingle == false && isGuessLuckyNumber == false)
                {
                    guessResultText.text = "未参加预测";
                }
                else
                {
                    guessResultText.text = "等待结果";
                }
            }
            else
            {
                if (isGuessSingle == false && isGuessLuckyNumber == false)
                {
                    guessResultText.text = "未参加预测";
                }
                else
                {
                    string singleStr = "";
                    if (isGuessSingle == true)
                    {
                        bool isGuessWin = (leftGuessTeam.Point > rightGuessTeam.Point && guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Left) || (leftGuessTeam.Point < rightGuessTeam.Point && guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Right);
                        singleStr = "胜负预测：" + (isGuessWin ? "<color=#FF7102>正确</color>" : "<color=#C6C6C6>错误</color>");
                    }
                    else
                    {
                        singleStr = "未参加胜负预测";
                    }

                    string luckyNumberStr = "";
                    if (isGuessLuckyNumber == true)
                    {
                        int supportTeamPoint = guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Left ? leftGuessTeam.Point : rightGuessTeam.Point;
                        int supportTeamPointEndScore = supportTeamPoint % 10;
                        bool isGuessLuckyNumberWin = supportTeamPointEndScore == guessLuckyNumber.Guess;
                        luckyNumberStr = "数字预测：" + (isGuessLuckyNumberWin ? "<color=#FF7102>正确</color>" : "<color=#C6C6C6>错误</color>");
                    }
                    else
                    {
                        luckyNumberStr = "未参加数字预测";
                    }
                    guessResultText.text = singleStr + "  " + luckyNumberStr;
                }
            }
        }

        progressPanel.gameObject.SetActive(!isCourseMatchEnd);
        if (isCourseMatchEnd == false)
        {
            progressPanel.SetData(leftGuessTeam == null ? 0 : leftGuessTeam.Support, rightGuessTeam == null ? 0 : rightGuessTeam.Support);
        }

        supportTeamPanel.gameObject.SetActive(isGuessSingle == false && isInCanGuessTime);
        supportedTeamPanel.gameObject.SetActive(isGuessSingle == true && !isCourseMatchEnd);
        supportedTeamButtonLeft.gameObject.SetActive(isGuessSingle == true && guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Left);
        supportedTeamButtonRight.gameObject.SetActive(isGuessSingle == true && guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Right);

        teamInfoPanel.gameObject.SetActive(true);
        RefreshTeamInfoPanel();

        luckyNumberPanel.gameObject.SetActive(isGuessSingle == true);
        if (luckyNumberPanel.gameObject.activeSelf) RefreshLuckyNumberPanel();

        scorePanel.gameObject.SetActive(isCourseMatchEnd);
        if (scorePanel.gameObject.activeSelf) RefreshScorePanel();

        hasGuessRewardNotGet = (isGuessSingle && guessSingle.IsReceive == false) || (isGuessLuckyNumber && guessLuckyNumber.IsReceive == false);
        getRewardPanel.gameObject.SetActive(isCourseMatchEnd && (isGuessSingle == true || isGuessLuckyNumber == true) && hasGuessRewardNotGet == true);
        rewardGotPanel.gameObject.SetActive(isCourseMatchEnd && (isGuessSingle == true || isGuessLuckyNumber == true) && hasGuessRewardNotGet == false);
    }

    private void RefreshTime()
    {
        if (isInCanGuessTime == false) return;
        if (isGuessSingle && isGuessLuckyNumber) return;
        int leftTime = (int)(finalsGuessCourseConfig.MatchTime - Utils.DataConvUtil.ServerTime);
        guessResultText.text = "预测剩余时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn(leftTime));
        if (leftTime < 0)
        {
            RefreshUI();
        }
    }

    [SerializeField] private Image teamIconImageLeft = null;
    [SerializeField] private Image teamIconImageRight = null;
    [SerializeField] private TMP_Text teamNameTextLeft = null;
    [SerializeField] private TMP_Text teamNameTextRight = null;
    private FinalsGuessTeamConfig finalsGuessTeamConfigLeft = null;
    private FinalsGuessTeamConfig finalsGuessTeamConfigRight = null;
    private async void RefreshTeamInfoPanel()
    {
        finalsGuessTeamConfigLeft = Configs.FinalsGuessTeam.GetConfigList().FirstOrDefault(t => t.Id == (int)Team.Left);
        finalsGuessTeamConfigRight = Configs.FinalsGuessTeam.GetConfigList().FirstOrDefault(t => t.Id == (int)Team.Right);
        teamNameTextLeft.text = finalsGuessTeamConfigLeft.Name;
        teamNameTextRight.text = finalsGuessTeamConfigRight.Name;
        teamIconImageLeft.sprite = await SpriteProxy.GetPlayoffFinalsGuessMVPTeamSprite(finalsGuessTeamConfigLeft.Icon);
        teamIconImageRight.sprite = await SpriteProxy.GetPlayoffFinalsGuessMVPTeamSprite(finalsGuessTeamConfigRight.Icon);
    }

    [SerializeField] private BabuButton selectLuckyNumberButton = null;
    [SerializeField] private RectTransform waitLuckyNumberPanel = null;
    [SerializeField] private BabuButton waitLuckyNumberButton = null;
    [SerializeField] private TMP_Text waitLuckyNumberText = null;
    [SerializeField] private RectTransform winLuckyNumberPanel = null;
    [SerializeField] private BabuButton winLuckyNumberButton = null;
    [SerializeField] private TMP_Text winLuckyNumberText = null;
    [SerializeField] private RectTransform loseLuckyNumberPanel = null;
    [SerializeField] private BabuButton loseLuckyNumberButton = null;
    [SerializeField] private TMP_Text loseLuckyNumberText = null;
    private void RefreshLuckyNumberPanel()
    {
        selectLuckyNumberButton.gameObject.SetActive(isGuessSingle && isGuessLuckyNumber == false && isInCanGuessTime);
        waitLuckyNumberPanel.gameObject.SetActive(isGuessSingle && isGuessLuckyNumber && isCourseMatchEnd == false);
        if (isGuessSingle && isGuessLuckyNumber && isCourseMatchEnd)
        {
            int supportTeamPoint = guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Left ? leftGuessTeam.Point : rightGuessTeam.Point;
            int supportTeamPointEndScore = supportTeamPoint % 10;
            bool isGuessLuckyNumberWin = supportTeamPointEndScore == guessLuckyNumber.Guess;
            winLuckyNumberPanel.gameObject.SetActive(isGuessLuckyNumberWin);
            loseLuckyNumberPanel.gameObject.SetActive(!isGuessLuckyNumberWin);
        }
        else
        {
            winLuckyNumberPanel.gameObject.SetActive(false);
            loseLuckyNumberPanel.gameObject.SetActive(false);
        }
        if (waitLuckyNumberPanel.gameObject.activeSelf)
        {
            waitLuckyNumberText.text = guessLuckyNumber.Guess.ToString();
        }
        if (winLuckyNumberPanel.gameObject.activeSelf)
        {
            winLuckyNumberText.text = guessLuckyNumber.Guess.ToString();
        }
        if (loseLuckyNumberPanel.gameObject.activeSelf)
        {
            loseLuckyNumberText.text = guessLuckyNumber.Guess.ToString();
        }
    }

    [SerializeField] private TMP_Text teamScoreTextLeft = null;
    [SerializeField] private TMP_Text teamScoreTextRight = null;
    private void RefreshScorePanel()
    {
        if (isGuessLuckyNumber)
        {
            bool isGuessLeft = guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Left;
            int supportTeamPoint = isGuessLeft ? leftGuessTeam.Point : rightGuessTeam.Point;
            int supportTeamPointStartScore = supportTeamPoint / 10;
            int supportTeamPointEndScore = supportTeamPoint % 10;
            bool isGuessLuckyNumberWin = supportTeamPointEndScore == guessLuckyNumber.Guess;
            string coloredEndScore = (isGuessLuckyNumberWin ? "<color=#FF7102>" : "<color=#C6C6C6>") + supportTeamPointEndScore.ToString() + "</color>";
            string coloredScore = supportTeamPointStartScore.ToString() + coloredEndScore;
            if (isGuessLeft)
            {
                teamScoreTextLeft.text = coloredScore;
                teamScoreTextRight.text = rightGuessTeam.Point.ToString();
            }
            else
            {
                teamScoreTextLeft.text = leftGuessTeam.Point.ToString();
                teamScoreTextRight.text = coloredScore;
            }
        }
        else
        {
            teamScoreTextLeft.text = leftGuessTeam.Point.ToString();
            teamScoreTextRight.text = rightGuessTeam.Point.ToString();
        }
    }

    private void OnEnable()
    {
        supportTeamButtonLeft.OnClick += OnClickSupportTeamButtonLeft;
        supportTeamButtonRight.OnClick += OnClickSupportTeamButtonRight;
        supportedTeamButtonLeft.OnClick += OnClickSupportedTeamButtonLeft;
        supportedTeamButtonRight.OnClick += OnClickSupportedTeamButtonRight;
        selectLuckyNumberButton.OnClick += OnClickSelectLuckyNumberButton;
        waitLuckyNumberButton.OnClick += OnClickWaitLuckyNumberButton;
        winLuckyNumberButton.OnClick += OnClickWinLuckyNumberButton;
        loseLuckyNumberButton.OnClick += OnClickLoseLuckyNumberButton;
        SecondUpdateManager.Instance.RegistAction(RefreshTime);
        boxGotButton.OnClick += OnClickBoxGotButton;
        boxImageButton.OnClick += OnClickBoxImageButton;
    }
    private void OnDisable()
    {
        supportTeamButtonLeft.OnClick -= OnClickSupportTeamButtonLeft;
        supportTeamButtonRight.OnClick -= OnClickSupportTeamButtonRight;
        supportedTeamButtonLeft.OnClick -= OnClickSupportedTeamButtonLeft;
        supportedTeamButtonRight.OnClick -= OnClickSupportedTeamButtonRight;
        selectLuckyNumberButton.OnClick -= OnClickSelectLuckyNumberButton;
        waitLuckyNumberButton.OnClick -= OnClickWaitLuckyNumberButton;
        winLuckyNumberButton.OnClick -= OnClickWinLuckyNumberButton;
        loseLuckyNumberButton.OnClick -= OnClickLoseLuckyNumberButton;
        SecondUpdateManager.Instance.UnRegistAction(RefreshTime);
        boxGotButton.OnClick -= OnClickBoxGotButton;
        boxImageButton.OnClick -= OnClickBoxImageButton;
    }

    [SerializeField] private BabuButton supportTeamButtonLeft = null;
    [SerializeField] private BabuButton supportTeamButtonRight = null;
    private void OnClickSupportTeamButtonLeft(BabuButton _)
    {
        OnClickSupporTeamButton(Team.Left);
    }
    private void OnClickSupportTeamButtonRight(BabuButton _)
    {
        OnClickSupporTeamButton(Team.Right);
    }
    private void OnClickSupporTeamButton(Team team)
    {
        string selectTeamName = team == Team.Left ? finalsGuessTeamConfigLeft.Name : finalsGuessTeamConfigRight.Name;
        UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("选择后无法更改，确认预测 {0} 获得本场比赛胜利吗？".SafeFormat(selectTeamName), () =>
        {
            if (finalsGuessCourseConfig.MatchTime <= Utils.DataConvUtil.ServerTime)
            {
                Tips.PopError("已过可预测时间");
            }
            else
            {
                PlayoffFinalsGuessManager.Instance.GuessSingle(finalsGuessCourseConfig.Id, (int)team);
            }
        }));
    }

    [SerializeField] private BabuButton supportedTeamButtonLeft = null;
    [SerializeField] private BabuButton supportedTeamButtonRight = null;
    private void OnClickSupportedTeamButtonLeft(BabuButton _)
    {
        OnClickSupportedTeamButton(Team.Left);
    }
    private void OnClickSupportedTeamButtonRight(BabuButton _)
    {
        OnClickSupportedTeamButton(Team.Right);
    }
    private void OnClickSupportedTeamButton(Team team)
    {
        FinalsGuessTeamConfig finalsGuessTeamConfig = Configs.FinalsGuessTeam.GetConfigList().FirstOrDefault(t => t.Id == (int)team);
        Tips.PopTips("您已选择支持{0}".SafeFormat(finalsGuessTeamConfig.Name));
    }

    private void OnClickSelectLuckyNumberButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<PlayoffFinalsGuessNumberUI>(new PlayoffFinalsGuessNumberUIProperties(finalsGuessCourseConfig.Id));
    }
    private void OnClickWaitLuckyNumberButton(BabuButton _)
    {
        Tips.PopTips("您已选择了数字{0}作为幸运数字，请等待比赛结果".SafeFormat(guessLuckyNumber.Guess));
    }
    private void OnClickWinLuckyNumberButton(BabuButton _)
    {
        Tips.PopTips("恭喜您，数字{0}是幸运数字，预测正确".SafeFormat(guessLuckyNumber.Guess));
    }
    private void OnClickLoseLuckyNumberButton(BabuButton _)
    {
        Tips.PopTips("数字{0}不是幸运数字，预测错误".SafeFormat(guessLuckyNumber.Guess));
    }

    [SerializeField] private BabuButton boxGotButton = null;
    [SerializeField] private BabuButton boxImageButton = null;
    private void OnClickBoxGotButton(BabuButton _)
    {
        Tips.PopTips("奖励已领取");
    }
    List<GameItem> gameItemList = new List<GameItem>();
    private void OnClickBoxImageButton(BabuButton _)
    {
        gameItemList.Clear();
        List<GameItem> gameItemListOnce = new();
        if (PlayoffFinalsGuessManager.Instance.HasSingleRewardCanGet(finalsGuessCourseConfig.Id))
        {
            bool isGuessSingleWin = (leftGuessTeam.Point > rightGuessTeam.Point && guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Left) || (leftGuessTeam.Point < rightGuessTeam.Point && guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Right);
            FinalsGuessRewardConfig finalsGuessRewardConfig = Configs.FinalsGuessReward.GetConfig((int)RewardType.Single);
            string rewardStr = isGuessSingleWin ? finalsGuessRewardConfig.SuccessReward : finalsGuessRewardConfig.FailReward;
            List<GameItem> gameItemTeamList = GameItemUtils.CreateGameItems(rewardStr).ToList();
            gameItemListOnce.AddRange(gameItemTeamList);
        }
        if (PlayoffFinalsGuessManager.Instance.HasLuckyNumberRewardCanGet(finalsGuessCourseConfig.Id))
        {
            int supportTeamPoint = guessSingle.Guess == (int)PlayoffFinalsGuessManager.Team.Left ? leftGuessTeam.Point : rightGuessTeam.Point;
            int supportTeamPointEndScore = supportTeamPoint % 10;
            bool isGuessLuckyNumberWin = supportTeamPointEndScore == guessLuckyNumber.Guess;
            FinalsGuessRewardConfig finalsGuessRewardConfig = Configs.FinalsGuessReward.GetConfig((int)RewardType.LuckyNumber);
            string rewardStr = isGuessLuckyNumberWin ? finalsGuessRewardConfig.SuccessReward : finalsGuessRewardConfig.FailReward;
            List<GameItem> gameItemLuckyNumberList = GameItemUtils.CreateGameItems(rewardStr).ToList();
            gameItemListOnce.AddRange(gameItemLuckyNumberList);
        }
        gameItemList.AddRange(gameItemListOnce);
        if (isSpecial)
        {
            gameItemList.AddRange(gameItemListOnce);
        }
        PlayoffFinalsGuessManager.Instance.ReceiveCourseReward(finalsGuessCourseConfig.Id,() =>
        {
            var properties = new InventoryObtainedUIProperties(gameItemList);
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            RefreshUI();
        });
        
    }
}
