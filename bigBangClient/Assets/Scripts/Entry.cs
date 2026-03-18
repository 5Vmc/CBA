using Babu.Client.Fsm;
using UnityEngine;

internal class Entry : MonoBehaviour
{
    private void Awake()
    {
        FsmManager.Instance.AddState<StateLoading>();
        FsmManager.Instance.AddState<StateCreatePlayer>();
        FsmManager.Instance.AddState<StateHome>();
        FsmManager.Instance.AddState<StateTrain>();
        FsmManager.Instance.AddState<StateBattle>();
        FsmManager.Instance.AddState<StateTinyFun>();
        FsmManager.Instance.AddState<StateNft>();
        FsmManager.Instance.AddState<StateMainTask>();
        FsmManager.Instance.AddState<StateTask>();
        FsmManager.Instance.AddState<StateMonthSign>();

        FsmManager.Instance.ChangeToState<StateLoading>(new StateLoadingUserData() { FromEntry = true });
    }
}