Shader "Custom/Wing"
{
    Properties
    {
        [PerRendererData] _MainTex ("Albedo (RGB)", 2D) = "white" { }

        _Alpha ("Alpha", 2D) = "white" { }
        _Gray ("Gray", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        _BorderColor ("BorderColor", Color) = (1, 1, 1, 1)
        _Value ("Value", Float) = 0
        _Width ("Width", Float) = 0
        _Rate ("Rate", Float) = 0
        _Border ("Border", Float) = 0

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
            Stencil
            {
                Ref 1
                Comp Greater
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            // 透明度相乘
            // 一般用来进行遮罩过滤
            fixed4 MultiplyAlpha(fixed4 source, fixed4 target)
            {
                source.a *= target.a;
                return source;
            }

            // 颜色覆盖
            fixed4 CoverColor(fixed4 source, fixed4 target)
            {
                if (target.a > 0)
                {
                    source.rgb = target.rgb;
                }
                return source;
            }

            // 三角周期函数
            // 由0按三角函数规律变化到1,再由1按三角函数规律变化到0
            float TrigonometricFunc(float value)
            {
                value -= (int)value;
                return(1 - cos(2 * 3.14 * value)) / 2;
            }

            float ToTrigonometricFunc(float value, float target, float range)
            {
                if (value < target - range || value > target + range) return 0;

                return TrigonometricFunc((target + range - value) / range / 2);
            }

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

            sampler2D _MainTex;
            sampler2D _Gray;
            sampler2D _Alpha;
            fixed4 _Color;
            fixed4 _BorderColor;
            float _Rate;
            float _Value;
            float _Width;
            float _Border;

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
                fixed4 output = tex2D(_MainTex, input.uv);
                fixed4 gray = tex2D(_Gray, input.uv);
                fixed4 alpha = tex2D(_Alpha, input.uv);

                #ifdef UNITY_UI_CLIP_RECT
                    output.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                    clip(output.a - 0.001);
                #endif
                fixed4 output2 = output;
                // 透明值
                output.a = ToTrigonometricFunc(gray.a, _Value, _Width);
                // 颜色值
                output.rgb = _Color.rgb + output.a * _Rate;
                // 遮罩过滤
                output = MultiplyAlpha(output, alpha);

                // 边框透明值
                output2.a = ToTrigonometricFunc(gray.a, _Value, _Width + _Border);
                // 边框颜色值
                output2.rgb = _BorderColor.rgb;
                output2.a = output2.a * 2;
                // 遮罩过滤
                output2 = MultiplyAlpha(output2, alpha);

                output = CoverColor(output2, output);

                return output;
            }

            ENDCG

        }
    }
    FallBack "Diffuse"
}
