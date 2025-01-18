Shader "Custom/OverlayBlend"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _OverlayTex ("Overlay (RGB)", 2D) = "white" { }
        _Blend ("Blend", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }

        // 直接在 SubShader 中定义渲染行为
        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_OverlayTex;
        };

        sampler2D _MainTex;
        sampler2D _OverlayTex;
        float _Blend;

        void surf(Input IN, inout SurfaceOutput o)
        {
            // 取主纹理和覆盖纹理的颜色值
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            half4 overlay = tex2D(_OverlayTex, IN.uv_OverlayTex);

            // 混合两种颜色，这里使用线性插值，基于 _Blend 的值
            o.Albedo = lerp(c.rgb, overlay.rgb, _Blend);
            o.Alpha = c.a;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
