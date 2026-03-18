using System;
using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HundredHomeUISignItem : MonoBehaviour
{
    [SerializeField] private BabuButton hundredHomeUISignItem = null;
    [SerializeField] private Image bgImage = null;
    [SerializeField] private Image ballImage = null;
    [SerializeField] private Image areaTextImage = null;
    [SerializeField] private HorizontalLayoutGroup midHorizontalLayout = null;
    [SerializeField] private Image manImage = null;
    [SerializeField] public TMP_Text countText = null;
    [SerializeField] public Image lightImage = null;
    [SerializeField] public Image darkImage = null;
    [SerializeField] public Image signSuccessImage = null;

    [HideInInspector] public int index = 0;

    private void OnEnable()
    {
        hundredHomeUISignItem.OnClick += OnClickHundredHomeUISignItem;
    }
    private void OnDisable()
    {
        hundredHomeUISignItem.OnClick -= OnClickHundredHomeUISignItem;
    }

    public Action<int> ClickCallback = null;
    private void OnClickHundredHomeUISignItem(BabuButton _)
    {
        ClickCallback?.Invoke(index);
    }
}
