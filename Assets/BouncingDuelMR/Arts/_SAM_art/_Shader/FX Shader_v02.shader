Shader "FX Shader_v02"
{
	Properties
	{
		_texture("texture", 2D) = "white" {}
		[HDR]_MainColor("Main Color", Color) = (0.1933962,0.6066974,1,0)
		_UVmaskVdown("UVmask V down", Float) = 2
		_UVmaskVup("UVmask V up", Float) = 1.1
		_offsetdir("offset dir", Vector) = (0,0,0,0)
		_MaskStrength("Mask Strength", Range( 0 , 50)) = 2.352941
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _tex4coord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend One One
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float4 uv2_tex4coord2;
		};

		uniform float4 _MainColor;
		uniform sampler2D _texture;
		uniform float2 _offsetdir;
		uniform float4 _texture_ST;
		uniform float _UVmaskVup;
		uniform float _UVmaskVdown;
		uniform float _MaskStrength;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv0_texture = i.uv_texcoord * _texture_ST.xy + _texture_ST.zw;
			float2 panner5 = ( 1.0 * _Time.y * _offsetdir + uv0_texture);
			float clampResult25 = clamp( ( ( ( 1.0 - ( i.uv_texcoord.y * _UVmaskVup ) ) * ( i.uv_texcoord.y * _UVmaskVdown ) ) * _MaskStrength ) , 0.0 , 1.0 );
			float4 temp_output_15_0 = ( tex2D( _texture, panner5 ) * clampResult25 );
			float4 appendResult34 = (float4(i.uv2_tex4coord2.x , i.uv2_tex4coord2.y , 0.0 , 0.0));
			o.Emission = ( ( _MainColor * ( ( temp_output_15_0 * temp_output_15_0 ) * 1.5 ) ) * (appendResult34).x ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
