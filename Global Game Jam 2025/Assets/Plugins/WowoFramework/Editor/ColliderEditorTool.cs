using UnityEditor;
using UnityEngine;

namespace WowoFramework.Editor
{
    public class ColliderEditorTool : UnityEditor.Editor
    {
        [MenuItem("WowoFramework/碰撞盒工具/给选中的UI添加碰撞盒")]
        static void AddUICollider()
        {
            GameObject selectObject = Selection.activeGameObject;
            if (selectObject == null)
            {
                Debug.LogError("未选择对象！");
                return;
            }
            RectTransform rect = selectObject.GetComponentInChildren<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("未找到UI的RectTransform组件！");
                return;
            }

            Collider[] colliders = selectObject.GetComponents<Collider>();
            foreach (var collider1 in colliders)
            {
                DestroyImmediate(collider1);
            }
            Debug.Log("销毁原有碰撞盒完成");
            
            BoxCollider collider = selectObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(rect.sizeDelta.x * rect.localScale.x, rect.sizeDelta.y * rect.localScale.y,
                0.001f);
            Debug.Log("生成新碰撞盒完成");
        }
        
        [MenuItem("WowoFramework/碰撞盒工具/给选中的2D物体添加碰撞盒")]
        static void Add2DCollider()
        {
            GameObject selectObject = Selection.activeGameObject;
            if (selectObject == null)
            {
                Debug.LogError("未选择对象！");
                return;
            }

            Collider[] colliders = selectObject.GetComponents<Collider>();
            foreach (var collider1 in colliders)
            {
                DestroyImmediate(collider1);
            }
            Debug.Log("销毁原有碰撞盒完成");
            
            BoxCollider collider = selectObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1, 1 , 0.001f);
            Debug.Log("生成新碰撞盒完成");
        }
    }
}
