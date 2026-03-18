using System.Collections;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

[ExecuteAlways]
public class PlayoffFinalsGuessHomePadProgressItem : MonoBehaviour
{
    [SerializeField] private RectTransform leftRectTransform = null;
    [SerializeField] private RectTransform rightRectTransform = null;
    [SerializeField] private TMP_Text leftText = null;
    [SerializeField] private TMP_Text rightText = null;

    [SerializeField] private float sliderMinWidth = 140f;
    [SerializeField] private float sliderMaxWidth = 492;

    [Range(0, 100)]
    public long leftNum = 0;
    [Range(0, 100)]
    public long rightNum = 0;

#if UNITY_EDITOR
    private void Update()
    {
        RefreshData();
    }
#endif

    /// <summary>
    /// 两边的数值
    /// </summary>
    public void SetData(long leftNum, long rightNum)
    {
        leftNum += 10;
        rightNum += 10;
        this.leftNum = leftNum > 0 ? leftNum : 0;
        this.rightNum = rightNum > 0 ? rightNum : 0;

        RefreshData();
    }

    private void RefreshData()
    {
        SetSliderBarWidth(leftNum, rightNum);
    }

    private void SetPercentText(long leftNum, long rightNum)
    {
        leftText.text = "{0}%".SafeFormat(leftNum.ToString("N0"));
        rightText.text = "{0}%".SafeFormat(rightNum.ToString("N0"));
    }
    private void SetSliderBarWidth(long leftNum, long rightNum)
    {
        leftNum = leftNum > 1 ? leftNum : 1;
        rightNum = rightNum > 1 ? rightNum : 1;
        float leftT = leftNum / (float)(rightNum + leftNum);
        float leftWidth = Mathf.Lerp(sliderMinWidth, sliderMaxWidth, leftT);
        leftRectTransform.SetSizeDeltaWidth(leftWidth);
        float rightT = 1 - leftT;
        float rightWidth = Mathf.Lerp(sliderMinWidth, sliderMaxWidth, rightT);
        rightRectTransform.SetSizeDeltaWidth(rightWidth);

        if (leftNum == 0 && rightNum == 0)
        {
            SetPercentText(50, 50);
        }
        else
        {
            int leftPercent = Mathf.RoundToInt(leftT * 100);
            int rightPercent = 100 - leftPercent;
            SetPercentText(leftPercent, rightPercent);
        }
    }
}
