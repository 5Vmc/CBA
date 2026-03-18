using Babu.Core;
using BigBang;
using BigBang.UI;
using System;
using System.Threading.Tasks;
using YooAsset;

public class StateCommonUserData : IUserData
{
    public Func<Task> OpenUIAction;
}

internal class StateCommon : IState
{
    public IUserData UserData { get; set; }
    public IStateMachine Parent { get; set; }

    protected enum State
    {
        None,

        Start,

        UnloadUnusedAssets,

        LoadResource,

        WaitLoadResourceCompleted,

        OpenUI,

        WaitForOpenUICompleted,

        Running
    }

    protected State _state = State.None;

    public virtual void OnEnter()
    {
#if !UNITY_WEBGL
        var userData = UserData as StateCommonUserData;
        userData.OpenUIAction();
        _state = State.Running;
        return;
#endif

        SimpleLoadingUI.Instance.Show();

        UIController.Instance.CloseAllPanelAndWindow();

        _state = State.Start;
    }

    public virtual void OnExit()
    {
    }

    public void OnUpdate()
    {
        switch (_state)
        {
            case State.None: break;
            case State.Start: _state = State.UnloadUnusedAssets; break;
            case State.UnloadUnusedAssets: UnloadUnusedAssets(); break;
            case State.LoadResource: LoadResource(); break;
            case State.OpenUI: OpenUI(); break;
        }
    }

    protected virtual void UnloadUnusedAssets()
    {
        SpriteManager.UnloadAll();
        YooAssets.UnloadUnusedAssets();
        _state = State.LoadResource;
    }

    protected virtual void LoadResource()
    {
        _state = State.OpenUI;
    }

    protected virtual async void OpenUI()
    {
        _state = State.WaitForOpenUICompleted;
        if (UserData != null)
        {
            var userData = UserData as StateCommonUserData;
            if (userData != null && userData.OpenUIAction != null)
            {
                await userData.OpenUIAction();
                OnLoadCompleted();
            }
        }
        else
        {
            await UIController.Instance.ShowPanel<HomeUI>();
            OnLoadCompleted();
        }
    }

    protected virtual void OnLoadCompleted()
    {
        SimpleLoadingUI.Instance.Hide();

        _state = State.Running;
    }
}