Shader "Custom/TestShader40" {
 Properties{
  _MainTex("MainTex",2D)="White"{}
  _Scale("Scale",Range(0,1))=0
 }
 SubShader{
  Tags { "RenderType"="Opaque" }
  Pass{
   CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 3.0
    #include "Lighting.cginc"
 
    sampler2D  _MainTex;
    float4 _MainTex_ST;
    float _Scale;
 
    struct a2v{
     float4 pos:POSITION;
     float4 uv:TEXCOORD0;
    };
 
    struct v2f{
     float4 wPos:SV_POSITION;
     float2 uv:TEXCOORD0;
    };
 
    v2f vert(a2v v){
     v2f o;
     o.wPos = UnityObjectToClipPos(v.pos);
     o.uv=TRANSFORM_TEX(v.uv,_MainTex);
 
     return o;
    }

    //2D
    float Random2DTo1D(float2 value,float a ,float2 b)
    {			
        //avaoid artifacts
        float2 smallValue = sin(value);
        //get scalar value from 2d vector	
        float  random = dot(smallValue,b);
        random = frac(sin(random) * a);
        return random;
    }

    float2 Random2DTo2D(float2 value){
        return float2(
            Random2DTo1D(value,14375.5964, float2(15.637, 76.243)),
            Random2DTo1D(value,14684.6034,float2(45.366, 23.168))
        );
    }

    float2 Random2DTo2DLimit(float2 value, float limit) {
        float2 randomValue = Random2DTo2D(value);
        return float2(randomValue.x * limit, randomValue.y * limit);
    }
 
    float4 frag(v2f o):SV_TARGET{
        fixed4 color = tex2D(_MainTex,o.uv);//,float2(_Scale,_Scale),float2(_Scale,_Scale)
        fixed4 color2 = tex2D(_MainTex, o.uv + Random2DTo2DLimit(o.uv, _Scale));
        fixed4 color3 = tex2D(_MainTex, o.uv - Random2DTo2DLimit(o.uv, _Scale));
        // //return color;
        // float2 uv1= o.uv +float2(_Scale,_Scale);
        // fixed4 color2 = tex2D(_MainTex,uv1);
        
        // float2 uv2= o.uv -float2(_Scale,_Scale);
        // fixed4 color3 = tex2D(_MainTex,uv2);
        
        return (color+color2+color3)/3;
    }
   ENDCG
  }
 }
}