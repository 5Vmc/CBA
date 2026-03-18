using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babu.Core
{
	/// <summary>
	/// 有限状态机
	/// </summary>
    public class FiniteStateMachine : IStateMachine
	{
		/// <summary>
		/// 状态字典
		/// </summary>
		private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

		/// <summary>
		/// 当前状态
		/// </summary>
		private IState _curState;
		public IState CurrentState => _curState;

		/// <summary>
		/// 父节点
		/// </summary>
        public IStateMachine Parent { get; set; }

		/// <summary>
		/// 用户数据
		/// </summary>
        public IUserData UserData { get; set; }

		/// <summary>
		/// 增加状态
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddState<T>() where T : IState
		{
			var node = Activator.CreateInstance(typeof(T)) as IState;
			if (node == null)
			{
				throw new ArgumentNullException();
			}

			if (_states.ContainsKey(node.GetType()) == false)
			{
				node.Parent = this;
				_states.Add(node.GetType(), node);
			}
			else
			{
				Debug.LogWarning($"State {node.GetType().Name} Already Existed");
			}
		}

		/// <summary>
		/// 跳转至状态
		/// </summary>
		/// <typeparam name="T"></typeparam>
        public void ChangeToState<T>(IUserData userData = null) where T : IState
        {
			IState node = GetState(typeof(T));
			if (node == null)
			{
				Debug.LogError($"Can Not Found State {typeof(T).Name}");
				return;
			}

			if (_curState != null)
            {
				_curState.OnExit();
				Debug.Log($"============= OnExit {_curState.GetType().Name} ===========");
			}

			_curState = node;
			_curState.UserData = userData;
            Debug.Log($"============= OnEnter {_curState.GetType().Name} ===========");
			_curState.OnEnter();
		}

        public virtual void OnEnter()
        {
        }

        public void OnUpdate()
        {
			_curState?.OnUpdate();
		}

        public void OnExit()
        {
			if (_curState != null)
            {
				Debug.Log($"============= OnExit {_curState.GetType().Name} ===========");
				_curState.OnExit();
				_curState = null;
			}
        }

		public IState GetState(Type type)
		{
			if (_states.TryGetValue(type, out IState node))
			{
				return node;
			}
			return null;
		}
	}
}
