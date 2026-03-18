Shader "UI/ChallengeAreaEnter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }

        _Gray ("Gray", 2D) = "white" { }
        _Value ("Value", Range(-1, 0.5)) = 0
        _Gap ("Gap", Range(1, 100)) = 10
        _Mask ("Mask", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            Name "Default"
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            // 线性周期函数
            // 由0均匀变化到1,再由1均匀变化到0
            float LineraFunc(float value)
            {
                value -= (int)value;
                return 2 * (value < 0.5 ? value : (1 - value));
            }

            // 一个周期的线性函数
            // 其余部分为0
            float LinearFuncOnce(float value)
            {
                if (value >= 0 && value <= 1)
                {
                    return LineraFunc(value);
                }
                return 0;
            }

            
            // 三角周期函数
            // 由0按三角函数规律变化到1,再由1按三角函数规律变化到0
            float TrigonometricFunc(float value)
            {
                value -= (int)value;
                return(1 - cos(2 * 3.14 * value)) / 2;
            }

            // 一个周期的三角函数
            // 其余部分为0
            float TrigonometricFuncOnce(float value)
            {
                if (value >= 0 && value <= 1)
                {
                    return TrigonometricFunc(value);
                }
                return 0;
            }

            float SemicircleFunc(float value)
            {
                value -= (int)value;
                return sqrt(1 - pow(value - 0.5f, 2));
            }

            float SemicircleFuncOnce(float value)
            {
                value -= (int)value;
                return sqrt(1 - pow(value - 0.5f, 2));
            }

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            sampler2D _Gray;
            sampler2D _Mask;
            float _Value;
            float _Gap;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 main = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                fixed4 gray = tex2D(_Gray, IN.texcoord);
                fixed4 mask = tex2D(_Mask, IN.texcoord);
                fixed4 output = main;


                // output.a += output.a * LinearFuncOnce((length(gray.rgb) / sqrt(3) + _Value) * _Gap);

                output.a += output.a * TrigonometricFuncOnce((length(gray.rgb) / sqrt(3) + (LineraFunc(_Time.x * 2) - 1) * 1.1) * _Gap) * 2;
                // output.a *= mask.a;

                #ifdef UNITY_UI_CLIP_RECT
                    output.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(output.a - 0.001);
                #endif

                return output;
            }
            ENDCG

        }
    }
}
