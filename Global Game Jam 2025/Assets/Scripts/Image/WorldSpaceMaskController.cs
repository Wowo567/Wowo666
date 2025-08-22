using System;
using Comic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]  // 让脚本在编辑器模式下运行
[RequireComponent(typeof(SpriteRenderer))]
public class WorldSpaceMaskController : MonoBehaviour
{
    private bool _ignore = false;
    private SpriteRenderer maskRenderer;  // 拖入场景中的 Mask 物体
    private Material material;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        maskRenderer = transform.parent.GetComponentInChildren<BrushTextureSprite>().GetComponent<SpriteRenderer>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        /*_ignore = gameObject.name.ToUpper().Contains("BG")||gameObject.name.ToUpper().Contains("BUBBLE")||gameObject.name.ToUpper().Contains("IGNORE");
        string shaderName = _ignore ? "SpriteMaskShader_Background" : "SpriteMaskShader";
        _spriteRenderer.material = new Material(Shader.Find("Custom/"+shaderName));

        // 获取贴图
        Texture mainTexture = _spriteRenderer.sprite.texture;
        _spriteRenderer.sharedMaterial.SetTexture("_MainTex", mainTexture);
        material = GetComponent<SpriteRenderer>().sharedMaterial;  // 获取共享材质
        */
    }

    public void Init(bool ignore)
    {
        _ignore = ignore || gameObject.name.ToUpper().Contains("BG")||gameObject.name.ToUpper().Contains("BUBBLE");
        string shaderName = _ignore ? "SpriteMaskShader_Background" : "SpriteMaskShader";
        _spriteRenderer.material = new Material(Shader.Find("Custom/"+shaderName));

        // 获取贴图
        Texture mainTexture = _spriteRenderer.sprite.texture;
        _spriteRenderer.sharedMaterial.SetTexture("_MainTex", mainTexture);
        material = GetComponent<SpriteRenderer>().sharedMaterial;  // 获取共享材质
    }

  
    void Start()
    {
        ApplyMaskSettings();
    }

    void Update()
    {
        ApplyMaskSettings();
    }

    void OnValidate()
    {
        ApplyMaskSettings();  // 在 Inspector 变更参数时，立即更新 Mask
    }

    void OnEnable()
    {
        ApplyMaskSettings();  // 确保进入 Play Mode 或 退出 Play Mode 时参数正确
    }
    
    void ApplyMaskSettings()
    {
        if (maskRenderer == null) return;
        
        
        if (material != null)
        {
            // 获取 Mask 贴图
            Texture maskTexture = maskRenderer.sprite.texture;
            material.SetTexture("_MaskTex", maskTexture);

            // 获取 Mask 的世界坐标和大小
            Bounds maskBounds = maskRenderer.bounds;
            Vector3 maskPos = maskBounds.center;
            Vector3 maskSize = maskBounds.size;

            // 传递给 Shader
            material.SetVector("_MaskPos", new Vector4(maskPos.x, maskPos.y, 0, 0));
            material.SetVector("_MaskSize", new Vector4(maskSize.x, maskSize.y, 1, 1));
        }
    }
}