Shader "Standard AlphaBlend"
{
	Properties
	{
		_alphamap("alpha map", 2D) = "white" {}
		_CustomColor("CustomColor", Color) = (0,0,0,0)
		_AlphaStrength("Alpha Strength", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _CustomColor;
		uniform sampler2D _alphamap;
		uniform float4 _alphamap_ST;
		uniform float _AlphaStrength;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _CustomColor.rgb;
			float temp_output_5_0 = 0.0;
			float3 temp_cast_1 = (temp_output_5_0).xxx;
			o.Emission = temp_cast_1;
			o.Metallic = temp_output_5_0;
			o.Smoothness = temp_output_5_0;
			float2 uv_alphamap = i.uv_texcoord * _alphamap_ST.xy + _alphamap_ST.zw;
			o.Alpha = ( tex2D( _alphamap, uv_alphamap ).a * _AlphaStrength );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}