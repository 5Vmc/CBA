using System;
using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using deVoid.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class HomeCenterButton : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform = null;
    [SerializeField] private BabuButton button = null;
    [SerializeField] private Image bgImage = null;
    [SerializeField] private TMP_Text nameText = null;
    [SerializeField] private Image borderImage = null;
    [SerializeField] private Image dotNodeImg = null;
    [SerializeField] private Image darkImage = null;
    [SerializeField] private TMP_Text lockText = null;
    [SerializeField] private Image lockImage = null;

    [SerializeField] private TriggerModuleType triggerModuleType = TriggerModuleType.Unknow;
    [SerializeField] private string redDotString = "";

    [SerializeField] private bool doNotControlRedDot = false;

    private void OnEnable()
    {
        RefreshLock(null);
        RefreshRedDot(null);
        if (button != null) button.OnClick += OnClickButton;
        EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        EventManager.Instance.Register(EventID.OnTeamlevelUp, RefreshLock);
        //EventManager.Instance.Register(EventID.RefreshBigBangUIRedDot, RefreshRedDot);
    }
    private void OnDisable()
    {
        if (button != null) button.OnClick -= OnClickButton;
        EventManager.Instance?.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
        EventManager.Instance?.Unregister(EventID.OnTeamlevelUp, RefreshLock);
        //EventManager.Instance.Unregister(EventID.RefreshBigBangUIRedDot, RefreshRedDot);
    }
    private void RefreshLock(object[] _)
    {
        bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, false);
        darkImage.gameObject.SetActive(!isModuleOpen);
        lockText.gameObject.SetActive(!isModuleOpen);
        lockImage.gameObject.SetActive(!isModuleOpen);
        if (!isModuleOpen) lockText.text = "<color=#FFA712>{0}</color>级开启".SafeFormat(TriggerManager.Instance.GetModuleOpenInfo(triggerModuleType, true));
    }
    public Action OnClick = null;
    private void OnClickButton(BabuButton _)
    {
        bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, true);
        if (!isModuleOpen) return;
        TriggerManager.Instance.JumpPanel(triggerModuleType);
        OnClick?.Invoke();
    }

    public void RefreshRedDot(object[] _)
    {
        if (doNotControlRedDot) return;
        bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, false);
        if (!isModuleOpen || string.IsNullOrWhiteSpace(redDotString))
        {
            dotNodeImg.gameObject.SetActive(false);
            return;
        }
        RedDotNode node = RedDotManager.Instance.ConfirmNode(redDotString, "");
        node.IsRed(dotNodeImg.transform);
    }

}
