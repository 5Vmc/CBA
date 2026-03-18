using System;
using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using UnityEngine;
using UnityEngine.UI;

public class ToggleCbaLogItem : MonoBehaviour
{
    [SerializeField] private int logId = 0;
    private void SendCbaLog()
    {
        CbaLogManager.Instance.AddLog(logId);
    }

    private Toggle toggle = null;
    private void Start()
    {
        if (toggle == null)
        {
            toggle = this.GetComponent<Toggle>();
        }
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnValueChanged);
        }
    }
    protected void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnValueChanged);
        }
    }

    private void OnValueChanged(bool flag)
    {
        if (flag) SendCbaLog();
    }
}
