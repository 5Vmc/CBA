Shader "NewDecalUV2"
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_DecalColor ("Decal Color", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" { }
		_DecalTex ("Decal (RGBA)", 2D) = "black" { }
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
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.uv0;
                o.uv1 = v.uv1;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(VertexOutput i, float facing : VFACE) : COLOR
            {
                fixed4 c = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
                half4 decal = tex2D(_DecalTex, TRANSFORM_TEX(i.uv1, _DecalTex));
                c *= _Color;
                c.rgb = lerp(c.rgb, decal.rgb * _DecalColor.rgb, decal.a);
                return c;
            }
            ENDCG
        }
	}

	Fallback "Diffuse"
}
