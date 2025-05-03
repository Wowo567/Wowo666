using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class RemoveMissingScripts : EditorWindow
{
    //目前只有右键Prefab有效
    [MenuItem("Assets/Remove Missing Scripts", false, 20)]
    private static void RemoveMissingScriptsFromSelection()
    {
        Object[] selectedAssets = Selection.objects;
        List<GameObject> prefabsToFix = new List<GameObject>();

        foreach (Object obj in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            
            // 检查是否是文件夹
            if (Directory.Exists(assetPath))
            {
                // 获取文件夹中所有的预制体
                string[] prefabsInFolder = Directory.GetFiles(assetPath, "*.prefab", SearchOption.AllDirectories);
                foreach (string prefabPath in prefabsInFolder)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        prefabsToFix.Add(prefab);
                    }
                }
            }
            // 检查是否是预制体
            else if (obj is GameObject && assetPath.EndsWith(".prefab"))
            {
                prefabsToFix.Add(obj as GameObject);
            }
        }

        int totalPrefabsFixed = 0;
        int totalScriptsRemoved = 0;

        // 处理所有找到的预制体
        foreach (GameObject prefab in prefabsToFix)
        {
            int scriptsRemoved = RemoveMissingScriptsFromGameObject(prefab);
            
            if (scriptsRemoved > 0)
            {
                totalPrefabsFixed++;
                totalScriptsRemoved += scriptsRemoved;
                Debug.Log($"已修复预制体: {AssetDatabase.GetAssetPath(prefab)} - 移除了 {scriptsRemoved} 个缺失脚本");
            }
        }

        if (totalScriptsRemoved > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"总计: 修复了 {totalPrefabsFixed} 个预制体，移除了 {totalScriptsRemoved} 个缺失脚本");
        }
        else
        {
            Debug.Log("没有找到需要修复的预制体或没有缺失脚本");
        }
    }

    private static int RemoveMissingScriptsFromGameObject(GameObject prefab)
    {
        int removedCount = 0;

        // 如果这是一个预制体资源，我们需要打开它进行编辑
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        bool isPrefabAsset = !string.IsNullOrEmpty(prefabPath);
        
        GameObject prefabInstance = null;
        
        if (isPrefabAsset)
        {
            // 创建临时实例以进行编辑
            prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
        }
        else
        {
            prefabInstance = prefab;
        }

        // 获取所有组件（包括子对象的组件）
        Transform[] transforms = prefabInstance.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in transforms)
        {
            // 计算并移除缺失的脚本
            int compCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            if (compCount > 0)
            {
                removedCount += compCount;
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
        }

        if (isPrefabAsset && removedCount > 0)
        {
            // 保存更改回预制体资源
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }

        return removedCount;
    }
}