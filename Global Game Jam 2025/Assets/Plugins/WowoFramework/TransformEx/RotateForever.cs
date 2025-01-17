using UnityEngine;

namespace WowoFramework.TransformEx
{
    public class RotateForever : MonoBehaviour
    {
        [SerializeField,Tooltip("旋转轴")]
        private AxisType axis = AxisType.X;
        [SerializeField,Tooltip("每秒旋转的角度")]
        private float speed = 30;
    
        private float angle = 0;
    
        void Update()
        {
            angle += (speed * Time.deltaTime + 360) % 360;
            switch (axis)
            {
                case AxisType.X:
                    transform.localRotation = Quaternion.Euler(angle, 0, 0);
                    break;
                case AxisType.Y:
                    transform.localRotation = Quaternion.Euler(0, angle, 0);
                    break;
                case AxisType.Z:
                    transform.localRotation = Quaternion.Euler(0, 0, angle);
                    break;
            }
        }
    }

    public enum AxisType
    {
        X,
        Y,
        Z
    }
}