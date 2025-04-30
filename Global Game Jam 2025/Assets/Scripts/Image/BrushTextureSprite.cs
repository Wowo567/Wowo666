using System;
using Comic;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;

public enum BrushType
{
    Grey = 0,
    Color = 1
}

public class BrushTextureSprite : MonoBehaviour
{
    private const string MaskSpritePath = "Masks/Mask";

    [SerializeField] private SpriteRenderer targetSprite; // 目标 SpriteRenderer
    [SerializeField] private float brushRadius = 130f;    // 画笔半径
    [SerializeField] private float targetBrushTime = 2f; // 目标显示时间：秒
    private float speed; // 不用再序列化了

    private Texture2D _texture;         // 可修改的纹理
    public Texture2D originalTexture { get; private set; }   // 存储原始纹理
    
    private Vector2 _brushPosition;     // 画笔坐标
    private Vector2 _direction = Vector2.one; // 方向向量
    private Vector2 _offset;

    private bool _isBrush = true;
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
    
    private int _revealedPixels = 0;   // 记录已显示像素数
    private int _totalPixels;   
    private bool _isFinish = false;

    private ComicItem _comicItem;
    private BrushType _type = BrushType.Color;

    private Transform _brushTrans;
    private Vector3 _pivotOffset;
    private int _frameCounter = 0;
    private Vector3 _previousBrushPosition;

    // 缓存常用计算结果
    private Color[] _originalPixels;
    private Color[] _texturePixels;
    private Color _transparentColor = new Color(0, 0, 0, 0);
    
    private int _noChangeFrameCount = 0;
    private const int MaxNoChangeFrames = 30; // 超过30帧都没变，就当作完成

    private void Awake()
    {
        _comicItem = GetComponentInParent<ComicItem>();
    }

    public void GetOriginalTexture()
    {
        if (originalTexture == null)
        {
            string resourcePath = MaskSpritePath + _comicItem.ComicData.id;
            Debug.Log($"GetOriginalTexture: {resourcePath}");
            
            Sprite maskSprite = Resources.Load<Sprite>(resourcePath);
            if (maskSprite == null)
            {
                Debug.LogError($"Failed to load sprite at path: {resourcePath}");
                return;
            }
            
            targetSprite.sprite = maskSprite;
            originalTexture = Instantiate(targetSprite.sprite.texture);
            _originalPixels = originalTexture.GetPixels();
        }
    }

    private void Start()
    {
        if (Enum.TryParse(transform.parent.name, out BrushType type))
        {
            _type = type;
        }
        else
        {
            Debug.LogWarning($"Failed to parse BrushType from {transform.parent.name}");
        }
        
        OnStart();
        
        if (_type == BrushType.Color)
        {
            enabled = false;
        }
    }
    
    public void OnStart()
    {
        GetOriginalTexture();
        
        if (originalTexture == null)
        {
            Debug.LogError("Original texture is null. Cannot continue.");
            enabled = false;
            return;
        }
        
        _texture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        _texturePixels = new Color[_texture.width * _texture.height];
        _totalPixels = CountNonTransparentPixels();
        
        // ✅ 根据图片尺寸设置速度
        float diag = Mathf.Sqrt(_texture.width * _texture.width + _texture.height * _texture.height);
        speed = 4 * diag / targetBrushTime;
        
        Clear(true);
        UpdateSpriteTexture();
        Init();
    }
    
    private int CountNonTransparentPixels()
    {
        int count = 0;
        foreach (Color pixel in _originalPixels)
        {
            if (pixel.a != 0f) // alpha 透明度大于0
            {
                count++;
            }
        }
        return count;
    }

    public void Clear(bool changeFinish = false)
    {
        _isFinish = !changeFinish;
        _revealedPixels = 0;

        // 填充为透明色
        for (int i = 0; i < _texturePixels.Length; i++)
        {
            _texturePixels[i] = _transparentColor;
        }
   
        _texture.SetPixels(_texturePixels);
        _texture.Apply();
    }

    public void ShowAll()
    {
        _isFinish = true;
        _revealedPixels = _totalPixels;

        // 直接拷贝原始纹理的像素数据
        _texture.SetPixels(_originalPixels);
        _texture.Apply();

        if (_brushTrans != null)
        {
            _brushTrans.gameObject.SetActive(false);
        }
    }

    public void Init()
    {
        // 初始化画笔位置
        _brushPosition = new Vector2(0, _texture.height - 1);
        _offset = new Vector2(brushRadius, -brushRadius);
        _revealedPixels = 0;
        _isFinish = false;
        Debug.Log(_brushPosition+"init _brushPosition"+_type);

        // 更新笔刷状态
        UpdateBrushTransform();
    }

    private void Update()
    {
        if (_isFinish)
        {
            enabled = false;
            return;
        }
        
        // 检查完成状态
        if (_revealedPixels >= 0.98f * _totalPixels || _noChangeFrameCount >= MaxNoChangeFrames)
        {
            if (!_isFinish)
            {
                Debug.Log("图片显示完全！" + _comicItem.id);
                _isFinish = true;
                ShowFinish();
            }
            return;
        }

        _brushTrans.gameObject.SetActive(true);
        
        // 移动画笔
        _brushPosition += _direction * speed * 0.016f;//不使用Time.deltaTime是担心一帧移动距离过大
        
        // 反弹检测
        CheckBounce();

        // 画出图像
        DrawTexture((int)_brushPosition.x, (int)_brushPosition.y, (int)brushRadius);
        
        UpdateBrushTransform();
    }

    private void CheckBounce()
    {
        if (_brushPosition.x < 0 || _brushPosition.x > _texture.width ||
            _brushPosition.y < 0 || _brushPosition.y > _texture.height)
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
            _noChangeFrameCount = 0;
        }
        else
        {
            _noChangeFrameCount++;
        }
    }

    private void UpdateBrushTransform()
    {
        if (_brushTrans == null)
        {
            _brushTrans = isBrush ? PaperManager.Instance.Pen : PaperManager.Instance.Eraser;
            if (_brushTrans == null) return;
        }

        if (!_brushTrans.gameObject.activeSelf) return;

        Bounds bounds = targetSprite.bounds;

        // 将画笔的纹理坐标映射到世界坐标
        float worldX = bounds.min.x + (_brushPosition.x / _texture.width) * bounds.size.x;
        float worldY = bounds.min.y + (_brushPosition.y / _texture.height) * bounds.size.y;

        Vector3 worldPos = new Vector3(worldX, worldY, bounds.center.z);

        // 💡加一个与 brushRadius 成比例的右下角偏移，平衡偏差
        float pixelPerUnit = targetSprite.sprite.pixelsPerUnit;
        float unitOffset = brushRadius / pixelPerUnit;

        // 这里的偏移量用世界单位补偿，右下是 (+x, -y)
        Vector3 radiusOffset = new Vector3(unitOffset, -unitOffset, 0f);

        worldPos += radiusOffset/2;

        // 平滑位置更新
        if (_frameCounter % 2 == 0)
        {
            _previousBrushPosition = _brushTrans.position;
        }

        _brushTrans.position = Vector3.Lerp(_previousBrushPosition, worldPos, 0.5f);
        _frameCounter++;
    }




    private void ShowFinish()
    {
        _comicItem.BrushFinish(_type, isBrush);
        if (_brushTrans != null)
        {
            _brushTrans.gameObject.SetActive(false);
        }
    }
    
    private void UpdateSpriteTexture()
    {
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        targetSprite.sprite = Sprite.Create(_texture, targetSprite.sprite.rect, pivot);
    }
}