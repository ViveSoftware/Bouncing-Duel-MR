Shader "HTC/Shape/TransparentColor"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _BackColor("Back Face Color", Color) = (1, 1, 1, 0.4)
        _MainTex("Main Texture", 2D) = "white" {}
        _OffsetFactor("Offset Factor", Range(-1, 1)) = 0
        _OffsetUnit("Offset Unit", Range(-1, 1)) = 0
    }
    SubShader
    {
        ZWrite Off
        Offset[_OffsetFactor],[_OffsetUnit]

        Pass
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                return _Color * tex2D(_MainTex, i.texcoord);
            }

            ENDCG
        }

        Pass
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _BackColor;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                return _BackColor * tex2D(_MainTex, i.texcoord);
            }

            ENDCG
        }
    }
}
