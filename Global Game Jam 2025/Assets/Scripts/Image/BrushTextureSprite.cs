using System;
using Comic;
using Sirenix.OdinInspector;
using UI;
using Unity.VisualScripting;
using UnityEngine;

public enum BrushType
{
    Grey = 0,
    Color = 1
}

public class BrushTextureSprite : MonoBehaviour
{
    private const string MaskSpritePath = "Masks/Mask";
    public SpriteRenderer targetSprite; // 目标 SpriteRenderer
    private Texture2D _texture;          // 可修改的纹理
    public Texture2D originalTexture{ get; private set; }   // 存储原始纹理
    public float brushRadius = 130f;    // 画笔半径
    public float speed = 2500;         // 画笔移动速度
    private Vector2 _brushPosition;      // 画笔坐标
    private Vector2 _direction = new(1, 1); // 方向向量
    private Vector2 _offset;

    private bool _isBrush = true;
    [ShowInInspector]
    public bool isBrush
    {
        get => _isBrush;
        set
        {
            _revealedPixels = 0;
            ChangeBrush();
            _isBrush = value;
        }
    }
    
    private int _revealedPixels = 0; // 记录已显示像素数
    private int _totalPixels;   
    private bool _isFinish = false;

    private ComicItem _comicItem;
    private BrushType _type = BrushType.Color;

    private Transform _brushTrans;
    private Vector3 _pivotOffset;
    //private Vector3 _brushOffset = new (0.9f, 1.2f, -5);
    private int _frameCounter = 0;
    private Vector3 _previousBrushPosition;
    private void Awake()
    {
        _comicItem = GetComponentInParent<ComicItem>();
    }

    public void GetOriginalTexture()
    {
        if (originalTexture == null)
        {
            targetSprite.sprite = Resources.Load<Sprite>(MaskSpritePath + _comicItem.ComicData.id);
            // 获取原始纹理，并创建一个可修改的副本
            originalTexture = Instantiate(targetSprite.sprite.texture); // 存储原始纹理
        }
    }
    void Start()
    {
        _type = Enum.Parse<BrushType>(transform.parent.name);
        OnStart();
        
        if (_type == BrushType.Color)
        {
            enabled = false;
        }
    }
    
    public void OnStart()
    {
        GetOriginalTexture();
        _texture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        _totalPixels = CountNonTransparentPixels(originalTexture);//但是出现的是蒙版，直接乘以变长就行
        // **初始化 Texture 为全透明**
        Clear();
        // 生成新的 Sprite 并替换原来的
        targetSprite.sprite = Sprite.Create(_texture, targetSprite.sprite.rect, new Vector2(0.5f, 0.5f));

        Init();
    }
    
    int CountNonTransparentPixels(Texture2D tex)
    {
        Color[] pixels = tex.GetPixels();
        int count = 0;

        foreach (Color pixel in pixels)
        {
            if (pixel.a > 0) // alpha 透明度大于0
            {
                count++;
            }
        }

        return count;
    }

    public void Clear()
    {
        _isFinish = true;

        Color[] pixels = new Color[_texture.width * _texture.height];

        // **填充整个数组为 Color.white**
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] =  new Color(0, 0, 0, 0);
        }
   
        // **一次性设置所有像素**
        _texture.SetPixels(pixels);
        _texture.Apply();
    }

    public void ShowAll()
    {
        _isFinish = true;

        // **复制 originalTexture 的像素数据**
        Color[] pixels = originalTexture.GetPixels();

        // **更新当前可修改的 texture**
        _texture.SetPixels(pixels);
        _texture.Apply();
        
        GetBrushTrans().gameObject.SetActive(false);
    }

    public void Init()
    {
        // 初始化画笔位置
        _brushPosition = new Vector2(0, _texture.height-1);
        _offset = new Vector2(brushRadius, -brushRadius);
        _revealedPixels = 0;
        _isFinish = false;
    }

    void Update()
    {
        if (_isFinish) return;
        if (_brushTrans == null) GetBrushTrans();
        
        bool shouldBrushBeActive = _revealedPixels < 0.95f * _totalPixels;

        if (_brushTrans.gameObject.activeSelf != shouldBrushBeActive)
        {
            _brushTrans.gameObject.SetActive(shouldBrushBeActive);
        }
        
        // 移动画笔
        _brushPosition += _direction * speed * Time.fixedDeltaTime;

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
        if (_brushPosition.x <= 0 || _brushPosition.x >= _texture.width
                                  || _brushPosition.y <= 0 || _brushPosition.y >= _texture.height)
        {
            Bounce();
        }

        // 画出图像
        DrawTexture((int)_brushPosition.x, (int)_brushPosition.y, (int)brushRadius);

        if (!_brushTrans.gameObject.activeSelf) return; // 这帧没有绘制，跳过画笔更新

        // **修正 worldPos 计算**
        Vector3 worldPos = targetSprite.transform.TransformPoint(
            new Vector3(
                (_brushPosition.x / _texture.width) * targetSprite.sprite.bounds.size.x,
                (_brushPosition.y / _texture.height) * targetSprite.sprite.bounds.size.y,
                0));

        worldPos -= _pivotOffset;
        //worldPos += _brushOffset;

        // **每两帧更新一次画笔位置**
        if (_frameCounter % 2 == 0)
        {
            _previousBrushPosition = _brushTrans.position; // 记录上一帧位置
        }
        _brushTrans.position = Vector3.Lerp(_previousBrushPosition, worldPos, 0.5f); // Lerp 平滑过渡

        _frameCounter++; // 递增帧计数
    }

    void Bounce()
    {
        _direction.x *= -1;
        _direction.y *= -1;
        _brushPosition += _offset;
        _brushPosition.x = Mathf.Clamp(_brushPosition.x, 0, _texture.width-1);
        _brushPosition.y = Mathf.Clamp(_brushPosition.y, 0, _texture.height-1);
    }

    void DrawTexture(int x, int y, int radius)
    {
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int px = x + i;
                    int py = y + j;

                    // 确保不越界
                    if (px >= 0 && px < _texture.width && py >= 0 && py < _texture.height)
                    {
                        float dist = Mathf.Sqrt(i * i + j * j);
                        if (dist < radius)
                        {
                            Color originalColor = originalTexture.GetPixel(px, py); // 原始颜色
                            Color currentColor = _texture.GetPixel(px, py); // 当前颜色
                            
                            if (isBrush)
                            {
                                if (currentColor.a == 0&&originalColor.a!=0)
                                {
                                    _revealedPixels++; // 计数增加
                                    _texture.SetPixel(px, py, originalColor);
                                }
                            }
                            else
                            {
                                if (currentColor.a != 0)
                                { 
                                    _revealedPixels++; // 计数增加
                                    _texture.SetPixel(px, py, new UnityEngine.Color(0, 0, 0, 0)); // 设为透明
                                }
                            }
                        }
                    }
                }
            }

            _texture.Apply(); // 应用更改
        
    }

    private void ChangeBrush()
    {
        Init();
    }

    private void ShowFinish()
    {
        _comicItem.BrushFinish(_type,isBrush);
    }

    private Transform GetBrushTrans()
    {
        _brushTrans = isBrush ? PaperManager.Instance.Pen : PaperManager.Instance.Eraser;
        // **修正 pivot 偏移**
        _pivotOffset = Vector3.Scale(_brushTrans.GetComponentInChildren<SpriteRenderer>().sprite.bounds.extents,
            _brushTrans.transform.lossyScale);
        return _brushTrans;
    }
}
