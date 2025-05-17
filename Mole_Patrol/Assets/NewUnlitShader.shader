Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1) // Default color tint
        _MainTex ("Texture", 2D) = "white" {} // Texture property
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _Color; // Color tint
            sampler2D _MainTex; // Texture sampler
            float4 _MainTex_ST; // Texture tiling and offset

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // Apply texture transform
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv); // Sample the texture
                return texColor * _Color; // Multiply texture color with tint
            }
            ENDCG
        }
    }
}
