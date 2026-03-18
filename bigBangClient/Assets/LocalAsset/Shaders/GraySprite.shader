// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/GraySprite"
{
    Properties     
    {     
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}     
        _Color ("Tint", Color) = (1,1,1,1)
		_Fade("Fade", Range(0, 1)) = 0

		//MASK SUPPORT END
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
		//MASK SUPPORT END
    }     
     
    SubShader     
    {     
        Tags     
        {      
            "Queue"="Transparent"      
            "IgnoreProjector"="True"      
            "RenderType"="Transparent"      
            "PreviewType"="Plane"     
            "CanUseSpriteAtlas"="True"     
        }

        ZWrite Off
        ZTest[unity_GUIZTestMode]

        // 源rgba*源a + 背景rgba*(1-源A值)   
        Blend SrcAlpha OneMinusSrcAlpha  

		//MASK SUPPORT ADD
		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
		ColorMask[_ColorMask]
		//MASK SUPPORT END
     
        Pass     
        {     
            CGPROGRAM     
            #pragma vertex vert     
            #pragma fragment frag     
            #include "UnityCG.cginc"     
                 
            struct appdata_t     
            {     
                float4 vertex   : POSITION;     
                float4 color    : COLOR;     
                float2 texcoord : TEXCOORD0;     
            };     
     
            struct v2f     
            {     
                float4 vertex   : SV_POSITION;     
                fixed4 color    : COLOR;     
                half2 texcoord  : TEXCOORD0;     
            };     
               
            sampler2D _MainTex;       
            fixed4 _Color;
			fixed _Fade;
     
            v2f vert(appdata_t IN)     
            {     
                v2f OUT;     
                OUT.vertex = UnityObjectToClipPos(IN.vertex);     
                OUT.texcoord = IN.texcoord;     
#ifdef UNITY_HALF_TEXEL_OFFSET     
                OUT.vertex.xy -= (_ScreenParams.zw-1.0);     
#endif     
                OUT.color = IN.color * _Color;     
                return OUT;  
            }  
     
			half smooth(float min, float max, fixed t)
			{
				return min + (max - min) * t;
			}

            fixed4 frag(v2f IN) : SV_Target     
            {     
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;     
                float grey = dot(color.rgb, fixed3(0.22, 0.707, 0.071));   
				half r = smooth(color.r, grey, _Fade);
				half g = smooth(color.g, grey, _Fade);
				half b = smooth(color.b, grey, _Fade);

                return half4(r,g,b,color.a);     
            }     
            ENDCG     
        }     
    }     
}
