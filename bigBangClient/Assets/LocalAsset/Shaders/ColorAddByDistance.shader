Shader "Babu/ColorAddByDistance"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "Black" { }
        _TargetColor("Target Color", Color) = (0.662745098, 0.8470588235, 0.89803921568, 1)
        _TargetPercent("Slider", Range(0, 1)) = 1
        _DistanceNear("Distance Near", Float) = 10.2
        _DistanceFar("Distance Far", Float) = 13
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            fixed4 _TargetColor;
            float _TargetPercent;
            float _DistanceNear;
            float _DistanceFar;

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float percent : TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                // 相机坐标系的物体坐标
                float3 posView = mul(UNITY_MATRIX_MV, v.vertex).xyz;
                // 计算与相机距离
                float dis = length(posView);
                // 计算percent
                if (dis < _DistanceNear)
                {
                    o.percent = 1;
                }
                else if (dis > _DistanceFar)
                {
                    o.percent = 1 - _TargetPercent;
                }
                else
                {
                    float percent = (dis - _DistanceNear) / (_DistanceFar - _DistanceNear);
                    o.percent = 1 - percent * _TargetPercent;
                }
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed4 color = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                return color * i.percent + (1 - i.percent) * _TargetColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
