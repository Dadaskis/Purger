Shader "Hidden/MuzzleFlash"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RenderTexture ("Camera texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _RenderTexture;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 cameraColor = tex2D(_RenderTexture, i.uv);
				fixed4 mainColor = tex2D(_MainTex, i.uv);
				//fixed4 cameraColorGrey = dot(cameraColor.rgb, float3(0.3, 0.59, 0.11));
				float cameraColorGrey = (cameraColor.r + cameraColor.g + cameraColor.b) / 3.0;
				return lerp(mainColor, cameraColor, cameraColorGrey);
			}
			ENDCG
		}
	}
}
