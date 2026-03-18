using System;
using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class HomeTopButton : MonoBehaviour
{
    [SerializeField] private BabuButton button = null;
    [SerializeField] public Image dotNodeImg = null;

    [SerializeField] private TriggerModuleType triggerModuleType = TriggerModuleType.Unknow;
    [SerializeField] private string redDotString = "";

    [SerializeField] private List<int> activityCientTypeList = new();

    [SerializeField] public bool doNotMove = false;
    [SerializeField] public bool useActivityRedDot = false;

    [SerializeField] public bool controlActiveByUI = false;

    [SerializeField] private bool doNotControlRedDot = false;

    private void OnEnable()
    {
        //Debug.LogError("OnEnable " + this.GetHashCode() + " " + this.gameObject.name);
        RefreshLock(null);
        RefreshRedDot(null);
        button.OnClick += OnClickButton;
        EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        EventManager.Instance.Register(EventID.OnTeamlevelUp, RefreshLock);
    }
    private void OnDisable()
    {
        //Debug.LogError("OnDisable " + this.GetHashCode() + " " + this.gameObject.name);
        button.OnClick -= OnClickButton;
        EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
        EventManager.Instance.Unregister(EventID.OnTeamlevelUp, RefreshLock);
    }
    private void OnDestroy()
    {
        button.OnClick -= OnClickButton;
        EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
        EventManager.Instance.Unregister(EventID.OnTeamlevelUp, RefreshLock);
    }
    bool isLock = false;
    public void RefreshLock(object[] _)
    {
        bool isControlByUI = triggerModuleType == TriggerModuleType.Unknow && activityCientTypeList.Count == 0;
        if (isControlByUI || controlActiveByUI) return;

        isLock = false;
        if (triggerModuleType != TriggerModuleType.Unknow)
        {
            bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, false);
            if (!isModuleOpen) isLock = true;
        }

        if (activityCientTypeList.Count > 0)
        {
            bool isActivityOpen = false;
            foreach (var activityCientType in activityCientTypeList)
            {
                List<ActivityConfig> activityConfigList = ActivityController.Instance.GetConfigListByType((ActivityClientType)activityCientType);
                foreach (ActivityConfig activityConfig in activityConfigList)
                {
                    if (ActivityController.Instance.IsActivityOpen(activityConfig))
                    {
                        isActivityOpen = true;
                        break;
                    }
                }
                if (isActivityOpen == true) break;
            }
            if (!isActivityOpen) isLock = true;
        }

        gameObject.SetActive(!isLock);
    }
    public Action OnClick = null;
    private void OnClickButton(BabuButton _)
    {
        RefreshLock(null);
        if (isLock)
        {
            Tips.PopTips("该活动已结束");
            return;
        }
        bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, true);
        if (!isModuleOpen) return;
        TriggerManager.Instance.JumpPanel(triggerModuleType);
        OnClick?.Invoke();
    }

    public void RefreshRedDot(object[] _)
    {
        if (doNotControlRedDot) return;
        //Debug.LogError("RefreshRedDot " + this.GetHashCode() + " " + this.gameObject.name);

        bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, false);
        if (!isModuleOpen)
        {
            dotNodeImg.gameObject.SetActive(false);
            return;
        }
        if (useActivityRedDot)
        {
            List<int> activityIdOpenList = new();

            foreach (var activityCientType in activityCientTypeList)
            {
                List<ActivityConfig> activityConfigList = ActivityController.Instance.GetConfigListByType((ActivityClientType)activityCientType);
                foreach (ActivityConfig activityConfig in activityConfigList)
                {
                    if (ActivityController.Instance.IsActivityOpen(activityConfig.Id))
                    {
                        activityIdOpenList.Add(activityConfig.Id);
                    }
                }
            }
            if (activityIdOpenList.Count == 0)
            {
                dotNodeImg.gameObject.SetActive(false);
            }
            else
            {
                bool isRed = false;
                foreach (int activityIdOpen in activityIdOpenList)
                {
                    bool isOneRed = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityIdOpen).IsRed(null);
                    if (isOneRed)
                    {
                        isRed = true;
                        break;
                    }
                }
                dotNodeImg.transform.gameObject.SetActive(isRed);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(redDotString))
            {
                dotNodeImg.gameObject.SetActive(false);
                return;
            }
            RedDotNode node = RedDotManager.Instance.ConfirmNode(redDotString, "");
            node.IsRed(dotNodeImg.transform);
        }
    }

}
