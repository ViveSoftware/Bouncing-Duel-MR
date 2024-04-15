Shader "HTC/Shape/PatternGround"
{
    Properties
    {
        _PatternTex("Pattern", 2D) = "white" {}
        _PattenrColor("Pattern Color", Color) = (1, 1, 1, 1)
        _PatternWidth("Width (in unity unit)", float) = 1
        _PatternHeight("Height (in unity unit)", float) = 1
        _MaskRadius("Mask Radius", Range(0,1)) = 1

        _SpotLightColor("Spot Light Color", Color) = (1, 1, 1, 1)
        _SpotLightRange("Spot Light Range (in unity unit)", float) = 0.2

        _PatternOffsetX("Pattern Offset X", float) = 0
        _PatternOffsetZ("Pattern Offset Z", float) = 0

        _PosOffsetX("Position Offset X", float) = 0
        _PosOffsetZ("Position Offset Z", float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 worldPos : COLOR0;
                float4 localPos : COLOR1;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _PatternTex;
            float4 _PattenrColor;
            float _PatternWidth;
            float _PatternHeight;
            float _MaskRadius;

            float4 _SpotLightColor;
            float _SpotLightRange;

            float _PatternOffsetX;
            float _PatternOffsetZ;

            float _PosOffsetX;
            float _PosOffsetZ;


            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.localPos = v.vertex;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float fullPatternWidth = _PatternWidth * 2;
                float x = i.worldPos.x - _PatternOffsetX - _PosOffsetX;
                float z = i.worldPos.z - _PatternOffsetZ - _PosOffsetZ;

                float2 patternUV = float2(
                    (x - floor(x / fullPatternWidth) * fullPatternWidth) / fullPatternWidth,
                    (z - floor(z / _PatternHeight) * _PatternHeight) / _PatternHeight );

                patternUV.x = (clamp(patternUV.x, 0, 0.5) - clamp(patternUV.x, 0.5, 1)) * 2;

                fixed4 patternTexColor = tex2D(_PatternTex, patternUV);
                float radius = _MaskRadius / 2;
                fixed4 patternColor = patternTexColor * _PattenrColor * (1 - length(i.uv - float2(0.5, 0.5))/ radius);
                patternColor = patternColor * patternTexColor.a;

                fixed4 spotLightColor = saturate((_SpotLightRange - length(i.worldPos - unity_ObjectToWorld._m03_m13_m23)) / _SpotLightRange) * _SpotLightColor;

                return patternColor * _PattenrColor.a + spotLightColor;
            }
            ENDCG
        }
    }
}
