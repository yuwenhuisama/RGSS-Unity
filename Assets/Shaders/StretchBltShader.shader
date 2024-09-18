Shader "Custom/StretchBltShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SrcRect ("Source Rect", Vector) = (0,0,1,1)
        _Opacity ("Opacity", Float) = 1.0
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _SrcRect;
            float _Opacity;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _SrcRect.zw + _SrcRect.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                color.a *= _Opacity;
                return color;
            }
            ENDCG
        }
    }
}