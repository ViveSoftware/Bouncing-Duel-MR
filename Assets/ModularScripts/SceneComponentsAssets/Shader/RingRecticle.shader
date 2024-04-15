// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "HTC/Shape/RingRecticle"
{
    Properties
    {
        _CenterColor("Center Color", Color) = (1,1,1,1)
        _RingColor("Ring Color", Color) = (1,1,1,1)
        _RingScale("Ring Scale", float) = 1.2
    }
    SubShader
    {
        ZWrite Off

        Tags { "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZTest Off
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float3 normal : NORMAL;
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half _RingScale;

            v2f vert(appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                v.vertex.xyz *= _RingScale;
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return float4(1,1,1,1);
            }
            ENDCG
        }

        Pass
        {
            ZTest Off
            ColorMask 0

            Stencil
            {
                Ref 2
                Comp Always
                Pass Replace
            }
        }

        Pass
        {
            ZTest Off

            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 normal : NORMAL;
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _RingColor;
            half _RingScale;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                v.vertex.xyz *= _RingScale;
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _RingColor;
            }
            ENDCG
        }
    }
}
