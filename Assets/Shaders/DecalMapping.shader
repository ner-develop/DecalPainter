// Meshに対してデカールを貼るシェーダー
// Mesh空間の位置からUV座標を計算して、デカールテクスチャを描き込む。
Shader "DecalMapping"
{
	Properties
	{
		// 累積用テクスチャ
		_AccumulateTexture ("AccumulateTexture", 2D) = "black" {}
		// デカールテクスチャ
		_DecalTexture("Decal Texture", 2D) = "black" {}
		// デカールペイント情報たち
		_DecalRadius("Decal Radius", Float) = 0.5
		_DecalPositionOS("Decal Position (Object Space)", Vector) = (0, 0, 0, 0)
		_DecalNormal("Decal Normal", Vector) = (0, 1, 0, 0)
		_DecalTangent("Decal Tangent", Vector) = (1, 0, 0, 0)
		_Color("Color", Color) = (0, 0, 0, 0)
	}
	
	SubShader
	{
		Tags{"RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
		LOD 300
		
		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		
		TEXTURE2D(_AccumulateTexture);
		SAMPLER(sampler_AccumulateTexture);
		TEXTURE2D(_DecalTexture);
		SAMPLER(sampler_DecalTexture);
		
		CBUFFER_START(UnityPerMaterial)
		float4 _AccumulateTexture_ST;
		float4 _DecalTexture_ST;
		float _DecalSize;
		float3 _DecalPositionOS;
		float3 _DecalNormal;
		float3 _DecalTangent;
		float4 _Color;
		CBUFFER_END
		ENDHLSL

		Pass
		{
			Blend One Zero
			Cull Back

			HLSLPROGRAM
			#pragma target 2.0
			#pragma vertex ProcessVertex
			#pragma fragment ProcessFragment
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float3 positionOS : TEXCOORD1;
				half3 normalOS : TEXCOORD2;
			};

			Varyings ProcessVertex(Attributes input)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				Varyings output = (Varyings)0;

				// UV座標をそのままClip空間として表示
				const float x = input.texcoord.x * 2 - 1;
				const float y = input.texcoord.y * 2 - 1;
				output.positionCS = float4(x, -y, 0, 1);

				// 計算をObject空間で行うため、Object空間の法線と座標を渡す。
				// World空間でもいいが、halfで精度が足りなくなりやすいので理由がなければObject空間で計算する。
				output.texcoord = TRANSFORM_TEX(input.texcoord, _AccumulateTexture);
				output.normalOS = input.normal;
				output.positionOS = input.positionOS.xyz;
				return output;
			}

			half4 ProcessFragment(Varyings input) : SV_Target
			{
				const half3 normal = normalize(input.normalOS);
				half3 decalNormal = normalize(_DecalNormal);
				half3 decalTangent = normalize(_DecalTangent);
				half3 decalBitangent = normalize(cross(decalTangent, decalNormal));
				decalTangent = normalize(cross(decalNormal, decalBitangent));
				
				// 1. 平面上に描画座標をマップする。
				// 平面から描画座標のベクトルを求め、平面への正射影ベクトルを求める(=平面に投影した座標)
				const half3 decalCenterToPositionVector = input.positionOS - _DecalPositionOS;
				const float2 positionOnPlane = float2(dot(decalTangent, decalCenterToPositionVector), dot(decalBitangent, decalCenterToPositionVector));
				//return half4(x,y,0,1);
				
				
				// 2. 平面に投影した座標を、平面のUV座標に変換する
				const half2 unclampedUv = (positionOnPlane / _DecalSize) + 0.5; // 0~1空間に正規化するが、範囲外の場合もあるのでClampしない。0~1なら平面内。
				const half sameDirectionMask = step(0, dot(normal, decalNormal)); // Decalと同じ向きなら1, 逆なら0
				const half decalAreaMask = int(0 <= unclampedUv.x && unclampedUv.x <= 1 && 0 <= unclampedUv.y && unclampedUv.y <= 1); // 平面内なら1, 平面外なら0 
				const half2 uv = unclampedUv * sameDirectionMask * decalAreaMask;
				//return half4(uv.xy, 0, 1);
				
				
				// 4. UV座標のDecalTextureの色をフェッチするだけ
				half4 decalColor = SAMPLE_TEXTURE2D(_DecalTexture, sampler_DecalTexture, TRANSFORM_TEX(uv, _DecalTexture));
				decalColor.xyz *= _Color.xyz * decalColor.xyz * decalColor.w;
				//return stamp;
				
				
				// 5. 累積テクスチャに重ねて描画する
				const half4 acc = SAMPLE_TEXTURE2D(_AccumulateTexture, sampler_AccumulateTexture, input.texcoord);
				return half4(lerp(acc.xyz, decalColor.xyz, decalColor.w), acc.w);
			}
			ENDHLSL
		}
	}
}
