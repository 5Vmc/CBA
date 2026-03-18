//如何求顶点投影到平面上的点(阴影点)
//当平面上取不相等的任意两个点组成一个向量，与平面的法线总是垂直的，向量垂直点乘为0，因此可以通过一个点和一个法线来定义，
//plane方程如下：(P - P0)·N = 0 N=normal，P0表示平面上的一个点，P表示平面上的任意点，当P = P0时 0·N = 0
//射线方程 P = o + t * D，(o为射线起点,t为标量，表示射线原点到和平面交点的距离)联立两个方程式可求交点。方程如下：

//          ( O + D·t - P0 )·N = 0
//       => ( O - P0 )·N + D·N·t = 0
//       => t = ( P0 - O)·N / D·N  ( 其中D·N ≠0 ,向量点积满足分配律)
// p0表示平面上一点中心点（0,0,0） o:顶点世界坐标  N:平面的法向量（0,1,0）D:直射光方向
//注意两点：
//当 D·N = 0 时，表示射线与平面垂直，则射线与平面平行。
//解出 t < 0 时，表示 射线沿着平面相反的半平面发射，也是不相交的（当然如果是直线就没关系啦）

Shader "Pluto/PlanarShadow2"
{
    Properties
    {
        _ShadowColor ("Shadow Color",Color) = (0,0,0,0.5)
        _Center("Center", Vector) = (0,0.001,0,0)
        _Normal("Normal", Vector) = (0,1,0,0)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        // //渲染模型
        // Pass
        // {
        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag

        //     #include "UnityCG.cginc"

        //     struct appdata
        //     {
        //         float4 vertex : POSITION; //模型空间中的顶点坐标
        //         float2 uv : TEXCOORD0;
        //     };

        //     struct v2f
        //     {
        //         float2 uv : TEXCOORD0;
        //         float4 vertex : SV_POSITION;  //裁剪空间中的顶点坐标
        //     };

        //     sampler2D _MainTex;
        //     float4 _MainTex_ST;

        //     v2f vert (appdata v)
        //     {
        //         v2f o;
        //         o.vertex = UnityObjectToClipPos(v.vertex); //将顶点从模型空间转换到裁剪空间中，更高效
        //         o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        //         return o;
        //     }

        //     fixed4 frag (v2f i) : SV_Target
        //     {
        //         // sample the texture
        //         fixed4 col = tex2D(_MainTex, i.uv);
        //         return col;
        //     }
        //     ENDCG
        // }

        //渲染平面阴影Pass
        Pass
        {
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            //  模板测试的判断依据
            //     if((referenceValue & readMask)   ComparisonFunction (stencilBufferValue & readMask))
            //         通过像素
            //     else
            //         抛弃像素

            //  在这个公式中，主要分ComparisonFunction的左边部分和右边部分
            //  referenceValue是有Ref来定义的，这个是由程序员来定义的，readMask是模板值读取掩码，它和referenceValue进行按位与（&）操作作为公式左边的结果，默认值为255，即按位与（&）的结果就是referenceValue本身。
            //  stencilBufferValue是对应位置当前模板缓冲区的值，同样与readMask做按位掩码与操作，结果做为右边的部分。

            //解决double blending,保证一个点只被渲染一次
            Stencil{
                Ref 0  //设定参考值0，stencilbuffer里面的值会跟它进行比较,stencilBuffer值默认为0
                Comp Equal  //比较方式为"相等"
                Pass IncrWrap  //当模版测试和深度测试都通过的时候，当前模板缓冲中的是值+1
                ZFail Keep   //当模板测试通过并且深度测试失败，保存当前模板缓存中的内容不变
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float vPosY : TEXCOORD0;
                float atten: TEXCOORD1;
            };

            float4 _ShadowColor;      //阴影颜色
            float4 _Center;         //平面上一点中心点
            float4 _Normal;            //平面法线

            v2f vert (appdata v)
            {
                v2f o;
                float4 wPos = mul(unity_ObjectToWorld ,v.vertex);  //顶点世界坐标
                float4 lightDir = normalize(_WorldSpaceLightPos0);  //直射光的方向
                float dist = dot(_Center.xyz - wPos.xyz, _Normal.xyz) / dot(lightDir, _Normal.xyz);
                wPos = wPos + lightDir * dist;
                o.vertex = mul( UNITY_MATRIX_VP,wPos);              //转换到裁剪空间坐标
                float3 vt3 = mul( unity_WorldToObject,wPos); 
                o.atten=length(vt3); //_Intensity;//根据前后定点的距离计算衰减
                o.vPosY = wPos.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float isShow = step(0.0f, i.vPosY);
                _ShadowColor.a *= isShow;
                return _ShadowColor;
            }

            ENDCG
        }
    }
}