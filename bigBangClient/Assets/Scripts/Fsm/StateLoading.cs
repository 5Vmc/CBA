using Babu.Core;
using BigBang;
using BigBang.UI;
using System.Threading.Tasks;
using YooAsset;

public class StateLoadingUserData : IUserData
{
    public bool FromEntry;
}


internal class StateLoading : IState
{
    public IUserData UserData { get; set; }
    public IStateMachine Parent { get; set; }

    enum State
    {
        None,

        Start,

        UnloadUnusedAssets,

        OpenUI,

        WaitingOpenUICompleted,

        Running
    }

    private State _state = State.None;

    public void OnEnter()
    {
#if !UNITY_WEBGL
        _state = State.Running;
        UIController.Instance.ShowPanel<LoadingUI>();
        return;
#endif

        if (UserData != null && (UserData as StateLoadingUserData).FromEntry)
        {
            _state = State.Running;
            UIController.Instance.ShowPanel<LoadingUI>();
        }
        else
        {
            SimpleLoadingUI.Instance.Show();

            UIController.Instance.CloseAllPanelAndWindow();

            _state = State.Start;
        }
    }

    public void OnExit()
    {
    }

    public void OnUpdate()
    {
        switch (_state)
        {
            case State.None: break;
            case State.Start: _state = State.UnloadUnusedAssets; break;
            case State.UnloadUnusedAssets: UnloadUnusedAssets(); break;
            case State.OpenUI: OpenUIAsync(); break;
        }
    }

    void UnloadUnusedAssets()
    {
        SpriteManager.UnloadAll();
        YooAssets.UnloadUnusedAssets();
        _state = State.OpenUI;
    }

    async void OpenUIAsync()
    {
        _state = State.WaitingOpenUICompleted;
        await UIController.Instance.ShowPanel<LoadingUI>();

        OnLoadCompleted();
    }

    void OnLoadCompleted()
    {
        SimpleLoadingUI.Instance.Hide();
        _state = State.Running;
    }
}