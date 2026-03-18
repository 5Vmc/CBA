using UnityEngine;
using YooAsset;

internal class StateTrain : StateCommon
{
    protected GameObject _trainGameObject;

    protected override async void LoadResource()
    {
        _state = State.WaitLoadResourceCompleted;

        Debug.Log("开始加载训练场景");
        var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/3DPrefab/Train/TrainActions.prefab");
        await h.Task;
        _trainGameObject = h.InstantiateSync();
        h.Release();

        _state = State.OpenUI;
    }

    public override void OnExit()
    {
        base.OnExit();

        if (_trainGameObject != null)
        {
            GameObject.Destroy(_trainGameObject);
        }

        _trainGameObject = null;
    }
}