Shader "HTC/Shape/DotLine"
{
    Properties
    {
        _DotLength("Grid Size", float) = 0.05
        _StartPoint("Start Point", Vector) = (0, 0, 0, 0)
        _OffsetFactor("Offset Factor", Range(-1, 1)) = 0
        _OffsetUnit("Offset Unit", Range(-1, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Offset[_OffsetFactor],[_OffsetUnit]

        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            Cull Off Lighting Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR0;
                float4 worldPos : COLOR1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 _StartPoint;
            float _DotLength;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color.rgba;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = i.color;
                float offset = fmod(length(i.worldPos.xyz - _StartPoint), 2*_DotLength) / (2*_DotLength);
                col *= saturate((smoothstep(0.3, 0.5, offset) - smoothstep(0.8, 1, offset)));
                return col;
            }
            ENDCG
        }
    }
}
