using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using WowoFramework.Singleton;

namespace WowoFramework.FSM
{
    public abstract class FSM<T> : MonoBehaviourSingleton<T> where T : MonoBehaviourSingleton<T>
    {
        protected FSMState CurrentState;
        protected readonly Dictionary<int, FSMState> States = new Dictionary<int, FSMState>();

        protected abstract override void Awake();
        
        public virtual void Update()
        {
            CurrentState.OnUpdate();
        }

        public virtual void FixedUpdate()
        {
            CurrentState.OnFixedUpdate();
        }

        public virtual void AddState(FSMState state)
        {
            States.Add(state.stateID, state);
        }

        public virtual void EnterState(int stateID)
        {
            if (States.TryGetValue(stateID, out var state))
            {
                CurrentState?.OnExit();
                CurrentState = state;
                CurrentState.OnEnter();
            }
            else
            {
                Debug.LogError($"状态管理中不存在状态：{stateID}，请在状态管理器实例 {typeof(T)}.Awake() 中添加");
            }
        }
    }
}
