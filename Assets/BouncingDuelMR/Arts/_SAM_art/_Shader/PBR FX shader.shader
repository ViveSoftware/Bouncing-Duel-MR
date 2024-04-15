
Shader "PBR FX shader"
{
	Properties
	{
		_AlbedoMap("Albedo Map", 2D) = "white" {}
		[Toggle]_Usecustomcolor("Use custom color", Float) = 1
		_Color("Color", Color) = (0,0,0,0)
		_NormalMap("NormalMap", 2D) = "bump" {}
		_MetalicMap("Metalic Map", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.1
		_AOMap("AO Map", 2D) = "white" {}
		_AOamount("AO amount", Range( 0 , 1)) = 1
		[Toggle]_Useroughnessmap("Use roughness map", Float) = 1
		_RoughnessMap("Roughness Map", 2D) = "white" {}
		_DissolveMap("Dissolve Map", 2D) = "white" {}
		[Toggle]_Useemissionmap("Use emission map", Float) = 1
		_EmissionMap("Emission Map", 2D) = "white" {}
		_EmissionStrength("Emission Strength", Range( 0 , 1)) = 0
		_Dissolve("Dissolve", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Pass
		{
			ColorMask 0
			ZWrite On
		}

		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _Usecustomcolor;
		uniform sampler2D _AlbedoMap;
		uniform float4 _AlbedoMap_ST;
		uniform float4 _Color;
		uniform float _Useemissionmap;
		uniform sampler2D _EmissionMap;
		uniform float _EmissionStrength;
		uniform sampler2D _DissolveMap;
		uniform float4 _DissolveMap_ST;
		uniform float _Dissolve;
		uniform sampler2D _MetalicMap;
		uniform float4 _MetalicMap_ST;
		uniform float _Useroughnessmap;
		uniform float _Smoothness;
		uniform sampler2D _RoughnessMap;
		uniform float4 _RoughnessMap_ST;
		uniform sampler2D _AOMap;
		uniform float4 _AOMap_ST;
		uniform float _AOamount;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 normalizeResult84 = normalize( UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) ) );
			o.Normal = normalizeResult84;
			float2 uv_AlbedoMap = i.uv_texcoord * _AlbedoMap_ST.xy + _AlbedoMap_ST.zw;
			o.Albedo = lerp(tex2D( _AlbedoMap, uv_AlbedoMap ),_Color,_Usecustomcolor).rgb;
			float4 color98 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float normalizeResult138 = normalize( ase_worldNormal.x );
			float3 hsvTorgb103 = HSVToRGB( float3(( ( normalizeResult138 * 0.5 ) + ( _Time.y * -2.0 ) ),1.0,1.0) );
			float3 ase_worldPos = i.worldPos;
			float4 tex2DNode96 = tex2D( _EmissionMap, ase_worldPos.xy );
			float2 uv0_DissolveMap = i.uv_texcoord * _DissolveMap_ST.xy + _DissolveMap_ST.zw;
			float4 tex2DNode123 = tex2D( _DissolveMap, uv0_DissolveMap );
			float temp_output_119_0 = ( 1.0 - _Dissolve );
			float temp_output_122_0 = step( tex2DNode123.r , temp_output_119_0 );
			float4 color131 = IsGammaSpace() ? float4(1.414214,0.6869069,0.1534289,0) : float4(2.143548,0.4295688,0.02040295,0);
			o.Emission = ( lerp(color98,( ( ( float4( hsvTorgb103 , 0.0 ) + tex2DNode96 ) * tex2DNode96 ) * _EmissionStrength ),_Useemissionmap) + ( ( ( 1.0 - step( tex2DNode123.r , ( temp_output_119_0 + -0.03 ) ) ) * temp_output_122_0 ) * color131 ) ).rgb;
			float2 uv_MetalicMap = i.uv_texcoord * _MetalicMap_ST.xy + _MetalicMap_ST.zw;
			o.Metallic = tex2D( _MetalicMap, uv_MetalicMap ).r;
			float2 uv_RoughnessMap = i.uv_texcoord * _RoughnessMap_ST.xy + _RoughnessMap_ST.zw;
			o.Smoothness = lerp(_Smoothness,( ( 1.0 - tex2D( _RoughnessMap, uv_RoughnessMap ).r ) * _Smoothness ),_Useroughnessmap);
			float2 uv_AOMap = i.uv_texcoord * _AOMap_ST.xy + _AOMap_ST.zw;
			o.Occlusion = ( tex2D( _AOMap, uv_AOMap ) * _AOamount ).r;
			o.Alpha = temp_output_122_0;
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
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
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
