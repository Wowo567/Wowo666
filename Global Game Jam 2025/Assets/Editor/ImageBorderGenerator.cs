using UnityEngine;
using UnityEditor;
using System.IO;
using DG.DemiEditor;

public class ImageBorderGenerator : EditorWindow
{
    private string sourceFolder = "Assets/Arts/xihua1heibai";
    private string outputFolder = "Assets/Resources/Masks";
    private int borderSize = 200;

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
                StaticAddBorderToImage(file, outputFolder);
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

        int newWidth = Xmax - Xmin;
        int newHeight = Ymax - Ymin;

        //自动 or 手动选择 borderSize
        int borderSize = useCustomBorder ? customBorderSize : (int)(0.15f * Mathf.Max(newWidth, newHeight));

        width += 2 * borderSize;
        height += 2 * borderSize;

        Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color white = Color.white;
        Color black = Color.black;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBorder = (x < borderSize + Xmin || x >= borderSize + Xmax || y < borderSize + Ymin || y >= borderSize + Ymax);
                newTexture.SetPixel(x, y, isBorder ? black : white);
            }
        }

        newTexture.Apply();

        string fileName = Path.GetFileNameWithoutExtension(filePath).Replace("BG", "");
        string outputPath = Path.Combine(outputFolder, "Mask" + fileName + ".png");
        File.WriteAllBytes(outputPath, newTexture.EncodeToPNG());

        AssetDatabase.ImportAsset(outputPath);
        TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        if (importer != null)
        {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
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

        int centerX = width / 2;
        int centerY = height / 2;

        for (int x = 0; x < width; x++)
        {
            Color pixel = texture.GetPixel(x, centerY);
            if (pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f)
            {
                if (x < Xmin) Xmin = x;
                if (x > Xmax) Xmax = x;
            }
        }

        for (int y = 0; y < height; y++)
        {
            Color pixel = texture.GetPixel(centerX, y);
            if (pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f)
            {
                if (y < Ymin) Ymin = y;
                if (y > Ymax) Ymax = y;
            }
        }

        Xmin = Mathf.Clamp(Xmin, 0, width - 1);
        Xmax = Mathf.Clamp(Xmax, 0, width - 1);
        Ymin = Mathf.Clamp(Ymin, 0, height - 1);
        Ymax = Mathf.Clamp(Ymax, 0, height - 1);
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
