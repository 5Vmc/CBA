using System;
using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCbaLogItem : MonoBehaviour
{
    [SerializeField] private int logId = 0;
    private void SendCbaLog()
    {
        CbaLogManager.Instance.AddLog(logId);
    }

    private BabuButton babuButton = null;
    private Button button = null;
    private void OnEnable()
    {
        if (babuButton == null)
        {
            babuButton = this.GetComponent<BabuButton>();
        }
        if (babuButton != null)
        {
            babuButton.OnClick += OnClickBabuButton;
        }
        else
        {
            if (button == null) button = this.GetComponent<Button>();
            if (button != null) button.onClick.AddListener(OnClickButton);
        }
    }
    private void OnDisable()
    {
        if (babuButton != null)
        {
            babuButton.OnClick -= OnClickBabuButton;
        }
        else
        {
            if (button != null) button.onClick.RemoveListener(OnClickButton);
        }
    }

    private void OnClickButton()
    {
        SendCbaLog();
    }
    private void OnClickBabuButton(BabuButton _)
    {
        SendCbaLog();
    }
}
