using System;
using Comic;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;

/// <summary>
/// 不使用Mask的笔刷系统
/// </summary>
public class BrushTextureSimple : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetSprite; // 目标 SpriteRenderer
    [SerializeField] private float brushRadius = 150f;    // 画笔半径
    [SerializeField] private float speed = 3500f;         // 画笔移动速度

    private Texture2D _texture;          // 可修改的纹理
    public Texture2D originalTexture { get; private set; }   // 存储原始纹理
    
    private Vector2 _brushPosition;      // 画笔坐标
    private Vector2 _direction = Vector2.one; // 方向向量
    private Vector2 _offset;

    private bool _isBrush = false;
    [ShowInInspector]
    public bool isBrush
    {
        get => _isBrush;
        set
        {
            _revealedPixels = 0;
            Init();
            _isBrush = value;
        }
    }
    
    private int _revealedPixels = 0;    // 记录已显示像素数
    private int _totalPixels;   
    private bool _isFinish = false;
    
    public Action OnShowFinish;
    
    private Transform _brushTrans;
    private Vector3 _pivotOffset;
    private int _frameCounter = 0;
    private Vector3 _previousBrushPosition;
    
    // 缓存计算结果
    private Color[] _originalPixels;
    private Color[] _texturePixels;
    private Color _transparentColor = new Color(0, 0, 0, 0);

    private void Start()
    {
        if (targetSprite == null)
        {
            targetSprite = GetComponent<SpriteRenderer>();
            if (targetSprite == null)
            {
                Debug.LogError("No SpriteRenderer found for BrushTextureSimple");
                enabled = false;
                return;
            }
        }

        if (targetSprite.sprite == null || targetSprite.sprite.texture == null)
        {
            Debug.LogError("SpriteRenderer has no valid sprite or texture");
            enabled = false;
            return;
        }

        InitializeTextures();
        Init();
    }

    private void InitializeTextures()
    {
        originalTexture = Instantiate(targetSprite.sprite.texture);
        _originalPixels = originalTexture.GetPixels();
        
        _texture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        _texturePixels = new Color[_texture.width * _texture.height];
        
        // 复制像素
        Array.Copy(_originalPixels, _texturePixels, _originalPixels.Length);
        _texture.SetPixels(_texturePixels);
        _texture.Apply();

        _totalPixels = CountNonTransparentPixels();
        
        // 生成新的 Sprite 并替换原来的
        UpdateSpriteTexture();
    }

    private int CountNonTransparentPixels()
    {
        int count = 0;
        foreach (Color pixel in _originalPixels)
        {
            if (pixel.a > 0) // alpha 透明度大于0
            {
                count++;
            }
        }
        return count;
    }

    private void Init()
    {
        // 初始化画笔位置
        _brushPosition = new Vector2(0, _texture.height - 1);
        _offset = new Vector2(brushRadius, -brushRadius);
        _revealedPixels = 0;
        _isFinish = false;
    }

    private void Update()
    {
        if (_isFinish) return;
        
        // 确保获取画笔
        if (_brushTrans == null) GetBrushTrans();
        if (_brushTrans == null) return;
        
        // 更新画笔显示状态
        bool shouldBrushBeActive = _revealedPixels < 0.95f * _totalPixels;
        _brushTrans.gameObject.SetActive(shouldBrushBeActive);
        
        // 移动画笔
        _brushPosition += _direction * speed * Time.deltaTime;

        // 检查完成状态
        if (_revealedPixels >= 0.98f * _totalPixels)
        {
            if (!_isFinish)
            {
                Debug.Log("图片显示完全！");
                _isFinish = true;
                ShowFinish();
            }
            return;
        }

        // 反弹检测
        CheckBounce();

        // 画出图像
        DrawTexture((int)_brushPosition.x, (int)_brushPosition.y, (int)brushRadius);

        // 如果画笔不活跃，跳过位置更新
        if (!_brushTrans.gameObject.activeSelf) return;

        UpdateBrushPosition();
    }

    private void CheckBounce()
    {
        if (_brushPosition.x <= 0 || _brushPosition.x >= _texture.width ||
            _brushPosition.y <= 0 || _brushPosition.y >= _texture.height)
        {
            _direction *= -1; // 反转方向
            _brushPosition += _offset;
            _brushPosition.x = Mathf.Clamp(_brushPosition.x, 0, _texture.width - 1);
            _brushPosition.y = Mathf.Clamp(_brushPosition.y, 0, _texture.height - 1);
        }
    }

    private void DrawTexture(int x, int y, int radius)
    {
        int radiusSquared = radius * radius;
        bool pixelsModified = false;
        
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                int px = x + i;
                int py = y + j;

                // 越界检查
                if (px < 0 || px >= _texture.width || py < 0 || py >= _texture.height)
                    continue;
                
                // 使用平方计算优化距离检测
                if (i * i + j * j >= radiusSquared)
                    continue;
                
                int pixelIndex = py * _texture.width + px;
                Color currentColor = _texturePixels[pixelIndex];
                Color originalColor = _originalPixels[pixelIndex];

                if (isBrush)
                {
                    if (currentColor.a == 0 && originalColor.a != 0)
                    {
                        _revealedPixels++;
                        _texturePixels[pixelIndex] = originalColor;
                        pixelsModified = true;
                    }
                }
                else
                {
                    if (currentColor.a != 0)
                    { 
                        _revealedPixels++;
                        _texturePixels[pixelIndex] = _transparentColor;
                        pixelsModified = true;
                    }
                }
            }
        }

        if (pixelsModified)
        {
            _texture.SetPixels(_texturePixels);
            _texture.Apply();
        }
    }

    private void UpdateBrushPosition()
    {
        // 计算画笔世界位置
        Vector3 worldPos = targetSprite.transform.TransformPoint(
            new Vector3(
                (_brushPosition.x / _texture.width) * targetSprite.sprite.bounds.size.x,
                (_brushPosition.y / _texture.height) * targetSprite.sprite.bounds.size.y,
                0));
        
        worldPos -= _pivotOffset;

        // 每两帧更新一次画笔位置，实现平滑移动
        if (_frameCounter % 2 == 0)
        {
            _previousBrushPosition = _brushTrans.position;
        }
        _brushTrans.position = Vector3.Lerp(_previousBrushPosition, worldPos, 0.5f);

        _frameCounter++;
    }

    private void ShowFinish()
    {
        OnShowFinish?.Invoke();
        DestroyImmediate(gameObject);
    }
    
    private Transform GetBrushTrans()
    {
        if (PaperManager.Instance == null)
        {
            Debug.LogError("PaperManager.Instance is null");
            return null;
        }
        
        _brushTrans = PaperManager.Instance.Eraser;
        if (_brushTrans == null)
        {
            Debug.LogError("Could not find Eraser in PaperManager");
            return null;
        }
        
        SpriteRenderer brushRenderer = _brushTrans.GetComponentInChildren<SpriteRenderer>();
        if (brushRenderer != null && brushRenderer.sprite != null)
        {
            _pivotOffset = Vector3.Scale(brushRenderer.sprite.bounds.extents, _brushTrans.transform.lossyScale);
        }
        
        return _brushTrans;
    }
    
    private void UpdateSpriteTexture()
    {
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        targetSprite.sprite = Sprite.Create(_texture, targetSprite.sprite.rect, pivot);
    }
}