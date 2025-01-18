using UnityEngine;
using UnityEngine.UI;

public class OverlayImage : MonoBehaviour
{
    public Image overlayImage;  // 在Inspector中设置

    void Start()
    {
        // 确保 Image 是全屏的
        RectTransform rt = overlayImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(Screen.width, Screen.height);

        // 使用自定义的混合模式材质
        Material overlayMaterial = new Material(Shader.Find("Custom/OverlayBlend"));  // 假设你已经有一个自定义的叠加Shader
        overlayImage.material = overlayMaterial;
    }
}