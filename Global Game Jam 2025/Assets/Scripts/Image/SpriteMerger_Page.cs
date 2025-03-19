using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Comic;
using UI;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using WowoFramework.Singleton;

public class SpriteMerger_Page : MonoBehaviourSingleton<SpriteMerger_Page>
{
    private Transform _newSpriteRenderer; // 需要变成最新的SpriteRenderer的物体
    //public Vector2 offest;

    private bool _started;
    private List<SpriteRenderer> _spriteRenderers;
    private Vector3 _originalCenter;
    private float _newSpritePosZ = -3;
    
    private void Prepare()
    {
        _started = true;
        Debug.Log("SpriteMerger_Page 准备合成...");
        //在当前Paper下生成这张图片
        GameObject newObj = new GameObject("SpriteMerger_Page"); 
        newObj.transform.SetParent(PaperManager.Instance.CurPaper);
        _newSpriteRenderer = newObj.transform;
        foreach (var sr in _spriteRenderers)
        {
            sr.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    public void MergeAndReplace(List<SpriteRenderer> spriteRenderers)
    {       
        if (_started) return;
        _spriteRenderers = spriteRenderers;
        Prepare();
        Texture2D mergedTexture = MergeSprites();
        if (mergedTexture == null)
        {
            Debug.LogError("错误: MergeSprites() 失败，未生成有效的贴图！");
            return;
        }
        Debug.Log("MergeSprites 执行完毕");

        //获取或添加 `SpriteRenderer`
        SpriteRenderer sr = _newSpriteRenderer.GetComponent<SpriteRenderer>();
        if (sr == null)
        { 
            Debug.Log("未找到 SpriteRenderer，正在添加...");
            sr = _newSpriteRenderer.gameObject.AddComponent<SpriteRenderer>();
        }
        Debug.Log("SpriteRenderer 获取/创建完成");

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
        
        // 创建新的 `Sprite` 并赋值
        sr.sprite = Sprite.Create(mergedTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));

        // **设置新物体的位置为原中心点**
        _newSpriteRenderer.position = new Vector3(_originalCenter.x, _originalCenter.y, _newSpritePosZ);

        Debug.Log("开始隐藏子物体...");

        // 移除除了当前漫画格所有图片
        foreach (var item  in spriteRenderers)
        {
            if (item != null && item.GetComponentInParent<ComicItem>() != null)
            {
                ComicItem comicItem = item.GetComponentInParent<ComicItem>();
               // if (comicItem != ComicManager.Instance.curComic)
               //{
                    comicItem.Remove();
               // }
            }
        }

        Debug.Log("合成完成！");
        
        //挂载橡皮擦
        StartCoroutine(AttachEraserNextFrame());
        
    }
    
    private IEnumerator AttachEraserNextFrame()
    {
        yield return null; // 等待下一帧

        // 挂载橡皮擦
        _newSpriteRenderer.AddComponent<BrushTextureSimple>().OnShowFinish += OnShowFinish;
        Debug.Log("下一帧：橡皮擦已挂载！");
    }

    void OnShowFinish()
    {
        ComicManager.Instance.curComic.BubbleShow();
    }

  private Texture2D MergeSprites()
    {
        // 获取父物体的 scale
        Vector3 parentScale = transform.lossyScale;

        // **计算合成区域的包围盒**
        Bounds bounds = GetBounds();
        _originalCenter = bounds.center;
        
        // 考虑父物体的缩放，调整纹理尺寸
        // **增加 100 像素的透明边框**
        int borderSize = 100;
        int textureWidth = Mathf.CeilToInt(bounds.size.x * 100 / parentScale.x) + 2 * borderSize;
        int textureHeight = Mathf.CeilToInt(bounds.size.y * 100 / parentScale.y) + 2 * borderSize;

        // **创建 RenderTexture**
        RenderTexture rt = new RenderTexture(textureWidth, textureHeight, 32, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 8;
        RenderTexture.active = rt;

        // **创建临时 Camera 进行渲染**
        GameObject tempCamObj = new GameObject("TempCamera");
        Camera cam = tempCamObj.AddComponent<Camera>();

        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = new Color(0, 0, 0, 0); // 透明背景
        cam.orthographic = true;
        cam.orthographicSize = bounds.size.y / 2 + borderSize / 100f;;
        cam.targetTexture = rt;
        //cam.transform.position = new Vector3(bounds.center.x + offest.x, bounds.center.y + offest.y, -10);
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10);

        // **临时修改 Layer 让相机只渲染这些 SpriteRenderer**
        List<int> originalLayers = new List<int>();
        foreach (var sr in _spriteRenderers)
        {
            originalLayers.Add(sr.gameObject.layer);
            sr.gameObject.layer = LayerMask.NameToLayer("MergeLayer");
        }
        cam.cullingMask = 1 << LayerMask.NameToLayer("MergeLayer");

        // **渲染目标 `SpriteRenderer`**
        cam.Render();

        // **读取 `RenderTexture` 转换为 `Texture2D`**
        Texture2D resultTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        resultTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        resultTexture.Apply();

        // **还原原始 Layer**
        int index = 0;
        foreach (var sr in _spriteRenderers)
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
    private Bounds GetBounds()
    {
        if (_spriteRenderers == null || _spriteRenderers.Count == 0)
        {
            Debug.LogError("没有找到 SpriteRenderer 列表");
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        // **1. 获取世界坐标范围**
        Vector2 minWorldPos = Vector2.positiveInfinity;
        Vector2 maxWorldPos = Vector2.negativeInfinity;

        foreach (var sr in _spriteRenderers)
        {
            Bounds bounds = sr.bounds;
            minWorldPos = Vector2.Min(minWorldPos, bounds.min);
            maxWorldPos = Vector2.Max(maxWorldPos, bounds.max);
        }

        // **2. 计算最终 `Bounds`**
        Vector3 center = (minWorldPos + maxWorldPos) / 2;
        Vector3 size = maxWorldPos - minWorldPos;
        return new Bounds(center, size);
    }
}
