Shader "Custom/ScreenShear"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShearAmount ("Shear Amount", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _ShearAmount;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float shearOffset = _ShearAmount * (sin(i.uv.y * 50.0) - 0.5);
                float2 uv = i.uv;
                uv.x += shearOffset;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
