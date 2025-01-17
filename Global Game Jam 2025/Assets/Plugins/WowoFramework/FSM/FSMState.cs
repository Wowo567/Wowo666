using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WowoFramework.FSM
{
    public abstract class FSMState
    {
        protected FSMState(int stateID)
        { 
            this.stateID = stateID;
        }
        
        public int stateID;

        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnFixedUpdate();
        public abstract void OnExit();
    }
}
