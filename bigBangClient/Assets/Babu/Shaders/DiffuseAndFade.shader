Shader "Custom/DiffuseAndFade" {
    Properties{
        _Color("Main Color", Color) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" { }
        _ColorTint("Tint", Color) = (0.7, 0.7, 0.7, 1.0)
        _FadeDistanceNear("Near fadeout dist (View Space)", float) = 3
        _FadeDistanceFar("Far fadeout dist (View Space)", float) = 7
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade vertex:vert

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _ColorTint;
        float _FadeDistanceNear;
        float _FadeDistanceFar;
        struct Input {
            float2 uv_MainTex;
            float3 customColor;
            //float fade;
        };

        void vert(inout appdata_full v, out Input o){
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float2 posView = UnityObjectToClipPos(v.vertex).yz;
            float dis = length(posView);
            o.customColor = _ColorTint.rgb * saturate((dis - _FadeDistanceNear)/ (_FadeDistanceFar - _FadeDistanceNear));
            //o.fade = 1 - saturate((dis - _FadeDistanceNear) / (_FadeDistanceFar - _FadeDistanceNear));
        }

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Albedo += IN.customColor;
            o.Alpha = c.a;
            //o.Alpha = IN.fade;
        }
        ENDCG
    }

    Fallback "Legacy Shaders/Transparent/Diffuse"
}
