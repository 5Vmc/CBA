namespace Babu.Core
{
    /// <summary>
    /// 状态机接口
    /// </summary>
    public interface IStateMachine : IState
    {
        /// <summary>
        /// 增加状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddState<T>() where T : IState;

        /// <summary>
        /// 跳转到指定状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void ChangeToState<T>(IUserData userData = null) where T : IState;
    }
}
