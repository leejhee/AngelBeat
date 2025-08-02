Shader "Custom/GaussianBlur"
{
    Properties
    {
        _BlurRadius ("Blur Radius", Range(0, 10)) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        GrabPass { }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;
            float _BlurRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            static const float kernel[11] = {
                0.027, 0.065, 0.121, 0.174, 0.199, 0.174, 0.121, 0.065, 0.027, 0.009, 0.003
            };

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = float2(i.uv.x, 1.0 - i.uv.y);
                float2 texelOffset = float2(_GrabTexture_TexelSize.x * _BlurRadius, 0);
                float4 col = tex2D(_GrabTexture, uv) * kernel[0];

                for (int k = 1; k < 6; ++k)
                {
                    float weight = kernel[k];
                    col += tex2D(_GrabTexture, uv + texelOffset * k) * weight;
                    col += tex2D(_GrabTexture, uv - texelOffset * k) * weight;
                }

                return col;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
