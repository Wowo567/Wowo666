  using DG.Tweening;
using UnityEngine;

namespace WowoFramework.TransformEx
{
    public class DelayFollower : MonoBehaviour
    {
        public float distanceOffset = 1.0f;
        public float angleOffset    = 45;
    
        public Transform followTarget;

        private Tween tween_angle;
        private Tween tween_distance;

        private void Awake()
        {
            transform.SetParent(null);
        }

        private void Update()
        {
            Vector3 XZForward = Vector3.ProjectOnPlane(followTarget.forward, Vector3.up).normalized;
            float angle = Vector3.Angle(transform.forward, XZForward);
            float distance = Vector3.Distance(transform.position, followTarget.position);

            if (angle > angleOffset || distance > distanceOffset)
            {
                float angleY = Vector3.SignedAngle(Vector3.forward,XZForward,  Vector3.up);
                tween_angle.Kill();
                tween_angle = transform.DORotate(new Vector3(0,angleY,0), 0.66f);
                tween_distance.Kill();
                tween_distance = transform.DOMove(followTarget.position, 0.66f);
            }
        }
    }
}
