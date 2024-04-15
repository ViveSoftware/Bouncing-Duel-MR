Shader "HTC/Shape/PointGizmo"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _EmissionColor("Color", Color) = (1, 1, 1, 1)
        _RimValue("Rim", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Cull Back

            Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                half3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;
            float4 _EmissionColor;
            float _RimValue;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                float rimValue = saturate(dot(viewDir, i.worldNormal) - _RimValue) / (1-_RimValue);

                fixed4 col = _Color + _EmissionColor * pow(rimValue, 3);
                col.a = rimValue;

                return col;
            }
            ENDCG
        }
    }
}
