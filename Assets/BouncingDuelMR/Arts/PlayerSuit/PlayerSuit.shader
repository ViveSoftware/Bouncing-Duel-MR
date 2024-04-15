
Shader "PlayerSuit"
{
	Properties
	{
		_Color("Color", Color) = (0.2039216,0.5137255,1,1)
		_Albedo("Albedo", 2D) = "white" {}
		[Normal]_Normal("Normal", 2D) = "bump" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.65
		_Specular("Specular", 2D) = "white" {}
		_AO("AO", 2D) = "white" {}
		_CM("CM", 2D) = "white" {}
		[HDR]_Rim_Color("Rim_Color", Color) = (0.5801887,0.8059666,1,0)
		_Rim_Power("Rim_Power", Range( 0 , 1)) = 0.38
		_Opacity("Opacity", Range( 0 , 1)) = 0.5405228
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

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf StandardSpecular keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _Normal;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
		uniform sampler2D _CM;
		uniform float4 _Rim_Color;
		uniform float _Rim_Power;
		uniform sampler2D _Specular;
		uniform float _Smoothness;
		uniform sampler2D _AO;
		uniform float _Opacity;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			o.Normal = UnpackNormal( tex2D( _Normal, i.uv_texcoord ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode12 = tex2D( _Albedo, uv_Albedo );
			float4 tex2DNode6 = tex2D( _CM, i.uv_texcoord );
			float4 lerpResult41 = lerp( tex2DNode12 , ( ( _Color * tex2DNode6 ) * tex2DNode12 ) , tex2DNode6);
			float4 clampResult32 = clamp( lerpResult41 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Albedo = clampResult32.rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV14 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode14 = ( 0.0 + _Rim_Power * pow( 1.0 - fresnelNdotV14, 1.4 ) );
			float4 clampResult25 = clamp( ( _Rim_Color * fresnelNode14 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Emission = clampResult25.rgb;
			o.Specular = tex2D( _Specular, i.uv_texcoord ).rgb;
			o.Smoothness = _Smoothness;
			o.Occlusion = tex2D( _AO, i.uv_texcoord ).r;
			o.Alpha = _Opacity;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
