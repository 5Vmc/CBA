using Babu;
using deVoid.UIFramework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Device;
using Task = System.Threading.Tasks.Task;

public class UIController : BabuSingleton<UIController>
{
    private class UIData
    {
        public string ScreenId { get; set; }
        public IScreenProperties Properties { get; set; }

        public UIData(string screenId, IScreenProperties properties)
        {
            ScreenId = screenId;
            Properties = properties;
        }
    }

    [SerializeField] public UIFrame uiFrame;
    [SerializeField] GameObject simpleLoadingUI;
    //[SerializeField] GameObject firstPanel;
    // private string _lastShowScreenId = "";

    /// <summary>
    /// 关闭后是否可以弹窗（启动窗口堆栈中的）
    /// </summary>
    public bool PopwindowFlag = true;
    public Canvas Canvas { get => GetComponent<Canvas>(); }

    private Stack<UIData> _openScreenStack = new Stack<UIData>();

    public GameObject CurrentPanel { get => GetCurrentShowPanel() == null ? null : uiFrame.Prefabs[GetCurrentShowPanel().ScreenId]; }

    public int CheckPanelInterval = 5;
    public int ClosedPanelDestroyTime = 30;

    void Start()
    {
        simpleLoadingUI.SetActive(false);
    }

    public void ShowNavigation<T>(IScreenProperties screenProperties = null)
    {
        string screenId = typeof(T).Name;
        uiFrame.ShowScreen(screenId, screenProperties);
    }

    private UIData GetCurrentShowPanel()
    {
        if (_openScreenStack.Count > 0)
        {
            return _openScreenStack.Peek();
        }

        return null;
    }

    public string GetCurrentShowPanelName()
    {
        UIData uiData = GetCurrentShowPanel();
        if (uiData != null)
            return uiData.ScreenId;
        return "";
    }

    public Task ShowPanel<T>(IScreenProperties screenProperties = null, bool showImmediately = true)
    {
        string screenId = typeof(T).Name;

        return ShowPanel(screenId, screenProperties, showImmediately);
    }

    public Task ShowPanel(string screenId, IScreenProperties screenProperties = null, bool showImmediately = true)
    {
        if (showImmediately)
        {
            var currentShowScreenId = GetCurrentShowPanel()?.ScreenId;

            if (currentShowScreenId == screenId)
            {
                //Debug.LogWarning("这里不应该进来，暂时会容错，场景Id：" + screenId);
            }
            else
            {
                if (!string.IsNullOrEmpty(currentShowScreenId))
                {
                    uiFrame.HidePanel(currentShowScreenId);
                }
                _openScreenStack.Push(new UIData(screenId, screenProperties));
            }

            return uiFrame.ShowScreen(screenId, screenProperties);
        }
        else
        {
            _onstartpanels.Push(new UIData(screenId, screenProperties));
            return Task.CompletedTask;
        }
    }

    //public void ShowPanel<T>(bool needCloseCurrent, IScreenProperties screenProperties = null)
    //{
    //    var currentShowScreenId = GetCurrentShowPanel()?.ScreenId;
    //    if (!string.IsNullOrEmpty(currentShowScreenId) && needCloseCurrent)
    //    {
    //        uiFrame.HidePanel(currentShowScreenId);
    //    }

    //    string screenId = typeof(T).Name;
    //    _openScreenStack.Push(new UIData(screenId, screenProperties));
    //    uiFrame.ShowScreen(screenId, screenProperties);
    //}

    public void HidePanel<T>(bool DoNotPop = false)
    {
        if (_openScreenStack.Count == 0)
        {
            return;
        }
        string screenId = typeof(T).Name;
        Debug.Log("HidePanel screenId = " + screenId);
        var nowScreen = _openScreenStack.Pop();
        if (screenId != nowScreen.ScreenId) return;
        var lastUI = GetCurrentShowPanel();
        if (lastUI == null) return;

        uiFrame.HidePanel(screenId);

        if (DoNotPop == false)
        {
            uiFrame.ShowScreen(lastUI.ScreenId, lastUI.Properties);
        }

        if (PopwindowFlag == true) OpenAllHideScreens();
    }

    public void HideTopestPanel()
    {
        if (_openScreenStack.Count == 0)
        {
            return;
        }
        var nowScreen = _openScreenStack.Pop();
        string screenId = nowScreen.ScreenId;
        uiFrame.HidePanel(screenId);

        //PrintAllOpenScreenStack();
    }

    public void PrintAllOpenScreenStack()
    {
        string screenIdAll = "screenIdAll : ";
        foreach (var item in _openScreenStack)
        {
            screenIdAll += item.ScreenId + " , ";
        }
        Debug.LogWarning(screenIdAll);
    }

    /// <summary>
    /// 启动待弹窗的window
    /// </summary>
    private Stack<UIData> _onstartwindows = new();
    /// <summary>
    /// 启动待弹窗的panel
    /// </summary>
    private Stack<UIData> _onstartpanels = new();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="screenProperties"></param>
    /// <param name="openImmediately">如果不立即打开就压入堆栈，在游戏启动的时候这么做，形成启动弹窗队列</param>
    public Task OpenWindow<T>(IScreenProperties screenProperties = null, bool openImmediately = true)
    {
        string screenId = typeof(T).Name;
        if (openImmediately)
        {
            return uiFrame.ShowScreen(screenId, screenProperties);
        }
        else
        {
            _onstartwindows.Push(new UIData(screenId, screenProperties));
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 打开1个启动弹窗
    /// </summary>
    public void OpenAllHideScreens()
    {
        Debug.Log("OpenAllHideScreens");
        if (_onstartpanels.Count > 0)
        {
            var uidata = _onstartpanels.Pop();
            //_openScreenStack.Push(uidata);
            //uiFrame.ShowScreen(uidata.ScreenId, uidata.Properties);
            ShowPanel(uidata.ScreenId, uidata.Properties);
        }
        else if (_onstartwindows.Count > 0)
        {
            var uidata = _onstartwindows.Pop();
            uiFrame.ShowScreen(uidata.ScreenId, uidata.Properties);
        }

    }

    public void CloseWindow<T>(bool callbackClose = true)
    {
        string screenId = typeof(T).Name;
        Debug.Log("HidePanel screenId = " + screenId);
        uiFrame.CloseWindow(screenId);
        if (PopwindowFlag && callbackClose)
            OpenAllHideScreens();

    }

    public Camera GetCamera()
    {
        return uiFrame.UICamera;
    }

    /// <summary>
    /// 是否显示一层透明图来屏蔽点击UI
    /// </summary>
    public bool IsTouchMaskShow
    {
        get
        {
            return uiFrame.IsTouchMaskShow;
        }
        set
        {
            uiFrame.IsTouchMaskShow = value;
        }
    }

    public void CloseAllPanelAndWindow()
    {
        Debug.Log("CloseAllScreen");
        //_openScreenStack.Clear();
        uiFrame.HideAll();
    }

    public void DestroyScreen(IUIScreenController screen)
    {
        uiFrame.DestroyScreen(screen);
    }

}
