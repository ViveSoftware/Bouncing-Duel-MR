Shader "HTC/Shape/RadialGradient"
{
    Properties
    {
        _ColorKey1("Color1", Color) = (1, 1, 1 ,1)
        _ColorKey2("Color2", Color) = (1, 1, 1 ,1)
        _ColorKey3("Color3", Color) = (1, 1, 1 ,1)
        _ColorKey4("Color4", Color) = (1, 1, 1 ,1)
        _ColorKey5("Color5", Color) = (1, 1, 1 ,1)
        _Dist1("Dist1", Range(0, 1)) = 0
        _Dist2("Dist2", Range(0, 1)) = 0.1
        _Dist3("Dist3", Range(0, 1)) = 0.2
        _Dist4("Dist4", Range(0, 1)) = 1
        _Dist5("Dist5", Range(0, 1)) = 1
        [HideInInspector]_Center("Center", Vector) = (0, 0, 0, 0)
        _Alpha("Alpha", Range(0, 1)) = 1
        _UseGradient("Use Gradient", Range(0, 1)) = 1

        _OffsetFactor("Offset Factor", Range(-1, 1)) = 0
        _OffsetUnit("Offset Unit", Range(-1, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Offset[_OffsetFactor],[_OffsetUnit]

        //Front 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
            };

            struct v2f
            {
                UNITY_VERTEX_OUTPUT_STEREO
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            float4 _ColorKey1;
            float4 _ColorKey2;
            float4 _ColorKey3;
            float4 _ColorKey4;
            float4 _ColorKey5;
            float _Dist1;
            float _Dist2;
            float _Dist3;
            float _Dist4;
            float _Dist5;
            float3 _Center;
            float _Alpha;
            float _UseGradient;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float peakStep(float a, float b, float x)
            {
                return step(a, x) - step(b, x);
            }

            float4 calculateColor(float dist)
            {
                float4 color =
                    peakStep(_Dist1, _Dist2, dist) * lerp(_ColorKey1, _ColorKey2, (dist - _Dist1) / (_Dist2 - _Dist1)) +
                    peakStep(_Dist2, _Dist3, dist) * lerp(_ColorKey2, _ColorKey3, (dist - _Dist2) / (_Dist3 - _Dist2)) +
                    peakStep(_Dist3, _Dist4, dist) * lerp(_ColorKey3, _ColorKey4, (dist - _Dist3) / (_Dist4 - _Dist3)) +
                    step(_Dist4, dist) * lerp(_ColorKey4, _ColorKey5, (dist - _Dist4) / (_Dist5 - _Dist4));
                return color;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = calculateColor(distance(i.worldPos, _Center)) * _UseGradient + _ColorKey5 * (1-_UseGradient);
                col.a *= _Alpha;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
