Shader "Custom/JerseyItemShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Albedo (RGB)", 2D) = "white" { }

        _Background ("Background", 2D) = "white" { }
        _BackgroundColor ("Background Color", Color) = (1, 1, 1, 1)
        _Sleeve ("Sleeve", 2D) = "white" { }
        _Pattern ("Pattern", 2D) = "white" { }
        _Mask ("Mask", 2D) = "white" { }
        _MaskColor ("Mask Color", Color) = (1, 1, 1, 1)
        
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
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            sampler2D _Background;
            sampler2D _Mask;
            sampler2D _Pattern;
            sampler2D _Sleeve;

            fixed4 _BackgroundColor;
            fixed4 _MaskColor;
            fixed4 _PatternColor;


            struct VertexInput
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct FragmentInput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            // 遮罩过滤
            fixed4 maskFilter(fixed4 source, fixed4 mask)
            {
                source.a = source.a * mask.a;
                return source;
            }
            
            // 颜色填充
            fixed4 fillColor(fixed4 source, fixed4 target)
            {
                // 输入标识颜色
                source.rgb = source.rgb * (1 - target.a) + target.rgb * target.a;
                return source;
            }

            // 颜色相加
            fixed4 addColor(fixed4 source, fixed4 color)
            {
                source.rgb = source.rgb * color.rgb;
                return source;
            }

            // 颜色相乘
            fixed4 multiplyColor(fixed4 source, fixed4 color)
            {
                source.rgb = source.rgb * color.rgb;
                return source;
            }

            FragmentInput vert(VertexInput input)
            {
                FragmentInput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            fixed4 frag(FragmentInput input) : SV_TARGET
            {
                fixed4 background = tex2D(_Background, input.uv);
                fixed4 pattern = tex2D(_Pattern, input.uv);
                fixed4 mask = tex2D(_Mask, input.uv);
                fixed4 sleeve = tex2D(_Sleeve, input.uv);

                fixed4 jersey = background;
                jersey = multiplyColor(jersey, _BackgroundColor);
                jersey = fillColor(jersey, pattern);

                fixed4 style = multiplyColor(mask, _MaskColor);
                style = fillColor(style, sleeve);
                
                fixed4 output = fillColor(jersey, style);

                #ifdef UNITY_UI_CLIP_RECT
                    output.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                    clip(output.a - 0.001);
                #endif
                
                // 颜色叠加
                output *= input.color;
                return output;
            }

            ENDCG

        }
    }
    FallBack "Diffuse"
}
