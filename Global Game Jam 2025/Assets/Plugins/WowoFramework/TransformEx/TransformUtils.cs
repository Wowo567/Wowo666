using UnityEngine;

namespace WowoFramework.TransformEx
{
    public static class TransformUtils
    {
        public static void DestroyChildren(this Transform transform)
        {
            if (transform == null)
            {
                Debug.LogError("Transform is null.");
                return;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                Object.DestroyImmediate(child);
            }
        }
        
        public static void ResetAllLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}