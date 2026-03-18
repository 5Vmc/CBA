using System;
using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using UnityEngine;

public class ClassicMapLevelTabItem : MonoBehaviour
{
    [SerializeField] private GameObject lightPanel;
    [SerializeField] private GameObject darkPanel;

    private bool isLight;
    public void SetLight(bool isLight)
    {
        this.isLight = isLight;
        lightPanel.SetActive(isLight);
        darkPanel.SetActive(!isLight);
    }

    public int level = 1;
    [SerializeField] private BabuButton btn;
    private Action<int> callback;
    public void SetCallBack(Action<int> callback)
    {
        this.callback = callback;
    }
    public void OnClick(BabuButton sender)
    {
        callback?.Invoke(level);
    }

    private void OnEnable()
    {
        if (btn == null) return;
        btn.OnClick += OnClick;
    }
    private void OnDisable()
    {
        if (btn == null) return;
        btn.OnClick -= OnClick;
    }

}
