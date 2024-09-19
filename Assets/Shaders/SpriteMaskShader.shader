Shader "Custom/SpriteMaskShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Region ("Region", Vector) = (0, 0, 1, 1)
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
            float4 _Region;

            fixed4 frag(v2f i) : SV_Target
            {
                float4 region = _Region;
                region.y = 1.0 - region.y - region.w;
                float2 mask = step(region.xy, i.uv) * step(i.uv, region.xy + region.zw);
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= mask.x * mask.y;
                return col;
            }
            ENDCG
        }
    }
}