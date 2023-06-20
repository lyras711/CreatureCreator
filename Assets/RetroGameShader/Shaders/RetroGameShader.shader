Shader "RetroGameShader/RetroGameShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CRTTex("Texture", 2D) = "white" {}
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
			sampler2D _CRTTex;
			uniform float4 _CRTParam;			// CRTTexelSizeX, CRTTexelSizeY, CRTColorGain, CRTCurve
			uniform float4 _PixelParam;			// size, gamma, colorbit, dither
			uniform float4 _GrayScaleParam;		// R-Factor, G-Factor, B-Factor, ratio

			fixed4 frag (v2f i) : SV_Target
			{
				float2 halfPixel = float2(0.5, 0.5);

				// Curve
				float2 screenUv = i.uv;
				float2 scaledUv = (screenUv - halfPixel) * _CRTParam.w + halfPixel;
				float2 curveRatio = abs(screenUv.yx - halfPixel) * 2.0;
				curveRatio = curveRatio * curveRatio * curveRatio;
				float2 curvedUv = lerp(screenUv, scaledUv, curveRatio);

				// pixelize
				float2 screenPos = floor(curvedUv * _ScreenParams.xy + float2(0.1, 0.1));
				float2 scaledPos = floor(screenPos / _PixelParam.x);
				float2 pixelizePos = scaledPos * _PixelParam.x;
				float2 pixelizeUv = (pixelizePos + halfPixel) / _ScreenParams.xy;

				float4 col = tex2D(_MainTex, pixelizeUv);

				// postarization & grayscale
				float3 colGamma = pow(col.rgb, _PixelParam.y);
				float grayColor = dot(colGamma, _GrayScaleParam.rgb);
				colGamma = lerp(colGamma, float3(grayColor, grayColor, grayColor), _GrayScaleParam.a);
				float3 colLow = floor(colGamma * _PixelParam.z) / _PixelParam.z;

				// dither
				float isOdd = frac(scaledPos.x * 0.5 + scaledPos.y * 0.5) * 2.0;
				float3 colLow2 = floor(colGamma * _PixelParam.z * 2.0) / (_PixelParam.z * 2.0);
				float3 colDiff = colLow2 - colLow;
				float3 colLowAdd = colDiff * 2.0 * isOdd * _PixelParam.w;
				colLow += colLowAdd;

				float3 colLowInvGamma = pow(colLow, 1.0 / _PixelParam.y);

				// scan line
				float4 colCRT = tex2D(_CRTTex, (screenPos + halfPixel) / _CRTParam.xy);
				colLowInvGamma *= colCRT;
				colLowInvGamma *= _CRTParam.z;

				// darken edge
				float darken = (1.0 - step(0.0, curvedUv.x)) + (1.0 - step(0.0, curvedUv.y)) + step(1.0, curvedUv.x) + step(1.0, curvedUv.y);
				colLowInvGamma *= 1.0 - darken;

				return float4(colLowInvGamma, col.a);
			}
			ENDCG
		}
	}
}
