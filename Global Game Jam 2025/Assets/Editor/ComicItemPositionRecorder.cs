using UnityEditor;
using UnityEngine;
using Comic;

public class ComicItemPositionRecorder : EditorWindow
{
    [MenuItem("Tools/Record ComicItem Positions and Scale")]
    public static void RecordAllComicItemTransforms()
    {
        ComicItem[] comicItems = GameObject.FindObjectsOfType<ComicItem>();

        if (comicItems.Length == 0)
        {
            Debug.LogWarning("没有找到任何 ComicItem 脚本挂载的物体！");
            return;
        }

        int modifiedCount = 0;

        foreach (ComicItem item in comicItems)
        {
            GameObject go = item.gameObject;

            // 记录位置（调用你的方法）
            item.RecordPosition();

            // 记录 scale
            Vector3 scale = go.transform.localScale;

            // 记录 prefab 修改
            PrefabUtility.RecordPrefabInstancePropertyModifications(go);

            // 应用修改到 prefab 实例
            PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);

            modifiedCount++;

            Debug.Log($"记录对象：{go.name}, Position: {item.recordedPosition}, Scale: {scale}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"记录完成，共处理 {modifiedCount} 个 ComicItem 实例。");
    }
}