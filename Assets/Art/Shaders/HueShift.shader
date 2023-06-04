Shader "Unlit/UnlitSpriteHueShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HueShift ("Hue Shift", Range(0.0, 6.283185307)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off

        

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _HueShift;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                // apply hue shift
                fixed3 colShift = {col.r, col.g, col.b};
                
                const fixed3 k = fixed3(0.57735, 0.57735, 0.57735);
                half cosAngle = cos(_HueShift);
                colShift = colShift * cosAngle + cross(k, colShift) * sin(_HueShift) + k * dot(k, colShift) * (1.0 - cosAngle);

                fixed4 newCol = {colShift.r, colShift.g, colShift.b, col.a};

                return newCol;
            }

            ENDCG
        }
    }
}
