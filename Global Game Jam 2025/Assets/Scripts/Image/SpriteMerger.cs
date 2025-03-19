using System;
using UnityEngine;
using System.Collections.Generic;
using Comic;

public class SpriteMerger : MonoBehaviour
{
    public Transform targetParent; // 需要合成的 SpriteRenderer 组的父物体
    public SpriteRenderer NewSpriteRenderer { get; private set; }

    //public Vector2 offest;
    private bool _started;
    private Vector3 _originalCenter;
    private float _newSpritePosZ = -0.4f;
    private Material _overwriteMaterial;
    private void Prepare()
    { 
        _started = true;
        Debug.Log("SpriteMerger 准备合成...");
        if (targetParent == null) targetParent = transform;
        if (NewSpriteRenderer == null)
        {
            GameObject newObj = new GameObject("SpriteMerger"); 
            newObj.transform.SetParent(targetParent.parent);
            NewSpriteRenderer = newObj.AddComponent<SpriteRenderer>();
            NewSpriteRenderer.transform.localScale = Vector3.one;
        }
    }

    public void MergeAndReplace()
    {       
        if (_started) return;
        Prepare();
        Texture2D mergedTexture = MergeSprites();
        if (mergedTexture == null)
        {
            Debug.LogError("错误: MergeSprites() 失败，未生成有效的贴图！");
            return;
        }
        Debug.Log("MergeSprites 执行完毕");
        

        if (!mergedTexture.isReadable)
        {
            Debug.LogError("错误: mergedTexture 不是可读的，尝试创建可读的副本...");
            Texture2D readableTexture = new Texture2D(mergedTexture.width, mergedTexture.height, TextureFormat.RGBA32, false);
            readableTexture.SetPixels(mergedTexture.GetPixels());
            readableTexture.Apply();
            mergedTexture = readableTexture;
        }
        // 获取 mergedTexture 的实际尺寸
        int textureWidth = mergedTexture.width;
        int textureHeight = mergedTexture.height;
        
        // **4️⃣ 创建新的 `Sprite` 并赋值**
        NewSpriteRenderer.sprite = Sprite.Create(mergedTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));
        // **设置新物体的位置为原中心点**
        NewSpriteRenderer.transform.position = new Vector3(_originalCenter.x, _originalCenter.y, targetParent.position.z+_newSpritePosZ);

        targetParent.gameObject.SetActive(false); 
        Debug.Log("合成完成！");
    }


    private Texture2D MergeSprites()
    {
        if (targetParent == null)
        {
            Debug.LogError("未指定目标父物体！");
            return null;
        }

        // 获取父物体的 scale
        Vector3 parentScale = targetParent.lossyScale;

        // **计算合成区域的包围盒**
        Bounds bounds = GetBounds(targetParent);
        _originalCenter = bounds.center;
        // 考虑父物体的缩放，调整纹理尺寸
        int textureWidth = Mathf.CeilToInt(bounds.size.x * 100 / parentScale.x);
        int textureHeight = Mathf.CeilToInt(bounds.size.y * 100 / parentScale.y);

        // **创建 RenderTexture**
        RenderTexture rt = new RenderTexture(textureWidth, textureHeight, 32, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 8;
        RenderTexture.active = rt;
        GL.Clear(true, true, new Color(0, 0, 0, 0));

        // **创建临时 Camera 进行渲染**
        GameObject tempCamObj = new GameObject("TempCamera");
        Camera cam = tempCamObj.AddComponent<Camera>();

        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = new Color(0, 0, 0, 0); // 透明背景
        cam.orthographic = true;
        cam.orthographicSize = bounds.size.y / 2;
        cam.targetTexture = rt;
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10);
        cam.transparencySortMode = TransparencySortMode.CustomAxis;
        cam.transparencySortAxis = Vector3.forward;

        // **临时修改 Layer 让相机只渲染这些 SpriteRenderer**
        List<int> originalLayers = new List<int>();
        foreach (var sr in targetParent.GetComponentsInChildren<SpriteRenderer>())
        {
            originalLayers.Add(sr.gameObject.layer);
            sr.gameObject.layer = LayerMask.NameToLayer("MergeLayer");
        }
        cam.cullingMask = 1 << LayerMask.NameToLayer("MergeLayer");


        cam.Render();

        // **读取 `RenderTexture` 转换为 `Texture2D`**
        Texture2D resultTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        resultTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        resultTexture.Apply();
        
        // **还原原始 Layer**
        int index = 0;
        foreach (var sr in targetParent.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.gameObject.layer = originalLayers[index++];
        }

        // **清理**
        RenderTexture.active = null;
        cam.targetTexture = null;
        Destroy(tempCamObj);
        rt.Release();

        Debug.Log("Sprite 合成完成！");
        return resultTexture;
    }

    // 获取父物体下所有 SpriteRenderer 的包围盒（不包含 Mask）
    private Bounds GetBounds(Transform parent)
    {
        Bounds bounds = new Bounds(parent.position, Vector3.zero);
        bool hasBounds = false;

        foreach (SpriteRenderer sr in parent.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.gameObject.name.Contains("Mask")) continue; // 忽略 Mask
            if (!hasBounds)
            {
                bounds = sr.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(sr.bounds);
            }
        }
        return bounds;
    }
}
