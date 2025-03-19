Shader "Custom/SpriteMaskShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}  
        _MaskTex ("Mask Texture", 2D) = "white" {}  
        _MaskPos ("Mask Position", Vector) = (0,0,0,0) 
        _MaskSize ("Mask Size", Vector) = (1,1,1,1)   
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Pass
        {
            Cull Off
            ZWrite Off
            Blend One OneMinusSrcAlpha // 预乘 Alpha 混合

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 maskUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MaskPos;  
            float4 _MaskSize; 

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // 计算世界坐标
                float2 worldPos = mul(unity_ObjectToWorld, v.vertex).xy;

                // 计算 Mask UV
                o.maskUV = (worldPos - _MaskPos.xy) / _MaskSize.xy + 0.5;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);  
                fixed4 mask = tex2D(_MaskTex, i.maskUV);  

                // **透明度正确计算**
                float maskAlpha = mask.r; //只取 Mask 的红色通道作为 Alpha
                color.rgb *= color.a; //预乘 Alpha，确保混合正确
                color.a *= maskAlpha; //透明度受到 Mask 影响

                // **确保透明区域不被错误渲染**
                clip(color.a - 0.01);

                return color;
            }
            ENDCG
        }
    }
}
