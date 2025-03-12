using System;
using Comic;
using Sirenix.OdinInspector;
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
    private Texture2D texture;          // 可修改的纹理
    public Texture2D originalTexture{ get; private set; }   // 存储原始纹理
    public float brushRadius = 10f;    // 画笔半径
    public float speed = 100f;         // 画笔移动速度
    private Vector2 _brushPosition;      // 画笔坐标
    private Vector2 _direction = new Vector2(1, 1); // 方向向量
    private Vector2 _offset;
    [OnValueChanged("ChangeBrush")]
    public bool isBrush = true;
    
    private int _revealedPixels = 0; // 记录已显示像素数
    private int _totalPixels;   
    private bool _isFinish = false;

    private ComicItem _comicItem;
    private BrushType _type;

    private void Awake()
    {
        _comicItem = GetComponentInParent<ComicItem>();
    }

    void Start()
    {
        targetSprite.sprite = Resources.Load<Sprite>(MaskSpritePath + _comicItem.ComicData.id);
        // 获取原始纹理，并创建一个可修改的副本
        originalTexture = Instantiate(targetSprite.sprite.texture); // 存储原始纹理
        texture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        _totalPixels = texture.width * texture.height;
        // **初始化 Texture 为全透明**
        Clear();
        // 生成新的 Sprite 并替换原来的
        targetSprite.sprite = Sprite.Create(texture, targetSprite.sprite.rect, new Vector2(0.5f, 0.5f));

        Init();
        
        _type = Enum.Parse<BrushType>(transform.parent.name);
        if (_type == BrushType.Color)
        {
            enabled = false;
        }
    }

    public void Clear()
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, new UnityEngine.Color(0, 0, 0, 0)); // 设为全透明
            }
        }
        texture.Apply();
    }

    void Init()
    {
        // 初始化画笔位置
        _brushPosition = new Vector2(0, texture.height-1);
        _offset = new Vector2(brushRadius, -brushRadius);
        _isFinish = false;
    }

    void Update()
    {
        // 移动画笔
        _brushPosition += _direction * speed * Time.fixedDeltaTime;
        if (_revealedPixels >= 0.97f*_totalPixels)
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
        if (_brushPosition.x <= 0 || _brushPosition.x >= texture.width
            || _brushPosition.y <= 0 || _brushPosition.y >= texture.height)
        {
            Bounce();
        }

        // 画出图像
        DrawTexture((int)_brushPosition.x, (int)_brushPosition.y, (int)brushRadius);
    }

    void Bounce()
    {
        _direction.x *= -1;
        _direction.y *= -1;
        _brushPosition += _offset;
        _brushPosition.x = Mathf.Clamp(_brushPosition.x, 0, texture.width-1);
        _brushPosition.y = Mathf.Clamp(_brushPosition.y, 0, texture.height-1);
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
                if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                {
                    float dist = Mathf.Sqrt(i * i + j * j);
                    if (dist < radius)
                    {
                        UnityEngine.Color originalColor = originalTexture.GetPixel(px, py); // 原始颜色
                        UnityEngine.Color currentColor = texture.GetPixel(px, py); // 当前颜色

                        // **逐步增加 Alpha，直到恢复原始 Alpha**
                        //currentColor = new UnityEngine.Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Min(originalColor.a, currentColor.a + 0.1f));
                        // 只有当当前像素是透明的，才增加计数
                        if (isBrush)
                        {
                            if (currentColor.a == 0)
                            {
                                _revealedPixels++; // 增加已显示像素数
                            }
                            texture.SetPixel(px, py, originalColor);
                        }
                        else
                        {
                            if (currentColor.a != 0)
                            {
                                _revealedPixels++; // 增加已显示像素数
                            }
                            texture.SetPixel(px, py, UnityEngine.Color.clear);
                        }

                    }
                }
            }
        }

        texture.Apply(); // 应用更改
    }

    private void ChangeBrush()
    {
        _revealedPixels = 0;
        Init();
    }

    private void ShowFinish()
    {
        _comicItem.BrushFinish(_type,isBrush);
    }
}
