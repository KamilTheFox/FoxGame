Shader "Custom/MetallShaderColor"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _MetallicMap ("Metallic Map", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _ReplacementColor ("Replacement Color", Color) = (1,0,0,1)
        _ColorThreshold ("Green Color Threshold", Range(0,1)) = 0.1
        [Toggle] _InvertMetallic ("Invert Metallic", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
        }
        LOD 200
        Cull Off

        // Shadow caster pass остается без изменений...

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alphatest:_Cutoff
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _MetallicMap;
        float4 _ReplacementColor;
        float _ColorThreshold;
        float _InvertMetallic;
        float _Metallic;
        float _Smoothness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float2 uv_MetallicMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            fixed3 normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            fixed metallicMap = tex2D(_MetallicMap, IN.uv_MetallicMap).r;

            metallicMap = lerp(metallicMap, 1 - metallicMap, _InvertMetallic);

            float greenDominance = c.g - max(c.r, c.b);
            if(greenDominance > _ColorThreshold)
            {
                c.rgb = _ReplacementColor.rgb;
            }

            o.Albedo = c.rgb;
            o.Normal = normal;
            o.Metallic = metallicMap * _Metallic;
            o.Smoothness = metallicMap * _Smoothness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/Diffuse"
}