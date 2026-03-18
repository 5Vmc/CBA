using System;
using System.Collections;
using System.Collections.Generic;
using BigBang;
using BigBang.UI;
using UnityEngine;
using UnityEngine.UI;

public class EnableCbaLogItem : MonoBehaviour
{
    [SerializeField] private int logId = 0;
    private void SendCbaLog()
    {
        CbaLogManager.Instance.AddLog(logId);
    }

    private void OnEnable()
    {
        SendCbaLog();
    }
}
