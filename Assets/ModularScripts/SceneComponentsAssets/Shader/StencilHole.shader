Shader "HTC/Shape/StencilHole"
{
    Properties
    {
        _StencilRef("Stencil Ref", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comp", Int) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpt("Stencil Opt", Int) = 2

        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTestMode("ZTestMode", Int) = 0
        [Enum(Off, 0, On, 1)] _ZWriteMode("ZWriteMode", Int) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }

        Pass
        {
            Stencil
            {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass[_StencilOpt]
            }

            ColorMask 0
            ZWrite [_ZWriteMode]
            ZTest [_ZTestMode]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
               
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                 return fixed4(0.0, 0.0, 0.0, 0.0);
            }
            ENDCG
        }
    }
}
