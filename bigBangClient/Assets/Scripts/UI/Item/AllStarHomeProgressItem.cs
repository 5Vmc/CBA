using System.Collections;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

[ExecuteAlways]
public class AllStarHomeProgressItem : MonoBehaviour
{
    [SerializeField] private Image leftImage = null;
    [SerializeField] private Image rightImage = null;
    [SerializeField] private RectTransform leftRectTransform = null;
    [SerializeField] private RectTransform rightRectTransform = null;
    [SerializeField] private ImageFont leftText = null;
    [SerializeField] private ImageFont rightText = null;

    private readonly float sliderMinWidth = 140f;
    private readonly float sliderMaxWidth = 496;

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
        this.leftNum = leftNum > 0 ? leftNum : 0;
        this.rightNum = rightNum > 0 ? rightNum : 0;

        RefreshData();
    }

    private void RefreshData()
    {
        SetPercentText(leftNum, rightNum);
        SetSliderBarWidth(leftNum, rightNum);
    }

    private void SetPercentText(long leftNum, long rightNum)
    {
        leftText.text = "{0}".SafeFormat(leftNum.ToString("N0"));
        rightText.text = "{0}".SafeFormat(rightNum.ToString("N0"));
    }
    [SerializeField] private RectTransform midPanel = null;
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
        midPanel.SetAnchoredPositionX(leftRectTransform.anchoredPosition.x + leftRectTransform.sizeDelta.x);
    }

    [SerializeField] private RectTransform lightImageLeft = null;
    [SerializeField] private RectTransform lightImageRight = null;

    private Sequence seq = null;
    public void StartLightMove()
    {
        lightImageLeft.gameObject.SetActive(true);
        lightImageRight.gameObject.SetActive(true);
        seq?.Kill();
        seq = null;
        seq = DOTween.Sequence();
        seq.SetTarget(this.gameObject);
        seq.AddTo(this.gameObject);

        seq.AppendCallback(() => { lightImageLeft.SetLocalPositionX(-leftRectTransform.sizeDelta.x / 2 - lightImageLeft.sizeDelta.x / 2); });
        seq.AppendCallback(() => { lightImageRight.SetLocalPositionX(rightRectTransform.sizeDelta.x / 2 + lightImageRight.sizeDelta.x / 2); });
        seq.Append(lightImageLeft.DOAnchorPosX(leftRectTransform.sizeDelta.x / 2 + lightImageLeft.sizeDelta.x / 2, 1f));
        seq.Join(lightImageRight.DOAnchorPosX(-rightRectTransform.sizeDelta.x / 2 - lightImageRight.sizeDelta.x / 2, 1f));

        seq.AppendInterval(1f);
        seq.SetLoops(-1);
    }
    public void StopLightMove()
    {
        seq?.Kill();
        seq = null;
        lightImageLeft.gameObject.SetActive(false);
        lightImageRight.gameObject.SetActive(false);
    }

}
