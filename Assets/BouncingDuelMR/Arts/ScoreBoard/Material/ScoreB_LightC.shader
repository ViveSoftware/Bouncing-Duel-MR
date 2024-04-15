
Shader "ScoreB_LightC"
{
	Properties
	{
		_ColorA("ColorA", Color) = (0.6937375,0.2313725,1,0)
		_ColorB("ColorB", Color) = (0.2688679,0.8419206,1,0)
		_Speed("Speed", Float) = 10
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _ColorA;
		uniform float4 _ColorB;
		uniform float _Speed;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float mulTime10 = _Time.y * _Speed;
			float4 lerpResult12 = lerp( _ColorA , _ColorB , sin( ( ( 1.0 * ase_vertex3Pos.x ) + mulTime10 ) ));
			o.Emission = lerpResult12.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
