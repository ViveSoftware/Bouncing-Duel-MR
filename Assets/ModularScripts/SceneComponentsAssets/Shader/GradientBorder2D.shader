﻿Shader "HTC/Shape/GradientBorder2D"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _FrontColorCenter("Center (Front)", Color) = (1, 1, 1, 1)
        _FrontColorBorder("Border (Front)", Color) = (1, 1, 1, 1)
        _FrontGradientWidth("Gradient length (Front)", float) = 0.1 //real world size
        _FrontPower("Power (Front)", Range(0, 5)) = 1.5

        _BackColorCenter("Center (Back)", Color) = (1, 1, 1, 1)
        _BackColorBorder("Border (Back)", Color) = (1, 1, 1, 1)
        _BackGradientWidth("Gradient length (Back)", float) = 0.1 //real world size
        _BackPower("Power (Back)", Range(0, 5)) = 1.5

        _StencilRef("Stencil Ref", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comp", Int) = 8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpt("Stencil Opt", Int) = 2

        _OffsetFactor("Offset Factor", Range(-1, 1)) = 0
        _OffsetUnit("Offset Unit", Range(-1, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Offset[_OffsetFactor],[_OffsetUnit]

        Stencil
        {
            Ref[_StencilRef]
            Comp[_StencilComp]
            Pass[_StencilOpt]
        }

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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_VERTEX_OUTPUT_STEREO
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FrontColorCenter;
            float4 _FrontColorBorder;
            float _FrontGradientWidth;
            float _FrontPower;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = (i.uv.x >= 0.5) ? 1 - i.uv.x : i.uv.x;
                float y = (i.uv.y >= 0.5) ? 1 - i.uv.y : i.uv.y;
                float scaledX = x * length(unity_ObjectToWorld._m00_m10_m20);
                float scaledY = y * length(unity_ObjectToWorld._m01_m11_m21);
                float rx = clamp(_FrontGradientWidth - scaledX, 0, _FrontGradientWidth);
                float ry = clamp(_FrontGradientWidth - scaledY, 0, _FrontGradientWidth);
                float d = sqrt(rx * rx + ry * ry);
                float lerpValue = d / _FrontGradientWidth;
                fixed4 col = lerp(_FrontColorCenter, _FrontColorBorder, pow(lerpValue, _FrontPower));
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        //Back
        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_VERTEX_OUTPUT_STEREO
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BackColorCenter;
            float4 _BackColorBorder;
            float _BackGradientWidth;
            float _BackPower;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = (i.uv.x >= 0.5) ? 1 - i.uv.x : i.uv.x;
                float y = (i.uv.y >= 0.5) ? 1 - i.uv.y : i.uv.y;
                float scaledX = x * length(unity_ObjectToWorld._m00_m10_m20);
                float scaledY = y * length(unity_ObjectToWorld._m01_m11_m21);
                float rx = clamp(_BackGradientWidth - scaledX, 0, _BackGradientWidth);
                float ry = clamp(_BackGradientWidth - scaledY, 0, _BackGradientWidth);
                float d = sqrt(rx * rx + ry * ry);
                float lerpValue = d / _BackGradientWidth;
                fixed4 col = lerp(_BackColorCenter, _BackColorBorder, pow(lerpValue, _BackPower));
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
