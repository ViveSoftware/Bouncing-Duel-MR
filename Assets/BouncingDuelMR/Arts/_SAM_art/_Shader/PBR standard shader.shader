Shader "PBR standard shader"
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
		[Toggle]_Useemissionmap("Use emission map", Float) = 0
		_EmissionMap("Emission Map", 2D) = "white" {}
		_EmissionStrength("Emission Strength", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _Usecustomcolor;
		uniform sampler2D _AlbedoMap;
		uniform float4 _AlbedoMap_ST;
		uniform float4 _Color;
		uniform float _Useemissionmap;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform float _EmissionStrength;
		uniform sampler2D _MetalicMap;
		uniform float4 _MetalicMap_ST;
		uniform float _Useroughnessmap;
		uniform float _Smoothness;
		uniform sampler2D _RoughnessMap;
		uniform float4 _RoughnessMap_ST;
		uniform sampler2D _AOMap;
		uniform float4 _AOMap_ST;
		uniform float _AOamount;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 normalizeResult84 = normalize( UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) ) );
			o.Normal = normalizeResult84;
			float2 uv_AlbedoMap = i.uv_texcoord * _AlbedoMap_ST.xy + _AlbedoMap_ST.zw;
			o.Albedo = lerp(tex2D( _AlbedoMap, uv_AlbedoMap ),_Color,_Usecustomcolor).rgb;
			float4 color98 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float2 uv0_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			o.Emission = lerp(color98,( tex2D( _EmissionMap, uv0_EmissionMap ) * _EmissionStrength ),_Useemissionmap).rgb;
			float2 uv_MetalicMap = i.uv_texcoord * _MetalicMap_ST.xy + _MetalicMap_ST.zw;
			o.Metallic = tex2D( _MetalicMap, uv_MetalicMap ).r;
			float2 uv_RoughnessMap = i.uv_texcoord * _RoughnessMap_ST.xy + _RoughnessMap_ST.zw;
			o.Smoothness = lerp(_Smoothness,( ( 1.0 - tex2D( _RoughnessMap, uv_RoughnessMap ).r ) * _Smoothness ),_Useroughnessmap);
			float2 uv_AOMap = i.uv_texcoord * _AOMap_ST.xy + _AOMap_ST.zw;
			o.Occlusion = ( tex2D( _AOMap, uv_AOMap ) * _AOamount ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}