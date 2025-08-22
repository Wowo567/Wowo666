using UnityEngine;
using UnityEditor;
using System.IO;
using DG.DemiEditor;

public class ImageBorderGenerator : EditorWindow
{
    private string sourceFolder = "Assets/Arts/xihua1heibai";
    private string outputFolder = "Assets/Resources/Masks";
    private int borderSize = 300;

    [MenuItem("Tools/Generate Image Borders")]
    public static void ShowWindow()
    {
        GetWindow<ImageBorderGenerator>("Image Border Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Image Border Generator", EditorStyles.boldLabel);
        sourceFolder = EditorGUILayout.TextField("Source Folder:", sourceFolder);
        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);
        borderSize = EditorGUILayout.IntField("Border Size:", borderSize);

        if (GUILayout.Button("Generate Borders"))
        {
            GenerateImagesWithBorder();
        }
    }

    private void GenerateImagesWithBorder()
    {
        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogError("Source folder not found: " + sourceFolder);
            return;
        }

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string[] files = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if ((file.EndsWith(".png") || file.EndsWith(".jpg")) && file.FileOrDirectoryName().Contains("BG"))
            {
                Debug.Log("AddBorderToImage: " + file);
                StaticAddBorderToImage(file, outputFolder, borderSize, useCustomBorder: true);
            }
        }

        Debug.Log("Image generation complete!");
        AssetDatabase.Refresh();
    }

    public static void StaticAddBorderToImage(string filePath, string outputFolder, int customBorderSize = 0, bool useCustomBorder = false)
{
    byte[] imageData = File.ReadAllBytes(filePath);
    Texture2D originalTexture = new Texture2D(2, 2);
    originalTexture.LoadImage(imageData);

    int width = originalTexture.width;
    int height = originalTexture.height;

    int Xmin, Xmax, Ymin, Ymax;
    new ImageBorderGenerator().FindBoundingBox(originalTexture, out Xmin, out Xmax, out Ymin, out Ymax);

    if (Xmin >= Xmax || Ymin >= Ymax)
    {
        Debug.LogWarning($"Skipping {filePath}: No valid white region found.");
        return;
    }
    
    // 计算内容区域
    int newWidth = Xmax - Xmin;
    int newHeight = Ymax - Ymin;

    // 自动 or 手动选择 borderSize
    int borderSize = useCustomBorder ? customBorderSize : (int)(0.18f * Mathf.Max(newWidth, newHeight));

    // 保证宽高为 4 的倍数
    int totalWidth = Mathf.CeilToInt((newWidth + 2 * borderSize) / 4f) * 4;
    int totalHeight = Mathf.CeilToInt((newHeight + 2 * borderSize) / 4f) * 4;

    // 创建新贴图（注意用 RGB24，因为 JPG 没有透明度）
    Texture2D newTexture = new Texture2D(totalWidth, totalHeight, TextureFormat.RGB24, false);

   
    Color white = Color.white;
    Color black = Color.black;

    // 先将整个纹理填充为黑色（边框颜色）
    for (int y = 0; y < totalHeight; y++)
    {
        for (int x = 0; x < totalWidth; x++)
        {
            newTexture.SetPixel(x, y, black);
        }
    }

    // 计算居中位置
    int offsetX = borderSize;
    int offsetY = borderSize;
    
    // 然后在内部区域填充白色，只要是非透明像素就填充为白色
    for (int y = Ymin; y < Ymax; y++)
    {
        for (int x = Xmin; x < Xmax; x++)
        {
            // 获取原始纹理中的颜色
            Color originalColor = originalTexture.GetPixel(x, y);
            // 只要原始图像中非透明区域就设置为白色
            if (originalColor.a > 0.1f)
            {
                int newX = x - Xmin + offsetX;
                int newY = y - Ymin + offsetY;
                newTexture.SetPixel(newX, newY, white);
            }
        }
    }

    newTexture.Apply();
    ExpandMask(newTexture, -4);

    string fileName = Path.GetFileNameWithoutExtension(filePath).Replace("BG", "");
    string outputPath = Path.Combine(outputFolder, "Mask" + fileName + ".jpg");
    
    // 如果文件已存在，跳过生成
    if (File.Exists(outputPath))
    {
        Debug.Log("Skipped (already exists): " + outputPath);
        return;
    }
    
    // 指定压缩质量，比如 75
    File.WriteAllBytes(outputPath,  newTexture.EncodeToJPG(75));

    AssetDatabase.ImportAsset(outputPath);
    TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
    if (importer != null)
    {
        importer.isReadable = true;
        importer.textureCompression = TextureImporterCompression.Compressed;
        
        // 如果图片超过2048，就把 maxSize 提高到 4096
        importer.maxTextureSize = newTexture.width > 2048 || newTexture.height > 2048 ? 4096 : 2048;
        
        // 确保与原始图像使用相同的导入设置
        TextureImporter originalImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;
        if (originalImporter != null)
        {
            importer.spritePixelsPerUnit = originalImporter.spritePixelsPerUnit;
            importer.filterMode = originalImporter.filterMode;
            //importer.spriteAlignment = originalImporter.spriteAlignment; // 使用相同的对齐方式
            importer.spritePivot = originalImporter.spritePivot; // 使用相同的 Pivot
        }
        
        importer.SaveAndReimport();
    }

    Debug.Log("Generated: " + outputPath);

    Object.DestroyImmediate(originalTexture);
    Object.DestroyImmediate(newTexture);
}

    public void FindBoundingBox(Texture2D texture, out int Xmin, out int Xmax, out int Ymin, out int Ymax)
    {
        int width = texture.width;
        int height = texture.height;

        Xmin = width;
        Xmax = 0;
        Ymin = height;
        Ymax = 0;

        // 扫描整个图像，检测任何非透明像素
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = texture.GetPixel(x, y);
                // 检测非透明像素（alpha值大于一定阈值，例如0.1）
                if (pixel.a > 0.1f)
                {
                    if (x < Xmin) Xmin = x;
                    if (x > Xmax) Xmax = x;
                    if (y < Ymin) Ymin = y;
                    if (y > Ymax) Ymax = y;
                }
            }
        }

        // 防止边界超出范围
        Xmin = Mathf.Clamp(Xmin, 0, width - 1);
        Xmax = Mathf.Clamp(Xmax + 1, 0, width); // +1 是为了包含最大边界像素
        Ymin = Mathf.Clamp(Ymin, 0, height - 1);
        Ymax = Mathf.Clamp(Ymax + 1, 0, height); // +1 是为了包含最大边界像素

        // 如果没有找到有效区域，使用整个图像
        if (Xmin >= Xmax || Ymin >= Ymax)
        {
            Xmin = 0;
            Xmax = width;
            Ymin = 0;
            Ymax = height;
        }
    }
    
    public static void ExpandMask(Texture2D tex, int size)
{
    if (size == 0) return;

    int width = tex.width;
    int height = tex.height;
    Color32[] pixels = tex.GetPixels32();
    Color32[] newPixels = new Color32[pixels.Length];

    // 复制原图，初始全部黑色
    for (int i = 0; i < newPixels.Length; i++) newPixels[i] = Color.black;

    if (size > 0)
    {
        // 膨胀：白色点向外扩展 size 像素
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (pixels[index].r > 200) // 白色点
                {
                    for (int dy = -size; dy <= size; dy++)
                    {
                        for (int dx = -size; dx <= size; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                newPixels[ny * width + nx] = Color.white;
                            }
                        }
                    }
                }
            }
        }
    }
    else
    {
        // 收缩：白色点只保留周围 size 范围内都为白色的像素
        int absSize = -size;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (pixels[index].r > 200) // 白色点
                {
                    bool keep = true;
                    // 检查周围 absSize 范围内是否有黑色点
                    for (int dy = -absSize; dy <= absSize && keep; dy++)
                    {
                        for (int dx = -absSize; dx <= absSize && keep; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                if (pixels[ny * width + nx].r < 200)
                                {
                                    keep = false;
                                }
                            }
                            else
                            {
                                keep = false; // 边界也视为黑色
                            }
                        }
                    }
                    if (keep) newPixels[index] = Color.white;
                }
            }
        }
    }

    tex.SetPixels32(newPixels);
    tex.Apply();
}


}

public class ImageBorderContextMenu
{
    private const int defaultBorderSize = 500;
    private const string outputFolder = "Assets/Resources/Masks";

    [MenuItem("Assets/Generate Image Border", false, 1000)]
    public static void GenerateBorderForSelectedImage()
    {
        Object selected = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(selected);

        if (string.IsNullOrEmpty(path) || (!path.EndsWith(".png") && !path.EndsWith(".jpg")))
        {
            Debug.LogError("Please select a valid PNG or JPG image.");
            return;
        }

        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), path);

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //右键使用固定 borderSize
        ImageBorderGenerator.StaticAddBorderToImage(absolutePath, outputFolder, defaultBorderSize, useCustomBorder: true);
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Generate Image Border", true)]
    public static bool ValidateGenerateBorderForSelectedImage()
    {
        Object selected = Selection.activeObject;
        if (selected == null) return false;

        string path = AssetDatabase.GetAssetPath(selected);
        return path.EndsWith(".png") || path.EndsWith(".jpg");
    }
}