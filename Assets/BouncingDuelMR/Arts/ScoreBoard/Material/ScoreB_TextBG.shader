
Shader "ScoreB_Text"
{
	Properties
	{
		[HDR]_ColorA("ColorA", Color) = (0.01257424,0.6475733,1.20084,1)
		[HDR]_ColorB("ColorB", Color) = (0.01999998,0.3629999,1,1)
		_Opacity("Opacity", Range( 0 , 1)) = 0.5495694
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend One One
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
		};

		uniform float4 _ColorB;
		uniform float4 _ColorA;
		uniform float _Opacity;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV25 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode25 = ( 0.0 + 2.42 * pow( 1.0 - fresnelNdotV25, 4.13 ) );
			float2 uv_TexCoord44 = i.uv_texcoord * float2( 1,800 );
			float mulTime10 = _Time.y * -4.0;
			float4 lerpResult12 = lerp( _ColorA , _ColorB , sin( ( uv_TexCoord44.y + mulTime10 ) ));
			float4 clampResult23 = clamp( ( lerpResult12 * _Opacity ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Emission = ( ( fresnelNode25 * _ColorB ) + clampResult23 ).rgb;
			float temp_output_21_0 = _Opacity;
			o.Alpha = temp_output_21_0;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
