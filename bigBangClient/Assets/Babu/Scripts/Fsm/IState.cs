namespace Babu.Core
{
	/// <summary>
	/// 状态机状态接口
	/// </summary>
	public interface IState
	{
		/// <summary>
		/// 用户数据
		/// </summary>
		IUserData UserData { set; }

		/// <summary>
		/// 父节点
		/// </summary>
		IStateMachine Parent { set; }

		/// <summary>
		/// 状态进入回调
		/// </summary>
		void OnEnter();

		/// <summary>
		/// 状态每帧Update回调
		/// </summary>
		void OnUpdate();

		/// <summary>
		/// 状态离开回调
		/// </summary>
		void OnExit();
	}
}
