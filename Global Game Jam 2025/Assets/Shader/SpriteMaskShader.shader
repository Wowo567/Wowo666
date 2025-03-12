Shader "Custom/SpriteMaskShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}  // 主要 Sprite 贴图
        _MaskTex ("Mask Texture", 2D) = "white" {}  // Mask 贴图
        _MaskPos ("Mask Position", Vector) = (0,0,0,0) // Mask 位置
        _MaskSize ("Mask Size", Vector) = (1,1,1,1)   // Mask 大小（世界单位）
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

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
            float4 _MaskPos;  // Mask 世界坐标
            float4 _MaskSize; // Mask 大小（世界单位）

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // 获取 Sprite 世界坐标
                float2 worldPos = mul(unity_ObjectToWorld, v.vertex).xy;

                // 计算 Mask UV，修正缩放问题
                o.maskUV = (worldPos - _MaskPos.xy) / _MaskSize.xy + 0.5;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);  // 主要 Sprite 颜色
                fixed4 mask = tex2D(_MaskTex, i.maskUV);  // 采样 Mask 贴图

                // 只让 Mask 贴图的白色区域可见
                color.a *= mask.r;

                return color;
            }
            ENDCG
        }
    }
}
å