Shader "Custom/SpriteShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0, 1)) = 1
        _Mirror ("Mirror", Range(0, 1)) = 0

        _WaveAmp ("Wave Amplitude", Float) = 0
        _WaveLength ("Wave Length", Float) = 1
        _WavePhase ("Wave Phase", Float) = 0
        _WaveTexWidth ("Wave texture Width", Float) = 0
        _WaveTexHeight ("Wave texture Height", Float) = 0

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
            float _Opacity;
            float _Mirror;

            float _WaveLength;
            float _WavePhase;
            float _WaveTexWidth;
            float _WaveTexHeight;
            float _WaveAmp;

            float _BushOpacity;
            float _BushDepth;

            fixed4 _MixColor;
            fixed4 _Tone;

            fixed4 _FlashColor;
            float _FlashProgress;

            const fixed3 lumaF = float3(.299, .587, .114);

            half4 wave_effect(float2 uv)
            {
                float wav = _WaveAmp / _WaveTexWidth;
                int i = (int)(uv.y / (8.0 / _WaveTexHeight));
                float phase_angle = _WavePhase * UNITY_PI / 180;
                float length_angle = i * (8 / _WaveTexHeight) * 2 * UNITY_PI / (_WaveLength / _WaveTexHeight);
                float offset = wav * sin(length_angle + phase_angle);

                float left = offset;
                float right = 1.0;
                float inWaveRange = step(left, uv.x) * step(uv.x, right);

                float2 uvToSample = lerp(uv, float2((uv.x - offset), uv.y), inWaveRange);
                fixed4 col = tex2D(_MainTex, uvToSample) * inWaveRange + float4(0, 0, 0, 0) * (1.0 - inWaveRange);
                return col;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = tex2D(_MainTex, uv);

                // Apply mirror effect
                uv.x = lerp(uv.x, 1 - uv.x, step(_Mirror, 0.5));

                // Apply wave effect
                float useWave = clamp(_WaveAmp, 0.0, 1.0);
                col = lerp(col, wave_effect(uv), useWave);

                // Apply gray
                float luma = dot(col.rgb, lumaF);
                col.rgb = lerp(col.rgb, float3(luma, luma, luma), _Tone.w);

                // Apply tone
                col.rgb += _Tone.rgb;

                // Apply opacity effect
                col.a = col.a * _Opacity;

                // Apply color
                col.rgb = lerp(col.rgb, _MixColor.rgb, _MixColor.a);

                // Apply bush effect
                float underBush = step(uv.y, _BushDepth);
                col.a = col.a * lerp(1.0, _BushOpacity, underBush);

                // Apply flush effect
                col.rgba = lerp(col.rgba, _FlashColor.rgba, _FlashProgress);

                return col;
            }
            ENDCG
        }
    }
}