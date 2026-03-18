Shader "Babu/DecalUV2ColorAddByDistance"
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_DecalColor ("Decal Color", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" { }
		_DecalTex ("Decal (RGBA)", 2D) = "black" { }

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
            uniform sampler2D _DecalTex; uniform float4 _DecalTex_ST;
            fixed4 _Color;
            fixed4 _DecalColor;

            fixed4 _TargetColor;
            float _TargetPercent;
            float _DistanceNear;
            float _DistanceFar;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };
            struct VertexOutput
            {
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 pos : SV_POSITION;
                float percent : TEXCOORD2;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.uv0;
                o.uv1 = v.uv1;
                o.pos = UnityObjectToClipPos(v.vertex);

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

            fixed4 frag(VertexOutput i, float facing : VFACE) : COLOR
            {
                fixed4 c = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
                half4 decal = tex2D(_DecalTex, TRANSFORM_TEX(i.uv1, _DecalTex));
                c *= _Color;
                c.rgb = lerp(c.rgb, decal.rgb * _DecalColor.rgb, decal.a);
                return c * i.percent + (1 - i.percent) * _TargetColor;
            }
            ENDCG
        }
	}

	Fallback "Diffuse"
}
