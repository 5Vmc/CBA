using Babu.Core;

namespace Babu.Client.Fsm
{
    /// <summary>
    /// 状态管理类
    /// </summary>
    public class FsmManager : BabuSingleton<FsmManager>
    {
        private FiniteStateMachine _fsm = new FiniteStateMachine();

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        void Update()
        {
            _fsm.OnUpdate();
        }

        /// <summary>
        /// 增加状态机
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddState<T>() where T : IState
        {
            _fsm.AddState<T>();
        }

        /// <summary>
        /// 跳转到状态机
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userData">用户数据</param>
        public void ChangeToState<T>(IUserData userData = null) where T : IState
        {
            _fsm.ChangeToState<T>(userData);
        }
    }
}
