using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.UI;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

public class HundredGuessSupportItem : MonoBehaviour
{
    [SerializeField] private ClubIconItem leftClubIconImage = null;
    [SerializeField] private TMP_Text leftClubNameText = null;
    [SerializeField] private BabuButton leftSupportBtn = null;
    [SerializeField] private HundredGuessProgressItem progressPanel = null;
    [SerializeField] private ClubIconItem rightClubIconImage = null;
    [SerializeField] private TMP_Text rightClubNameText = null;
    [SerializeField] private BabuButton rightSupportBtn = null;
    [SerializeField] private Image leftSupportedImage = null;
    [SerializeField] private Image rightSupportedImage = null;

    [SerializeField] private BabuButton leftClubIconBgImage = null;
    [SerializeField] private BabuButton rightClubIconBgImage = null;

    private void OnEnable()
    {
        leftSupportBtn.OnClick += OnLeftSupportBtnClick;
        rightSupportBtn.OnClick += OnRightSupportBtnClick;
        leftClubIconBgImage.OnClick += OnLeftClubIconBgImageClick;
        rightClubIconBgImage.OnClick += OnRightClubIconBgImageClick;
    }
    private void OnDisable()
    {
        leftSupportBtn.OnClick -= OnLeftSupportBtnClick;
        rightSupportBtn.OnClick -= OnRightSupportBtnClick;
        leftClubIconBgImage.OnClick -= OnLeftClubIconBgImageClick;
        rightClubIconBgImage.OnClick -= OnRightClubIconBgImageClick;
    }
    private void OnLeftSupportBtnClick(BabuButton _)
    {
        CheckSupportTip(true);
    }
    private void OnRightSupportBtnClick(BabuButton _)
    {
        CheckSupportTip(false);
    }
    private void OnLeftClubIconBgImageClick(BabuButton _)
    {
        UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(leagueCourseItemData.FightId, true, CompitionID.Hundred, leagueCourseItemData.AwayTeam.TeamId));
    }
    private void OnRightClubIconBgImageClick(BabuButton _)
    {
        UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(leagueCourseItemData.FightId, false, CompitionID.Hundred, leagueCourseItemData.HomeTeam.TeamId));
    }
    private void CheckSupportTip(bool isAway)
    {
        if (leagueCourseItemData.Time < Utils.DataConvUtil.ServerTime)
        {
            Tips.PopTips("已进行的比赛不能应援");
            return;
        }
        if (leagueCourseItemData.Time < Utils.DataConvUtil.ServerTime + 30 * 60)
        {
            Tips.PopTips("开赛前30分钟内不能应援");
            return;
        }
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, GoodsId.HundredGuessWhistle, 0);
        if (gameItem.GetPlayerCount() <= 0)
        {
            Tips.PopTips("{0}不足".SafeFormat(gameItem.GetName()));
            return;
        }
        if (HundredManager.Instance.isNeedAlertSupport)
        {
            ConfirmBoxCheckUIProperties confirmBoxCheckUIProperties = new("要用使用一个{0}来应援<color=#9D2A2D>{1}</color>吗？若比赛胜利则返还双倍{0}。".SafeFormat(gameItem.GetName(), isAway ? leagueCourseItemData.AwayTeam.TeamName : leagueCourseItemData.HomeTeam.TeamName), () =>
            {
                OnSupport(isAway);
            }, null, !HundredManager.Instance.isNeedAlertSupport, "不再提醒", (bool isCheck) => { HundredManager.Instance.isNeedAlertSupport = !isCheck; });
            UIController.Instance.OpenWindow<ConfirmBoxCheckUI>(confirmBoxCheckUIProperties);
        }
        else
        {
            OnSupport(isAway);
        }
    }
    private void OnSupport(bool isAway)
    {
        int zoneId = 0;
        if ((HundredProgress)HundredManager.Instance.guessCourseInfo.Stage == HundredProgress.Fight2)
        {
            bool isSupported = HundredManager.Instance.guessCourseInfo.SupportZone >= 1 && HundredManager.Instance.guessCourseInfo.SupportZone <= 8;
            zoneId = isSupported ? HundredManager.Instance.guessCourseInfo.SupportZone : HundredManager.Instance.dropdownValue + 1;
        }
        HundredManager.Instance.guessCourseInfo.SupportZone = zoneId;
        if (HundredManager.Instance.nowCourse != null) HundredManager.Instance.nowCourse.SupportZone = zoneId;
        int courseId = leagueCourseItemData.CourseId;
        string teamId = isAway ? leagueCourseItemData.AwayTeam.TeamId : leagueCourseItemData.HomeTeam.TeamId;
        NetworkManager.Instance.SupportHundred(zoneId, courseId, teamId, (SupportHundredResponse supportHundredResponse) =>
        {
            if (supportHundredResponse.SupportSucceed)
            {
                if ((HundredProgress)HundredManager.Instance.guessCourseInfo.Stage == HundredProgress.Fight2)
                {
                    if (HundredManager.Instance.guessSupportInfo.SupportPlayoffCourses.FirstOrDefault(IsSameFight) == null)
                        HundredManager.Instance.guessSupportInfo.SupportPlayoffCourses.Add(leagueCourseItemData);
                }
                else
                {
                    if (HundredManager.Instance.guessSupportInfo.SupportChampionCourses.FirstOrDefault(IsSameFight) == null)
                        HundredManager.Instance.guessSupportInfo.SupportChampionCourses.Add(leagueCourseItemData);
                }
                leagueCourseItemData.AwayTeam.Support += isAway ? 1 : 0;
                leagueCourseItemData.HomeTeam.Support += isAway ? 0 : 1;
                SupportCourseData supportCourseData = HundredManager.Instance.AddSupportLocal(leagueCourseItemData, isAway);
                EventManager.Instance.Dispatch(EventID.AfterHundredGuessSupport, supportCourseData);
            }
            RefreshInfo(true);
            AudioManager.Instance.PlaySound(AudioNames.BTN_CFM);
        });
    }

    public LeagueCourseItemData leagueCourseItemData = null;
    public void SetData(LeagueCourseItemData leagueCourseItemData)
    {
        this.leagueCourseItemData = leagueCourseItemData;
        leftClubIconImage.SetIcon(leagueCourseItemData.AwayTeam.TeamIcon);
        leftClubNameText.text = leagueCourseItemData.AwayTeam.TeamName;
        rightClubIconImage.SetIcon(leagueCourseItemData.HomeTeam.TeamIcon);
        rightClubNameText.text = leagueCourseItemData.HomeTeam.TeamName;
        RefreshInfo();
    }
    private void RefreshInfo(bool needSupportAnim = false)
    {
        leftSupportedImage.transform.DOKill();
        rightSupportedImage.transform.DOKill();
        bool isSupported = false;
        bool isSupportAway = HundredManager.Instance.IsSupported(leagueCourseItemData, true);
        bool isSupportHome = HundredManager.Instance.IsSupported(leagueCourseItemData, false);
        isSupported = isSupportAway || isSupportHome;
        leftSupportedImage.gameObject.SetActive(isSupportAway);
        rightSupportedImage.gameObject.SetActive(isSupportHome);
        leftSupportBtn.gameObject.SetActive(!isSupported);
        rightSupportBtn.gameObject.SetActive(!isSupported);
        progressPanel.SetData(leagueCourseItemData.AwayTeam.Support, leagueCourseItemData.HomeTeam.Support);
        if (needSupportAnim)
        {
            if (isSupportAway)
            {
                PlaySupportAnim(true);
            }
            if (isSupportHome)
            {
                PlaySupportAnim(false);
            }
        }
    }
    private bool IsSameFight(LeagueCourseItemData data)
    {
        if (leagueCourseItemData.Round != this.leagueCourseItemData.Round) return false;
        if (leagueCourseItemData.AwayTeam.TeamId != data.AwayTeam.TeamId) return false;
        if (leagueCourseItemData.HomeTeam.TeamId != data.HomeTeam.TeamId) return false;
        return true;
    }

    private void PlaySupportAnim(bool isAway)
    {
        Transform supportImageTrans = isAway ? leftSupportedImage.transform : rightSupportedImage.transform;
        supportImageTrans.SetLocalScale(2);
        supportImageTrans.DOScale(1, 0.8f).SetEase(Ease.OutBack).AddTo(this.gameObject);
    }
}
