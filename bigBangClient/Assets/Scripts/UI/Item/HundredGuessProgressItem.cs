using System.Collections;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

[ExecuteAlways]
public class HundredGuessProgressItem : MonoBehaviour
{
    [SerializeField] private Image leftImage = null;
    [SerializeField] private Image rightImage = null;
    [SerializeField] private RectTransform leftRectTransform = null;
    [SerializeField] private RectTransform rightRectTransform = null;
    [SerializeField] private TMP_Text leftText = null;
    [SerializeField] private TMP_Text rightText = null;

    private readonly float sliderMinWidth = 40f;
    private readonly float sliderMaxWidth = 364f - 40f;

    [Range(0, 100)]
    public int leftNum = 0;
    [Range(0, 100)]
    public int rightNum = 0;

#if UNITY_EDITOR
    private void Update()
    {
        RefreshData();
    }
#endif

    /// <summary>
    /// 两边的数值
    /// </summary>
    public void SetData(int leftNum, int rightNum)
    {
        this.leftNum = leftNum > 0 ? leftNum : 0;
        this.rightNum = rightNum > 0 ? rightNum : 0;

        RefreshData();
    }

    private void RefreshData()
    {
        SetSliderBarWidth(leftNum, rightNum);
    }
    [SerializeField] private RectTransform midPanel = null;
    private void SetSliderBarWidth(int leftNumO, int rightNumO)
    {
        float leftT = 0f;
        if (leftNumO == 0 && rightNumO == 0)
        {
            leftT = 0.5f;
        }
        else if (leftNumO == 0)
        {
            leftT = 0f;
        }
        else if (rightNumO == 0)
        {
            leftT = 1f;
        }
        else
        {
            int leftNum1 = leftNumO > 1 ? leftNumO : 1;
            int rightNum1 = rightNumO > 1 ? rightNumO : 1;
            leftT = leftNum1 / (float)(rightNum1 + leftNum1);
        }
        float rightT = 1 - leftT;
        int leftPercent = Mathf.RoundToInt(leftT * 100);
        leftText.text = "{0}%".SafeFormat(leftPercent);
        rightText.text = "{0}%".SafeFormat(100 - leftPercent);
        float leftWidth = Mathf.Lerp(sliderMinWidth, sliderMaxWidth, leftT);
        leftRectTransform.SetSizeDeltaWidth(leftWidth);
        float rightWidth = Mathf.Lerp(sliderMinWidth, sliderMaxWidth, rightT);
        rightRectTransform.SetSizeDeltaWidth(rightWidth);
        midPanel.SetAnchoredPositionX(leftRectTransform.anchoredPosition.x + leftRectTransform.sizeDelta.x);
    }

}
