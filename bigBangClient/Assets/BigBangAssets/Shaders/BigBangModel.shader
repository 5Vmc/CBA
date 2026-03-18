
Shader "bigbang/BigBangModel"
{
    Properties
    {
        [Header(Outline)]
        _slider ("Slider", Range(0, 1)) = 0.7350425
        _main_tex ("Main Texture", 2D) = "Black" { }
        _outline_color ("Outline Color", Color) = (0.5, 0.5, 0.5, 1)

        [Header(Light)]
        _light_tex ("Light Texture", 2D) = "Black" { }
        _light_rate ("Light Rate", Float) = 1
        _light_tex_u_speed ("Light Texture U Speed", Float) = 0
        _light_tex_v_speed ("Light Texture V Speed", Float) = 0

        [Header(Dissolve)]
        _dissolve_filter ("Dissolve Filter", Range(0, 1)) = 0
        _dissolve_tex ("Dissolve Texture", 2D) = "White" { }
        _dissolve_outline_in_size ("Dissolve Outline In Size", Range(0, 0.2)) = 0.05
        _dissolve_outline_out_size ("Dissolve Outline Out Size", Range(0, 0.2)) = 0.05
        _dissolve_outline_color ("Dissolve Outline Color", Color) = (0.5, 0.5, 0.5, 1)
    }
    SubShader
    {
        Tags { "IgnoreProjector" = "True" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha One
            // Blend One One
            ////Cull Off
            ////ZWrite Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float _slider;
            uniform sampler2D _main_tex; uniform float4 _main_tex_ST;
            uniform float4 _outline_color;

            uniform sampler2D _light_tex; uniform float4 _light_tex_ST;
            uniform float _light_rate;
            uniform float _light_tex_u_speed;
            uniform float _light_tex_v_speed;

            uniform float _dissolve_filter;
            uniform sampler2D _dissolve_tex; uniform float4 _dissolve_tex_ST;
            uniform float _dissolve_outline_in_size;
            uniform float _dissolve_outline_out_size;
            uniform float4 _dissolve_outline_color;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex_color : COLOR;
            };
            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 pos_world : TEXCOORD2;
                float3 normal : TEXCOORD3;
                float4 vertex_color : COLOR;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.uv0;
                o.uv1 = v.uv1;
                o.vertex_color = v.vertex_color;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.pos_world = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(VertexOutput i, float facing : VFACE) : COLOR
            {
                // 溶解判断
                fixed4 dissolve_tex_color = tex2D(_dissolve_tex, TRANSFORM_TEX(i.uv1, _dissolve_tex));
                if (dissolve_tex_color.r < _dissolve_filter)
                {
                    fixed diff = _dissolve_filter - dissolve_tex_color.r;
                    if (diff <= _dissolve_outline_out_size)
                    {
                        return fixed4(_dissolve_outline_color.rgb, ((_dissolve_outline_out_size - diff) * (1 / _dissolve_outline_out_size)));
                    }
                    return fixed4(0, 0, 0, 0);
                }

                // 贴图颜色
                fixed4 main_tex_color = tex2D(_main_tex, TRANSFORM_TEX(i.uv0, _main_tex));

                // 扫光颜色
                float time = _Time.g;
                float2 light_uv = float2(_light_tex_u_speed * time, _light_tex_v_speed * time) + i.uv1;
                float4 light_tex_color = tex2D(_light_tex, TRANSFORM_TEX(light_uv, _light_tex));
                float light_effect = light_tex_color.r + light_tex_color.g + light_tex_color.b;

                // 扫光影响贴图
                fixed3 light_color = main_tex_color.rgb * i.vertex_color.rgb * i.vertex_color.a * light_effect * 100 * _light_rate + main_tex_color.rgb;


                // 外发光
                fixed3 view_dir = normalize(_WorldSpaceCameraPos.xyz - i.pos_world.xyz);
                fixed3 normal_dir = i.normal;
                fixed3 fianl_color = light_color * _outline_color.rgb + pow(1 - max(0, dot(normal_dir, view_dir)), exp(abs(sin(_Time.y)) * _slider)) * _outline_color.rgb * _outline_color.a;

                // 溶解叠加
                if (dissolve_tex_color.r < _dissolve_filter + _dissolve_outline_in_size)
                {
                    fixed diff = dissolve_tex_color.r - _dissolve_filter;
                    return fixed4(fianl_color.rgb + _outline_color.rgb * ((_dissolve_outline_in_size - diff) * (1 / _dissolve_outline_in_size)), _outline_color.a * main_tex_color.a * i.vertex_color.a);
                }

                // 显示原有颜色
                return fixed4(fianl_color, _outline_color.a * main_tex_color.a * i.vertex_color.a);
            }
            ENDCG

        }
    }
    FallBack "Diffuse"
}
