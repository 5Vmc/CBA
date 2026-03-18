using System;
using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using UnityEngine;
using UnityEngine.UI;

public class BabuToggleCbaLogItem : MonoBehaviour
{
    [SerializeField] private int logId = 0;
    private void SendCbaLog()
    {
        CbaLogManager.Instance.AddLog(logId);
    }

    private BabuToggle babuToggle = null;
    private void Start()
    {
        if (babuToggle == null)
        {
            babuToggle = this.GetComponent<BabuToggle>();
        }
        if (babuToggle != null)
        {
            babuToggle.OnSelect += OnSelect;
        }
    }
    protected void OnDestroy()
    {
        if (babuToggle != null)
        {
            babuToggle.OnSelect -= OnSelect;
        }
    }

    private void OnSelect()
    {
        SendCbaLog();
    }
}
