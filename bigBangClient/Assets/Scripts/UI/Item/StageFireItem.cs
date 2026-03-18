using System.Collections;
using System.Collections.Generic;
using BigBang;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils;
using System;
using UnityEngine.UI;
using BigBang.UI;
using BigBang.Animation;

/// <summary>
/// 战斗中，某一阶段，球队爆发
/// </summary>
public class StageFireItem : MonoBehaviour
{

    [SerializeField] private RectTransform bothPanel = null;
    [SerializeField] private RectTransform redBgPanel = null;
    [SerializeField] private RectTransform blueBgPanel = null;
    [SerializeField] private RectTransform onePanel = null;
    [SerializeField] private RectTransform titlePanel = null;


    [SerializeField] private RectTransform bothRedTeamClubIconRoot = null;
    [SerializeField] private RectTransform bothRedTeamClubIconTrans = null;
    [SerializeField] private ClubIconItem bothRedTeamClubIconImage = null;
    [SerializeField] private List<GameObject> teamFireBotnTeamRedStarGoList = new();

    [SerializeField] private RectTransform bothBlueTeamClubIconRoot = null;
    [SerializeField] private RectTransform bothBlueTeamClubIconTrans = null;
    [SerializeField] private ClubIconItem bothBlueTeamClubIconImage = null;
    [SerializeField] private List<GameObject> teamFireBotnTeamBlueStarGoList = new();

    [SerializeField] private RectTransform oneTeamClubIconRoot = null;
    [SerializeField] private RectTransform oneTeamClubIconTrans = null;
    [SerializeField] private ClubIconItem oneTeamClubIconImage = null;
    [SerializeField] private List<GameObject> teamFireOneTeamStarGoList = new();

    [SerializeField] private TMP_Text titleStageText = null;

    [SerializeField] private ImageFont bothBluePercentText = null;
    [SerializeField] private ImageFont bothRedPercentText = null;
    [SerializeField] private ImageFont onePercentText = null;
    [SerializeField] private RectTransform stageFireFlyRoot = null;

    public void PlayTeamFireAni(int stage, int redPercent, int bluePercent, int redTeamIcon, int blueTeamIcon, Action callBack)
    {
        this.gameObject.SetActive(true);

        bool isRedFire = redPercent > 0;
        bool isBlueFire = bluePercent > 0;

        if (!isRedFire && !isBlueFire)
        {
            callBack?.Invoke();
            return;
        }

        bool isBoth = isRedFire && isBlueFire;

        bothPanel.gameObject.SetActive(isBoth);
        bothRedTeamClubIconRoot.gameObject.SetActive(isBoth);
        bothBlueTeamClubIconRoot.gameObject.SetActive(isBoth);
        bothRedPercentText.gameObject.SetActive(isBoth);
        bothBluePercentText.gameObject.SetActive(isBoth);
        redBgPanel.gameObject.SetActive(!isBoth && isRedFire);
        blueBgPanel.gameObject.SetActive(!isBoth && isBlueFire);
        onePanel.gameObject.SetActive(!isBoth);
        onePercentText.gameObject.SetActive(!isBoth);
        oneTeamClubIconRoot.gameObject.SetActive(!isBoth);
        titlePanel.gameObject.SetActive(true);

        titleStageText.text = "第 <size=30><color=#ffed51>{0}</color></size> 节".SafeFormat(stage);
        if (isBoth)
        {
            bothRedPercentText.text = "";
            bothBluePercentText.text = "";
            int redStar = FormationBase.GetFireCount(redPercent);
            int blueStar = FormationBase.GetFireCount(bluePercent);
            SetStar(teamFireBotnTeamRedStarGoList, redStar);
            SetStar(teamFireBotnTeamBlueStarGoList, blueStar);
            bothRedTeamClubIconImage.SetIcon(redTeamIcon);
            bothBlueTeamClubIconImage.SetIcon(blueTeamIcon);
            StartPlayBothAni(callBack, redPercent, bluePercent);
        }
        else
        {
            onePercentText.text = "";
            if (isRedFire)
            {
                int redStar = FormationBase.GetFireCount(redPercent);
                SetStar(teamFireOneTeamStarGoList, redStar);
                oneTeamClubIconImage.SetIcon(redTeamIcon);
                StartPlayOneAni(callBack, isBlueFire, redPercent);
            }
            if (isBlueFire)
            {
                int blueStar = FormationBase.GetFireCount(bluePercent);
                SetStar(teamFireOneTeamStarGoList, blueStar);
                oneTeamClubIconImage.SetIcon(blueTeamIcon);
                StartPlayOneAni(callBack, isBlueFire, bluePercent);
            }
        }
    }
    private void SetStar(List<GameObject> starGoList, int starCount)
    {
        for (int i = 0; i < starGoList.Count; i++)
        {
            starGoList[i].SetActive(i < starCount);
        }
    }
    public void ClearTeamFireAni()
    {
        clearAni();
        this.gameObject.SetActive(false);
    }

    [SerializeField] private Image darkImage = null;
    [SerializeField] private RectTransform contentPanel = null;
    private Sequence aniSeq = null;
    private float StartPlayBothAni(Action callBack, int redPercent, int bluePercent)
    {
        clearAni();
        aniSeq = DOTween.Sequence();
        darkImage.SetAlpha(0);
        stageFireFlyRoot.gameObject.SetActive(true);

        Vector3 startLocalPosBlue = Utility.ConvertLocalPosition(clubIconImageBlue.transform.parent, clubIconImageBlue.transform.localPosition, bothBlueTeamClubIconRoot);
        bothBlueTeamClubIconTrans.SetLocalScale(1f);
        bothBlueTeamClubIconTrans.SetLocalPosition(startLocalPosBlue);
        bothBlueTeamClubIconTrans.gameObject.SetAlpha(1);

        Vector3 startLocalPosRed = Utility.ConvertLocalPosition(clubIconImageRed.transform.parent, clubIconImageRed.transform.localPosition, bothRedTeamClubIconRoot);
        bothRedTeamClubIconTrans.SetLocalScale(1f);
        bothRedTeamClubIconTrans.SetLocalPosition(startLocalPosRed);
        bothRedTeamClubIconTrans.gameObject.SetAlpha(1);

        contentPanel.SetLocalScaleY(0);
        contentPanel.gameObject.SetAlpha(1);

        aniSeq.Append(bothBlueTeamClubIconTrans.DOLocalMove(Vector3.zero, 0.5f));
        aniSeq.Join(bothRedTeamClubIconTrans.DOLocalMove(Vector3.zero, 0.5f));
        aniSeq.Join(bothBlueTeamClubIconTrans.DOScale(0.7f, 0.7f).SetEase(Ease.InQuart));
        aniSeq.Join(bothRedTeamClubIconTrans.DOScale(0.7f, 0.7f).SetEase(Ease.InQuart));
        aniSeq.Join(darkImage.DOFade(155f / 255f, 0.7f));
        aniSeq.Append(contentPanel.DOScaleY(1f, 0.5f).SetEase(Ease.OutBack));
        aniSeq.AppendInterval(0.1f);
        aniSeq.Append(bothRedPercentText.DOChangeNumber(redPercent, 0.8f, 0f, "+{0}%").SetEase(Ease.Linear));
        aniSeq.Join(bothBluePercentText.DOChangeNumber(bluePercent, 0.8f, 0f, "+{0}%").SetEase(Ease.Linear));
        aniSeq.AppendInterval(0.6f);
        aniSeq.Append(contentPanel.gameObject.DOFade(0, 0.5f));
        aniSeq.Join(darkImage.DOFade(0, 0.5f));
        aniSeq.Join(bothRedTeamClubIconTrans.gameObject.DOFade(0, 0.5f));
        aniSeq.Join(bothBlueTeamClubIconTrans.gameObject.DOFade(0, 0.5f));

        aniSeq.AppendCallback(() =>
        {
            stageFireFlyRoot.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
            callBack?.Invoke();
        });
        return aniSeq.Duration();
    }
    [SerializeField] private Image clubIconImageBlue = null;
    [SerializeField] private Image clubIconImageRed = null;
    private void StartPlayOneAni(Action callBack, bool isBlueFire, int onePercent)
    {
        clearAni();
        aniSeq = DOTween.Sequence();
        darkImage.SetAlpha(0);
        Image clubIconImage = isBlueFire ? clubIconImageBlue : clubIconImageRed;
        Vector3 startLocalPos = Utility.ConvertLocalPosition(clubIconImage.transform.parent, clubIconImage.transform.localPosition, oneTeamClubIconRoot);
        stageFireFlyRoot.gameObject.SetActive(true);
        oneTeamClubIconTrans.SetLocalScale(1f);
        oneTeamClubIconTrans.SetLocalPosition(startLocalPos);
        oneTeamClubIconTrans.gameObject.SetAlpha(1);
        contentPanel.SetLocalScaleY(0);
        contentPanel.gameObject.SetAlpha(1);

        aniSeq.Append(oneTeamClubIconTrans.DOLocalMove(Vector3.zero, 0.5f));
        aniSeq.Join(oneTeamClubIconTrans.DOScale(0.7f, 0.7f).SetEase(Ease.InQuart));
        aniSeq.Join(darkImage.DOFade(155f / 255f, 0.7f));
        aniSeq.Append(contentPanel.DOScaleY(1f, 0.5f).SetEase(Ease.OutBack));
        aniSeq.AppendInterval(0.1f);
        aniSeq.Append(onePercentText.DOChangeNumber(onePercent, 0.8f, 0f, "+{0}%").SetEase(Ease.Linear));
        aniSeq.AppendInterval(0.6f);
        aniSeq.Append(contentPanel.gameObject.DOFade(0, 0.5f));
        aniSeq.Join(darkImage.DOFade(0, 0.5f));
        aniSeq.Join(oneTeamClubIconTrans.gameObject.DOFade(0, 0.5f));
        aniSeq.AppendCallback(() =>
        {
            this.gameObject.SetActive(false);
            stageFireFlyRoot.gameObject.SetActive(false);
            callBack?.Invoke();
        });
    }
    private void clearAni()
    {
        aniSeq?.Kill();
        aniSeq = null;
    }
}
