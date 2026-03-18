Shader "Custom/PlayerJersey"
{
    Properties
    {
        // 衣服
        _MainTex ("衣服", 2D) = "white" { }
        // 图案
        _Pattern ("图案", 2D) = "white" { }
        // 队徽
        _Icon ("队徽", 2D) = "white" { }
        // 数字
        _Number ("数字", 2D) = "white" { }
        // 衣服颜色
        _ClothesColor ("衣服颜色", Color) = (1, 1, 1, 1)
        // 图案颜色
        _PatternColor ("图案颜色", Color) = (1, 1, 1, 1)
        // 数字颜色
        _NumberColor ("数字颜色", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
        };
        
        sampler2D _MainTex;
        sampler2D _Pattern;
        sampler2D _Icon;
        sampler2D _Number;

        fixed4 _ClothesColor;
        fixed4 _PatternColor;
        fixed4 _NumberColor;
        
        float4 _Icon_ST;
        float4 _Number_ST;


        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // 衣服
            fixed4 clothes = tex2D(_MainTex, IN.uv_MainTex) * _ClothesColor;
            // 图案
            fixed4 pattern = tex2D(_Pattern, IN.uv_MainTex);
            // 队徽
            fixed4 icon = tex2D(_Icon, IN.uv_MainTex * _Icon_ST.xy + _Icon_ST.zw);
            // 数字
            fixed4 number = tex2D(_Number, IN.uv_MainTex * _Number_ST.xy + _Number_ST.zw);
            // 添加图案
            o.Albedo = clothes.rgb * (1 - pattern.a) + _PatternColor * pattern.a;
            // 添加队徽
            o.Albedo = o.Albedo.rgb * (1 - icon.a) + icon.rgb * icon.a;
            // 添加数字
            o.Albedo = o.Albedo.rgb * (1 - number.a) + _NumberColor * number.a;
        }
        ENDCG

    }
    FallBack "Diffuse"
}
