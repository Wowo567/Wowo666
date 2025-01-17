using UnityEngine;

namespace WowoFramework.Singleton
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<T>();
                }

                return _instance;
            }
        }
        private static T _instance;

    
        protected virtual void Awake()
        {
            _instance = this as T;
        }
    }
}