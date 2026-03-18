Shader "Custom/ClubIconShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Albedo (RGB)", 2D) = "white" { }
        _Frame ("Frame", 2D) = "white" { }
        _FrameColor ("Frame Color", Color) = (1, 1, 1, 1)
        _Mask ("Mask", 2D) = "white" { }
        _MaskColor ("Mask COlor", Color) = (1, 1, 1, 1)
        _Pattern ("Pattern", 2D) = "white" { }
        _PatternColor ("Pattern Color", Color) = (1, 1, 1, 1)
        _Flag ("Flag", 2D) = "white" { }
        _FlagColor ("Flag Color", Color) = (1, 1, 1, 1)
        
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
            sampler2D _Frame;
            sampler2D _Mask;
            sampler2D _Pattern;
            sampler2D _Flag;
            fixed4 _FrameColor;
            fixed4 _MaskColor;
            fixed4 _PatternColor;
            fixed4 _FlagColor;


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
                fixed4 main = tex2D(_MainTex, input.uv);
                fixed4 output = fixed4(0, 0, 0, 0);
                // 框架
                fixed4 frame = tex2D(_Frame, input.uv);
                frame.rgb = _FrameColor.rgb;
                // 遮罩
                fixed4 mask = tex2D(_Mask, input.uv);
                mask.rgb = _MaskColor.rgb;
                // 图案
                fixed4 pattern = tex2D(_Pattern, input.uv);
                pattern.rgb = _PatternColor.rgb;
                // 标识
                fixed4 flag = tex2D(_Flag, input.uv);
                flag.rgb = _FlagColor.rgb;
                // 添加框架颜色
                output.rgba = frame.rgba;
                // 添加遮罩颜色
                output.rgb = output.rgb * (1 - mask.a) + mask.rgb * mask.a;
                // 输入图案颜色
                pattern.a = pattern.a - clamp(pattern.a - mask.a, 0, 1);
                output.rgb = output.rgb * (1 - pattern.a) + pattern.rgb * pattern.a;
                // 输入标识颜色
                output.rgb = output.rgb * (1 - flag.a) + flag.rgb * flag.a;

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
