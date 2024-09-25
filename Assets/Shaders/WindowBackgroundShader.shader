Shader "Custom/WindowBackgroundShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tone ("Tone", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _Tone;
            const fixed3 lumaF = float3(.299, .587, .114);

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Apply gray
                float luma = dot(col.rgb, lumaF);
                col.rgb = lerp(col.rgb, float3(luma, luma, luma), _Tone.w);

                // Apply tone
                col.rgb += _Tone.rgb;

                return col;
            }
            ENDCG
        }
    }
}