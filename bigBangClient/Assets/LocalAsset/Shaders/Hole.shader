Shader "Custom/Hole"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }

        _Center ("Center", vector) = (0, 0, 0, 0)
        _Radius ("Radius", Range(0, 5000)) = 0
        _Edge ("Edge", Range(0, 1)) = 0.6

        _Color ("Tint", Color) = (1, 1, 1, 1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]
        Pass
        {
            Name "Default"
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityUI.cginc"

            struct a2v
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            float2 _Center;
            float _Radius;
            float _Edge;

            v2f vert(a2v v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                half4 color = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                float dis = distance(i.worldPosition.xy, _Center);
                if (dis <= _Radius)
                {
                    color.a = max(min(i.color.a, (dis - _Edge * _Radius) / ((1 - _Edge) * _Radius)), 0);
                }
                return color;
            }
            ENDCG

        }
    }
    FallBack "Diffuse"
}