﻿Shader "Hidden/RendeerTextureDebug/DebugColorSpace"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // from 2019.1...
            // #pragma multi_compile_local _ LINEAR_TO_GAMMMA GAMMA_TO_LINEAR
            #pragma multi_compile _ LINEAR_TO_GAMMMA GAMMA_TO_LINEAR
            #pragma multi_compile _ FLIP_Y
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                #if FLIP_Y
                o.uv.y = 1.0 - o.uv.y;
                #endif
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);

                #if LINEAR_TO_GAMMMA
                col.rgb = LinearToGammaSpace(col.rgb);
                #elif GAMMA_TO_LINEAR
                col.rgb = GammaToLinearSpace(col.rgb);
                #endif
                return col;
            }
            ENDCG
        }
    }
}
