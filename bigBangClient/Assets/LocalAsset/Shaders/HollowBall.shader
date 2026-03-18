Shader "Custom/HollowBall"
{
    Properties
    {
        [PerRendererData] _MainTex ("Albedo (RGB)", 2D) = "white" { }

        _Mask ("Mask", 2D) = "white" { }
        
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
            // 模板
            Stencil
            {
                Ref 2
                Comp Greater
                Pass replace
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
            sampler2D _Mask;

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
                // 球圈圈
                fixed4 mask = tex2D(_Mask, input.uv);
                // 抛弃片元
                if (mask.a <= 0.001) discard;

                fixed4 output = tex2D(_MainTex, input.uv);

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
    FallBack "Diffuse"
}
