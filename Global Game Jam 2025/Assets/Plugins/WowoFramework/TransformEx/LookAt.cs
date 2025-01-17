using UnityEngine;

namespace WowoFramework.TransformEx
{
    public class LookAt : MonoBehaviour
    {
        [SerializeField]
        private bool      lookAtCamera = true;
        [SerializeField]
        private bool      reverse = true;
        private  Transform target;

        private void Start()
        {
            if (lookAtCamera)
            {
                if (Camera.main != null) SetNewTarget(Camera.main.transform);
            }
        }

        public void SetNewTarget(Transform _target)
        {
            this.target = _target;
        }
    
        void Update()
        {
            if (target)
            {
                transform.forward = (target.transform.position - transform.position).normalized*(reverse?-1:1);
            }
        }
    }
}
