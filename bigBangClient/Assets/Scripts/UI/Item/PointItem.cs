using System;
using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class PointItem : MonoBehaviour
{
    [SerializeField] private BabuButton pointButton = null;
    [SerializeField] private DarkLightItem darkLightItem = null;

    private void OnEnable()
    {
        pointButton.OnClick += OnClickPointButton;
    }
    private void OnDisable()
    {
        pointButton.OnClick -= OnClickPointButton;
    }

    public void SetData(int index , Action<int> clickCallback)
    {
        this.index = index;
        this.OnClickPoint = clickCallback;
    }

    private int index = 0;
    private Action<int> OnClickPoint = null;

    private void OnClickPointButton(BabuButton _)
    {
        OnClickPoint?.Invoke(index);
    }

    public void SetLight(bool isLight)
    {
        darkLightItem.SetLight(isLight);
    }

    public int Index
    {
        get
        {
            return index;
        }
    }
}
