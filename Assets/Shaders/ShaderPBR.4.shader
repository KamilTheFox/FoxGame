Shader "Custom/ShaderPBR"
{
    Properties
    {
        _MainTex("Mask (R=CustomMetal, B=Metal, G=Eyes, Black=Outline)", 2D) = "white" {}

        [Header(Custom Metal)]
        _CustomMetalColor("Custom Metal Color", Color) = (1,0,0,1)
        _CustomMetallic("Custom Metal Metallic", Range(0,1)) = 0.8
        _CustomSmoothness("Custom Metal Smoothness", Range(0,1)) = 0.7

        [Header(Base Metal)]
        _MetalColor("Metal Color", Color) = (0.7,0.7,0.7,1)
        _MetalMetallic("Metal Metallic", Range(0,1)) = 1.0
        _MetalSmoothness("Metal Smoothness", Range(0,1)) = 0.8

        [Header(Eyes)]
        _EyeColor("Eye Color", Color) = (0,1,0,1)
        [HDR] _EyeEmission("Eye Emission", Color) = (0,2,0,1)

        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0.1,0.1,0.1,1)
        _OutlineMetallic("Outline Metallic", Range(0,1)) = 0.5
        _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags {"RenderType" = "Opaque"}

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;

        float4 _CustomMetalColor, _MetalColor, _EyeColor, _OutlineColor;
        float4 _EyeEmission;
        float _CustomMetallic, _CustomSmoothness;
        float _MetalMetallic, _MetalSmoothness;
        float _OutlineMetallic, _OutlineSmoothness;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 mask = tex2D(_MainTex, IN.uv_MainTex);

            // ќпредел€ем область контура (где все каналы близки к 0)
            float outlineMask = 1 - max(max(mask.r, mask.g), mask.b);

            // Ѕазовый цвет (albedo)
            o.Albedo = mask.r * _CustomMetalColor +
                        mask.b * _MetalColor +
                        mask.g * _EyeColor +
                        outlineMask * _OutlineColor;

            // ћеталличность
            o.Metallic = mask.r * _CustomMetallic +
                        mask.b * _MetalMetallic +
                        outlineMask * _OutlineMetallic;

            // √ладкость
            o.Smoothness = mask.r * _CustomSmoothness +
                            mask.b * _MetalSmoothness +
                            outlineMask * _OutlineSmoothness;

            // Ёмисси€ только дл€ глаз
            o.Emission = mask.g * _EyeEmission;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
