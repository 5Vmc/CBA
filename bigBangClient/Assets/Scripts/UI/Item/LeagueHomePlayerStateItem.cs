using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class LeagueHomePlayerStateItem : MonoBehaviour
{
    [SerializeField] private Color redColor = new Color();
    [SerializeField] private Color midColor = new Color();
    [SerializeField] private Color greenColor = new Color();

    [SerializeField] private Image awayImage = null;
    [SerializeField] private Image homeImage = null;
    [SerializeField] private RectTransform awayRectTransform = null;
    [SerializeField] private RectTransform homeRectTransform = null;
    [SerializeField] private TMP_Text homePercentText = null;
    [SerializeField] private TMP_Text awayPercentText = null;

    private readonly float sliderMinWidth = 15f;
    private readonly float sliderMaxWidth = 310f;

    [Range(0.9f, 100f)]
    public float homePercent = 0;
    [Range(0.9f, 100f)]
    public float awayPercent = 0;

#if UNITY_EDITOR
    private void Update()
    {
        RefreshData();
    }
#endif

    /// <summary>
    /// 两边的百分数
    /// </summary>
    /// <param name="homePercent">0f - 100f</param>
    /// <param name="awayPercent">0f - 100f</param>
    public void SetData(float homePercent, float awayPercent)
    {
        this.homePercent = Utility.KeepInRange(homePercent, 0.9f, 100);
        this.awayPercent = Utility.KeepInRange(awayPercent, 0.9f, 100);

        RefreshData();
    }

    private void RefreshData()
    {
        SetPercentText(homePercent, awayPercent);
        SetSliderBarWidth(homePercent, awayPercent);
        SetColor(homePercent, awayPercent);
    }

    private void SetPercentText(float homePercent, float awayPercent)
    {
        homePercentText.text = "{0}%".SafeFormat(Mathf.FloorToInt(homePercent));
        awayPercentText.text = "{0}%".SafeFormat(Mathf.FloorToInt(awayPercent));
    }
    private void SetSliderBarWidth(float homePercent, float awayPercent)
    {
        float leftT = homePercent / (homePercent + awayPercent);
        float leftWidth = Mathf.Lerp(sliderMinWidth, sliderMaxWidth, leftT);
        homeRectTransform.SetSizeDeltaWidth(leftWidth);
        float rightT = 1 - leftT;
        float rightWidth = Mathf.Lerp(sliderMinWidth, sliderMaxWidth, rightT);
        awayRectTransform.SetSizeDeltaWidth(rightWidth);
    }
    private void SetColor(float homePercent, float awayPercent)
    {
        Color homeColor;
        if (homePercent / 100 < 0.5)
        {
            homeColor = Color.Lerp(redColor, midColor, homePercent / 100 * 2);
        }
        else
        {
            homeColor = Color.Lerp(midColor, greenColor, (homePercent / 100 - 0.5f) * 2);
        }
        homeImage.color = homeColor;
        homePercentText.color = homeColor;

        Color awayColor;
        if (awayPercent / 100 < 0.5)
        {
            awayColor = Color.Lerp(redColor, midColor, awayPercent / 100 * 2);
        }
        else
        {
            awayColor = Color.Lerp(midColor, greenColor, (awayPercent / 100 - 0.5f) * 2);
        }
        awayImage.color = awayColor;
        awayPercentText.color = awayColor;
    }

}
