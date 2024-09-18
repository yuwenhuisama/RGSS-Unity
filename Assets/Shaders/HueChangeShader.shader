Shader "Custom/HueShiftShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HueAngle ("Hue Angle", Range(0, 360)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        Pass
        {
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
            float _HueAngle;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 HueShift(half4 color, float shift)
            {
                float3 k = float3(0.57735, 0.57735, 0.57735);
                float cosAngle = cos(shift * 6.283185);
                float sinAngle = sin(shift * 6.283185);
                float3x3 rotationMatrix = float3x3(
                    cosAngle + (1.0 - cosAngle) * k.x * k.x, (1.0 - cosAngle) * k.x * k.y - sinAngle * k.z,
                    (1.0 - cosAngle) * k.x * k.z + sinAngle * k.y,
                    (1.0 - cosAngle) * k.y * k.x + sinAngle * k.z, cosAngle + (1.0 - cosAngle) * k.y * k.y,
                    (1.0 - cosAngle) * k.y * k.z - sinAngle * k.x,
                    (1.0 - cosAngle) * k.z * k.x - sinAngle * k.y, (1.0 - cosAngle) * k.z * k.y + sinAngle * k.x,
                    cosAngle + (1.0 - cosAngle) * k.z * k.z
                );
                return half4(mul(rotationMatrix, color.rgb), color.a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                return HueShift(color, _HueAngle * UNITY_PI / 180);
            }
            ENDCG
        }
    }
}