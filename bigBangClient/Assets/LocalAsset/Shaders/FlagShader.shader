Shader "Custom/FlagShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        _Tex1 ("Tex1", 2D) = "white" { }
        _Tex2 ("Tex2", 2D) = "white" { }
        _Logo ("Logo", 2D) = "white" { }
        _Gray ("Gray", 2D) = "white" { }
        _OffsetX ("OffsetX", Range(-1, 1)) = 0
        _OffsetY ("OffsetY", Range(-1, 1)) = 0
        _ScaleX ("ScaleX", Range(0, 2)) = 1
        _ScaleY ("ScaleY", Range(0, 2)) = 1
        _Width ("Width", float) = 1
        _Value ("Value", Range(-2, 2)) = 0
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _TargetColor ("Target Color", Color) = (0.662745098, 0.8470588235, 0.89803921568, 1)
        _TargetPercent ("Slider", Range(0, 1)) = 1
        _DistanceNear ("Distance Near", Float) = 10.2
        _DistanceFar ("Distance Far", Float) = 13

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
        // Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Tags { "RenderType" = "Opaque" }


        // Stencil
        // {
        //     Ref [_Stencil]
        //     Comp [_StencilComp]
        //     Pass [_StencilOp]
        //     ReadMask [_StencilReadMask]
        //     WriteMask [_StencilWriteMask]
        // }

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

            // #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            // #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float percent : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            fixed4 _TargetColor;
            float _TargetPercent;
            float _DistanceNear;
            float _DistanceFar;

            sampler2D _Tex1;
            sampler2D _Tex2;
            sampler2D _Logo;
            sampler2D _Gray;
            float _Value;
            float _ScaleX;
            float _ScaleY;
            float _OffsetX;
            float _OffsetY;
            float _Width ;

            // foreground:前景图片
            // background:背景图片
            // rate:过渡比率,为0完全显示背景，为1完全显示前景。0-1为过渡值
            fixed4 Transition(fixed4 foreground, fixed4 background, float rate)
            {
                if (rate > 0.5)
                {
                    return background;
                }
                else
                {
                    return foreground;
                }
                rate = clamp(rate, 0, 1);
                return background * background.a * rate + foreground * foreground.a * (1 - rate);
            }

            
            // 颜色填充
            fixed4 FillColor(fixed4 source, fixed4 target)
            {
                // 输入标识颜色
                source.rgb = source.rgb * (1 - target.a) + target.rgb * target.a;
                return source;
            }

            v2f vert(appdata_base v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                // OUT.color = v.color * _Color;

                // 相机坐标系的物体坐标
                float3 posView = mul(UNITY_MATRIX_MV, v.vertex).xyz;
                // 计算与相机距离
                float dis = length(posView);
                // 计算percent
                if (dis < _DistanceNear)
                {
                    OUT.percent = 1;
                }
                else if (dis > _DistanceFar)
                {
                    OUT.percent = 1 - _TargetPercent;
                }
                else
                {
                    float percent = (dis - _DistanceNear) / (_DistanceFar - _DistanceNear);
                    OUT.percent = 1 - percent * _TargetPercent;
                }
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                fixed4 tex1 = tex2D(_Tex1, IN.texcoord);
                fixed4 tex2 = tex2D(_Tex2, IN.texcoord);
                float2 uv = float2((IN.texcoord.x + _OffsetX) * _ScaleX, (IN.texcoord.y + _OffsetY) * _ScaleY);
                fixed4 gray = tex2D(_Gray, IN.texcoord);
                fixed4 logo = tex2D(_Logo, uv);

                // #ifdef UNITY_UI_CLIP_RECT
                //     color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                // #endif

                // #ifdef UNITY_UI_ALPHACLIP
                //     clip(color.a - 0.001);
                // #endif

                fixed4 background = tex2;
                fixed4 foreground = tex1;
                // fixed4 output = Transition(background, foreground, gray.a + _Value);
                fixed4 output = Transition(background, foreground, length(gray.rgb) / sqrt(3) + _Value);

                output = FillColor(output, logo);
                return output * IN.percent + (1 - IN.percent) * _TargetColor;
            }
            ENDCG

        }
    }
}
