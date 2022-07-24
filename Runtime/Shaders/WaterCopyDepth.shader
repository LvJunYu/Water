Shader "Hidden/Hidden/Water/CopyDepth"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			ZWrite Off
			ZTest Always
			Cull Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			//#if UNITY_UV_STARTS_AT_TOP
			//	o.uv.y = 1 - o.uv.y;
			//#endif
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float depth = SAMPLE_DEPTH_TEXTURE(_MainTex, i.uv);
				return float4(depth, 0, 0, 1);
			}
			ENDHLSL
		}
	}

	FallBack Off
}