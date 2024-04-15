Shader "HTC/Shape/GizmoLine"
{
    Properties
    {
        _MainTex("Particle Texture", 2D) = "white" {}
        _ColorMultiplier("Color Multiplier", Color) = (1, 1, 1, 1)
        _OffsetFactor("Offset Factor", Range(-1, 1)) = 0
        _OffsetUnit("Offset Unit", Range(-1, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off 
            Lighting Off 
            ZWrite Off
            Offset[_OffsetFactor],[_OffsetUnit]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorMultiplier;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR0;
       
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR0;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color * tex2D(_MainTex, i.texcoord) * _ColorMultiplier;
            }
            ENDCG
        }
    }
}
