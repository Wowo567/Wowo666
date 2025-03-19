using UnityEngine;
using UnityEditor;
using System.IO;
using DG.DemiEditor;

public class ImageBorderGenerator : EditorWindow
{
    private string sourceFolder = "Assets/Arts/xihua1heibai";  // 需要处理的图片文件夹
    private string outputFolder = "Assets/Resources/Masks";  // 生成的图片存放位置
    //private int borderSize = 100;  // 边框大小

    [MenuItem("Tools/Generate Image Borders")]
    public static void ShowWindow()
    {
        GetWindow<ImageBorderGenerator>("Image Border Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Image Border Generator", EditorStyles.boldLabel);

        // 输入源文件夹路径
        sourceFolder = EditorGUILayout.TextField("Source Folder:", sourceFolder);
        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);

        // 选择边框大小
        //borderSize = EditorGUILayout.IntField("Border Size:", borderSize);

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

        // 确保输出文件夹存在
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string[] files = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (file.EndsWith(".png") || file.EndsWith(".jpg"))
            {
                if(file.FileOrDirectoryName().Contains("BG"))
                {
                    Debug.Log("AddBorderToImage: " + file);
                    AddBorderToImage(file);
                }
            }
        }

        Debug.Log("Image generation complete!");
        AssetDatabase.Refresh(); // 刷新 Unity 资源数据库
    }

    private void AddBorderToImage(string filePath)
    {
        byte[] imageData = File.ReadAllBytes(filePath);
        Texture2D originalTexture = new Texture2D(2, 2);
        originalTexture.LoadImage(imageData);

        int width = originalTexture.width;
        int height = originalTexture.height;
        
        // 计算裁剪范围
        int Xmin, Xmax, Ymin, Ymax;
        FindBoundingBox(originalTexture, out Xmin, out Xmax, out Ymin, out Ymax);

        if (Xmin >= Xmax || Ymin >= Ymax)
        {
            Debug.LogWarning($"Skipping {filePath}: No valid white region found.");
            return;
        }

        int newWidth = Xmax - Xmin;
        int newHeight = Ymax - Ymin;

        int borderSize = (int)(0.15f * Mathf.Max(newWidth, newHeight));

        width += 2 * borderSize;
        height += 2 * borderSize;
       // 创建新的纹理，纯白色填充
        Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // 填充白色背景
        Color white = Color.white;
        Color black = Color.black;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 判断是否处于边框区域
                bool isBorder = (x < borderSize + Xmin || x >=borderSize + Xmax || y < borderSize + Ymin ||
                                 y >= borderSize + Ymax);
                newTexture.SetPixel(x, y, isBorder ? black : white);
            }
        }

        newTexture.Apply();

        // 保存图片
        byte[] pngData = newTexture.EncodeToPNG();
        string fileName = Path.GetFileNameWithoutExtension(filePath).Replace("BG", "");
        Debug.Log(fileName+"  fileName");
        string outputPath = Path.Combine(outputFolder, "Mask" + fileName + ".png");
        File.WriteAllBytes(outputPath, pngData);

        // 5️⃣ 让新生成的纹理 Read/Write 可用
        AssetDatabase.ImportAsset(outputPath);
        TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        if (importer != null)
        {
            importer.isReadable = true;  // ✅ 让 Read/Write 选中
            importer.textureCompression = TextureImporterCompression.Uncompressed; // 避免压缩影响
            importer.SaveAndReimport();
        }
        
        Debug.Log("Generated: " + outputPath);

        // 清理资源
        Object.DestroyImmediate(originalTexture);
        Object.DestroyImmediate(newTexture);
    }
    
    private void FindBoundingBox(Texture2D texture, out int Xmin, out int Xmax, out int Ymin, out int Ymax)
    {
        int width = texture.width;
        int height = texture.height;

        // 初始化边界值
        Xmin = width;
        Xmax = 0;
        Ymin = height;
        Ymax = 0;

        // 获取图像的中心点
        int centerX = width / 2;
        int centerY = height / 2;

        // **水平扫描（从中心行）**
        for (int x = 0; x < width; x++)
        {
            Color pixel = texture.GetPixel(x, centerY);
            if (pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f) // 近似白色
            {
                if (x < Xmin) Xmin = x;
                if (x > Xmax) Xmax = x;
            }
        }

        // **竖直扫描（从中心列）**
        for (int y = 0; y < height; y++)
        {
            Color pixel = texture.GetPixel(centerX, y);
            if (pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f) // 近似白色
            {
                if (y < Ymin) Ymin = y;
                if (y > Ymax) Ymax = y;
            }
        }

        // 防止越界
        Xmin = Mathf.Clamp(Xmin, 0, width - 1);
        Xmax = Mathf.Clamp(Xmax, 0, width - 1);
        Ymin = Mathf.Clamp(Ymin, 0, height - 1);
        Ymax = Mathf.Clamp(Ymax, 0, height - 1);
    }

}
