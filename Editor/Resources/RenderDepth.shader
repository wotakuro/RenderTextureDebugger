Shader "Hidden/RendeerTextureDebug/RenderDepth"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MinParam("Min", Float) = 0.0
		_MaxParam("Max", Float) = 1.0
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

			float _MinParam;
			float _MaxParam;
			
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
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float raw_depth = SAMPLE_DEPTH_TEXTURE(_MainTex, i.uv);
			    float linearized_depth = Linear01Depth(raw_depth);

				linearized_depth = (linearized_depth - _MinParam) * ( 1 / (_MaxParam - _MinParam) );

			    return float4(linearized_depth, linearized_depth, linearized_depth, 1);
			}
			ENDCG
		}
	}
}
