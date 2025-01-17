using UnityEngine;

namespace WowoFramework.TransformEx
{
    public class TargetFollower : MonoBehaviour
    {
        public Transform target;
        [Space]
        public bool isLocal;
        [Space]
        public bool position;
        public bool rotation;

        private void Update()
        {
            if (position)
            {
                if (isLocal)
                {
                    transform.localPosition = target.localPosition;
                }
                else
                {
                    transform.position = target.position;
                }
            }

            if (rotation)
            {
                if (isLocal)
                {
                    transform.localRotation = target.localRotation;
                }
                else
                {
                    transform.rotation = target.rotation;
                }
            }
        }
    }
}
