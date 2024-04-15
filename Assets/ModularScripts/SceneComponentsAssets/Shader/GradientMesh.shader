Shader "HTC/Shape/GradientMesh"
{
    Properties
    {
        _Size("Size", Vector) = (1, 1, 0, 0)
        _LeftLerpValue("Left lerp value", Range(0, 1)) = 0
        _RightLerpValue("Right lerp value", Range(0, 1)) = 0
        _DepthLerpValue("Depth lerp value", Range(0, 1)) = 0

        _LeftColorKey1("LeftColor1", Color) = (1, 1, 1 ,1)
        _LeftColorKey2("LeftColor2", Color) = (1, 1, 1 ,1)
        _LeftColorKey3("LeftColor3", Color) = (1, 1, 1 ,1)
        _LeftColorKey4("LeftColor4", Color) = (1, 1, 1 ,1)
        _LeftDist1("LeftDist1", Range(0, 1)) = 0
        _LeftDist2("LeftDist2", Range(0, 1)) = 0.1
        _LeftDist3("LeftDist3", Range(0, 1)) = 0.2
        _LeftDist4("LeftDist4", Range(0, 1)) = 1

        _RightColorKey1("RightColor1", Color) = (1, 1, 1 ,1)
        _RightColorKey2("RightColor2", Color) = (1, 1, 1 ,1)
        _RightColorKey3("RightColor3", Color) = (1, 1, 1 ,1)
        _RightColorKey4("RightColor4", Color) = (1, 1, 1 ,1)
        _RightDist1("RightDist1", Range(0, 1)) = 0
        _RightDist2("RightDist2", Range(0, 1)) = 0.1
        _RightDist3("RightDist3", Range(0, 1)) = 0.2
        _RightDist4("RightDist4", Range(0, 1)) = 1

        _DepthColorKey1("DepthColor1", Color) = (1, 1, 1 ,1)
        _DepthColorKey2("DepthColor2", Color) = (1, 1, 1 ,1)
        _DepthColorKey3("DepthColor3", Color) = (1, 1, 1 ,1)
        _DepthColorKey4("DepthColor4", Color) = (1, 1, 1 ,1)
        _DepthDist1("DepthDist1", Range(0, 1)) = 0
        _DepthDist2("DepthDist2", Range(0, 1)) = 0.1
        _DepthDist3("DepthDist3", Range(0, 1)) = 0.2
        _DepthDist4("DepthDist4", Range(0, 1)) = 1

        _GradientWidth("Gradient length", float) = 0.1 //real world size
        _GradientPower("Power (Front)", Range(0, 5)) = 1.5
        _GradientCenter("Center Color", Color) = (1, 1, 1 ,1)
        _GradientBorder("Border Color", Color) = (1, 1, 1 ,1)

        _OffsetFactor("Offset Factor", Range(-1, 1)) = 0
        _OffsetUnit("Offset Unit", Range(-1, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Lighting Off
        Offset[_OffsetFactor],[_OffsetUnit]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                float4 localPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 _Size;
            float3 _LocalPivot;
            float _LeftLerpValue;
            float _RightLerpValue;
            float _DepthLerpValue;

            float _GradientWidth;
            float _GradientPower;
            fixed4 _GradientCenter;
            fixed4 _GradientBorder;

            float4 _LeftColorKey1;
            float4 _LeftColorKey2;
            float4  _LeftColorKey3;
            float4  _LeftColorKey4;
            float _LeftDist1;
            float _LeftDist2;
            float _LeftDist3;
            float _LeftDist4;

            float4 _RightColorKey1;
            float4 _RightColorKey2;
            float4 _RightColorKey3;
            float4 _RightColorKey4;
            float _RightDist1;
            float _RightDist2;
            float _RightDist3;
            float _RightDist4;

            float4 _DepthColorKey1;
            float4 _DepthColorKey2;
            float4 _DepthColorKey3;
            float4 _DepthColorKey4;
            float _DepthDist1;
            float _DepthDist2;
            float _DepthDist3;
            float _DepthDist4;

            v2f vert (appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.localPos = v.vertex;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float distFromLeftCorner(float2 p)
            {
                return abs(p.x + p.y + (_Size.x + _Size.y) / 2) / sqrt(2);
            }

            float distFromRightCorner(float2 p)
            {
                return abs(p.x - p.y - (_Size.x + _Size.y) / 2) / sqrt(2);
            }

            float distFromTop(float2 p)
            {
                return abs(p.y - _Size.y / 2);
            }

            float peakStep(float a, float b, float x)
            {
                return step(a, x) - step(b, x);
            }

            float4 calculateLeftColor(float dist)
            {
                float4 color =
                    peakStep(_LeftDist1, _LeftDist2, dist) * lerp(_LeftColorKey1, _LeftColorKey2, (dist - _LeftDist1) / (_LeftDist2 - _LeftDist1)) +
                    peakStep(_LeftDist2, _LeftDist3, dist) * lerp(_LeftColorKey2, _LeftColorKey3, (dist - _LeftDist2) / (_LeftDist3 - _LeftDist2)) +
                    step(_LeftDist3, dist) * lerp(_LeftColorKey3, _LeftColorKey4, (dist - _LeftDist3) / (_LeftDist4 - _LeftDist3));
                return color;
            }

            float4 calculateRightColor(float dist)
            {
                float4 color =
                    peakStep(_RightDist1, _RightDist2, dist) * lerp(_RightColorKey1, _RightColorKey2, (dist - _RightDist1) / (_RightDist2 - _RightDist1)) +
                    peakStep(_RightDist2, _RightDist3, dist) * lerp(_RightColorKey2, _RightColorKey3, (dist - _RightDist2) / (_RightDist3 - _RightDist2)) +
                    step(_RightDist3, dist) * lerp(_RightColorKey3, _RightColorKey4, (dist - _RightDist3) / (_RightDist4 - _RightDist3));
                return color;
            }

            float4 calculateDepthColor(float dist)
            {
                float4 color =
                    peakStep(_DepthDist1, _DepthDist2, dist) * lerp(_DepthColorKey1, _DepthColorKey2, (dist - _DepthDist1) / (_DepthDist2 - _DepthDist1)) +
                    peakStep(_DepthDist2, _DepthDist3, dist) * lerp(_DepthColorKey2, _DepthColorKey3, (dist - _DepthDist2) / (_DepthDist3 - _DepthDist2)) +
                    step(_DepthDist3, dist) * lerp(_DepthColorKey3, _DepthColorKey4, (dist - _DepthDist3) / (_DepthDist4 - _DepthDist3));
                return color;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //calculate gradient face
                float2 localCoord = i.localPos.xz - _Size / 2;
                float leftCornerDist = distFromLeftCorner(localCoord);
                float4 leftCornerColor = calculateLeftColor(leftCornerDist);
                float rightCornerDist = distFromRightCorner(localCoord);
                float4 rightCornerColor = calculateRightColor(rightCornerDist);
                float depthDist = distFromTop(localCoord);
                float4 depthColor = calculateDepthColor(depthDist);

                //calculate gradient border
                float distX = _Size.x / 2 - abs(localCoord.x);
                float distZ = _Size.y / 2 - abs(localCoord.y);
                float rx = clamp(_GradientWidth - distX, 0, _GradientWidth);
                float rz = clamp(_GradientWidth - distZ, 0, _GradientWidth);
                float d = sqrt(rx * rx + rz * rz);
                float lerpValue = d / _GradientWidth;
                float4 c = lerp(_GradientCenter, _GradientBorder, pow(lerpValue, _GradientPower));
                float gradientColorLerpValue = clamp(1 - _LeftLerpValue - _RightLerpValue - _DepthLerpValue, 0, 1);

                c = c * gradientColorLerpValue + leftCornerColor * _LeftLerpValue + rightCornerColor * _RightLerpValue + depthColor * _DepthLerpValue;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, c);
                return c;
            }
            ENDCG
        }
    }
}
