Shader "UvMappingTest"
{
	SubShader
	{
		Tags{ "Queue" = "Overlay" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
		LOD 300

		Pass
		{
			Blend One Zero
			Cull Off
			ZWrite Off
			ZTest Always

			HLSLPROGRAM
			#pragma target 2.0
			#pragma vertex ProcessVertex
			#pragma fragment ProcessFragment
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			Varyings ProcessVertex(Attributes input)
			{
				Varyings output = (Varyings)0;

				// UV座標をそのままClip空間として表示
				// プラットフォームごとによる上下の違いについて：https://docs.unity3d.com/2019.1/Documentation/Manual/SL-PlatformDifferences.html
				const float x = input.texcoord.x * 2 - 1;
				const float y = input.texcoord.y * 2 - 1;
				const float shouldFlipY = _ProjectionParams.x < 0 ? -1 : 1;
				output.positionCS = float4(x, shouldFlipY * y, 1, 1);

				output.texcoord = input.texcoord;
				return output;
			}

			half4 ProcessFragment(Varyings input) : SV_Target
			{
				return half4(input.texcoord.xy, 0, 1);
			}
			ENDHLSL
		}
	}
}
