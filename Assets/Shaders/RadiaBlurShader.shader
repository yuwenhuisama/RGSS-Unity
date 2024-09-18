Shader "Custom/RadialBlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TempTex ("Temp Texture", 2D) = "white" {}

        _Angle ("Angle", Float) = 0.0
        _Division ("Blur Samples", Float) = 10.0
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Division;
            float _Angle;

            const float2 blurCenter = float2(0.5, 0.5);
            const float blurStrength = 0.5;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 rotateUV(float2 uv, float degrees)
            {
                const float Deg2Rad = (UNITY_PI * 2.0) / 360.0;
                float rotationRadians = degrees * Deg2Rad;
                float s = sin(rotationRadians);
                float c = cos(rotationRadians);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                uv -= 0.5;
                uv = mul(rotationMatrix, uv);
                uv += 0.5;
                return uv;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = half4(0, 0, 0, 0);
                float2 uv = i.uv;

                for (float j = 0.0; j < _Division; j++)
                {
                    float2 rotatedUV = rotateUV(uv, _Angle / _Division * j);
                    color += tex2D(_MainTex, rotatedUV);
                }
                return color / _Division;
            }
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _TempTex;
            float _Division;

            const float2 blurCenter = float2(0.5, 0.5);
            const float blurStrength = 0.5;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = half4(0, 0, 0, 0);
                float2 uv = i.uv;
                float2 dir = uv - blurCenter;
                float dist = length(dir);
                dir = normalize(dir);

                for (float j = 0.0; j < _Division; j++)
                {
                    float scale = 1.0 + j * blurStrength / _Division;
                    color += tex2D(_TempTex, blurCenter + dir * dist * scale);
                }

                return color / _Division;
            }
            ENDCG
        }
    }
}