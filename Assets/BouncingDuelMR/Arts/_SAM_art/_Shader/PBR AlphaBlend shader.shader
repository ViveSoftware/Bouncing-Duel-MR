Shader "PBR AlphaBlend shader"
{
	Properties
	{
		_AlbedoMap("Albedo Map", 2D) = "white" {}
		[Toggle]_Usefakereflaction("Use fake reflaction", Float) = 1
		_reflactmap("reflact map", 2D) = "white" {}
		_OpacityMap("Opacity Map", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_MetalicMap("Metalic Map", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.1
		_AOMap("AO Map", 2D) = "white" {}
		_AOamount("AO amount", Range( 0 , 1)) = 1
		[Toggle]_Useroughnessmap("Use roughness map", Float) = 1
		_RoughnessMap("Roughness Map", 2D) = "white" {}
		[Toggle]_Usecustomcolor("Use custom color", Float) = 0
		_Color("Color", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _Usefakereflaction;
		uniform float _Usecustomcolor;
		uniform sampler2D _AlbedoMap;
		uniform float4 _AlbedoMap_ST;
		uniform float4 _Color;
		uniform sampler2D _reflactmap;
		uniform sampler2D _MetalicMap;
		uniform float4 _MetalicMap_ST;
		uniform float _Useroughnessmap;
		uniform float _Smoothness;
		uniform sampler2D _RoughnessMap;
		uniform float4 _RoughnessMap_ST;
		uniform sampler2D _AOMap;
		uniform float4 _AOMap_ST;
		uniform float _AOamount;
		uniform sampler2D _OpacityMap;
		uniform float4 _OpacityMap_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 normalizeResult84 = normalize( UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) ) );
			o.Normal = normalizeResult84;
			float2 uv_AlbedoMap = i.uv_texcoord * _AlbedoMap_ST.xy + _AlbedoMap_ST.zw;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float2 temp_cast_0 = (( ( ase_screenPosNorm.x * 2.0 ) + ase_worldViewDir.y )).xx;
			o.Albedo = lerp(lerp(tex2D( _AlbedoMap, uv_AlbedoMap ),_Color,_Usecustomcolor),( ( tex2D( _reflactmap, temp_cast_0 ).r * 0.13 ) + lerp(tex2D( _AlbedoMap, uv_AlbedoMap ),_Color,_Usecustomcolor) ),_Usefakereflaction).rgb;
			float2 uv_MetalicMap = i.uv_texcoord * _MetalicMap_ST.xy + _MetalicMap_ST.zw;
			o.Metallic = tex2D( _MetalicMap, uv_MetalicMap ).r;
			float2 uv_RoughnessMap = i.uv_texcoord * _RoughnessMap_ST.xy + _RoughnessMap_ST.zw;
			o.Smoothness = lerp(_Smoothness,( ( 1.0 - tex2D( _RoughnessMap, uv_RoughnessMap ).r ) * _Smoothness ),_Useroughnessmap);
			float2 uv_AOMap = i.uv_texcoord * _AOMap_ST.xy + _AOMap_ST.zw;
			o.Occlusion = ( tex2D( _AOMap, uv_AOMap ) * _AOamount ).r;
			float2 uv_OpacityMap = i.uv_texcoord * _OpacityMap_ST.xy + _OpacityMap_ST.zw;
			o.Alpha = tex2D( _OpacityMap, uv_OpacityMap ).r;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float4 tSpace0 : TEXCOORD4;
				float4 tSpace1 : TEXCOORD5;
				float4 tSpace2 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}