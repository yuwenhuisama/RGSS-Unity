Shader "Custom/TransitionPostprocessShader"
{
    Properties
    {
        _FrozenTex ("Old Texture", 2D) = "white" {}
        _TransitionTex ("Transition Texture", 2D) = "white" {}
        _NewTex ("New Texture", 2D) = "white" {}
        _Vague ("Value", Range(0, 1)) = 0.0
        _Progress ("Transition Progress", Range(0, 1)) = 0.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _FrozenTex;
            sampler2D _TransitionTex;
            sampler2D _NewTex;
            float _Vague;
            float _Progress;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float transV = tex2D(_TransitionTex, uv).r;
                float cTransV = clamp(transV, _Progress, _Progress + _Vague);
                float alpha = (cTransV - _Progress) / _Vague;

                fixed4 newFrag = tex2D(_NewTex, uv);
                fixed4 oldFrag = tex2D(_FrozenTex, uv);

                fixed4 col = lerp(newFrag, oldFrag, alpha);
                return col; 
            }
            ENDCG
        }
    }
}
