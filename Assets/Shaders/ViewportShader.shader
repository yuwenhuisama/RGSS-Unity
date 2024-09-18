Shader "Costum/ViewportShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _BushOpacity ("Bush Opacity", Range(0, 1)) = 1
        _BushDepth ("Bush Depth", Range(0, 1)) = 0

        _MixColor ("Color to mixture", Vector) = (0,0,0,0)
        _Tone ("Tone to mixture", Vector) = (0,0,0,0)

        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashProgress ("Flash Progress", Range(0, 1)) = 0

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

            fixed4 _MixColor;
            fixed4 _Tone;

            fixed4 _FlashColor;
            float _FlashProgress;

            const fixed3 lumaF = float3(.299, .587, .114);
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Apply gray
                float luma = dot(col.rgb, lumaF);
                col.rgb = lerp(col.rgb, float3(luma, luma, luma), _Tone.w);

                // Apply tone
                col.rgb += _Tone.rgb;

                // Apply flush effect
                col.rgba = lerp(col.rgba, _FlashColor.rgba, _FlashProgress);
                
                return col;
            }
            ENDCG
        }
    }
}