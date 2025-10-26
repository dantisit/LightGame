Shader "Test/MockDissolveShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            // Properties that SimpleBlockToSpriteSync sets
            float4 _RegionData[10];      // Array of region UV bounds (x, y, width, height)
            float _RegionDissolve[10];   // Array of dissolve amounts per region
            int _RegionCount;            // Number of active regions

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Simple dissolve logic for testing
                float dissolve = 0.0;
                for (int idx = 0; idx < _RegionCount; idx++)
                {
                    float4 region = _RegionData[idx];
                    // Check if current UV is in this region
                    if (i.uv.x >= region.x && i.uv.x <= region.x + region.z &&
                        i.uv.y >= region.y && i.uv.y <= region.y + region.w)
                    {
                        dissolve = max(dissolve, _RegionDissolve[idx]);
                    }
                }

                // Apply dissolve
                col.a *= (1.0 - dissolve);

                return col;
            }
            ENDCG
        }
    }
}
