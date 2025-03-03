Shader "Custom/MaskBasedMaterial"
{
    Properties
    {
        // Маска
    _MaskMap("Mask Map", 2D) = "white" {}

    // Настройки отражений
    [Toggle(_USECUSTOMREFLECTION_ON)] _UseCustomReflection("Use Custom Reflection", Float) = 0
    [NoScaleOffset] _ReflectionCube("Custom Reflection", CUBE) = "black" {}
    _ReflectionIntensity("Reflection Intensity", Range(0, 2)) = 1
    _FresnelPower("Fresnel Power", Range(0, 10)) = 1

        [Header(______________RED)]
        // Красный канал
        [Toggle] _UseRedTexture("Use Red Texture", Float) = 1
        _RedAlbedo("Red Albedo", 2D) = "white" {}
        _RedColor("Red Color", Color) = (1,1,1,1)
        [NoScaleOffset] _RedNormal("Red Normal", 2D) = "bump" {}
        _RedMetallic("Red Metallic", Range(0,1)) = 0
        _RedSmoothness("Red Smoothness", Range(0,1)) = 0.5
        _RedTiling("Red Tiling", Vector) = (1,1,0,0)
        _RedReflectionIntensity("Red Reflection Intensity", Range(0, 2)) = 1
        //[Header(Red Parallax)]
        //[Toggle(_USEPARALAXRED)] _UseParallaxRed("Use Parallax Red", Float) = 0
        //_RedParallaxMap("Red Parallax Map", 2D) = "white" {}
        //_ParallaxStrengthRed("Parallax Strength Red", Range(0, 0.1)) = 0.02

        [Header(Red Emission)]
        [Toggle] _UseRedEmission("Use Red Emission", Float) = 0
        [HDR] _RedEmission("Red Emission", Color) = (0,0,0,1)
        _RedEmissionMap("Red Emission Map", 2D) = "white" {}

        [Header(______________GREEN)]
        // Зеленый канал
        [Toggle] _UseGreenTexture("Use Green Texture", Float) = 1
        _GreenAlbedo("Green Albedo", 2D) = "white" {}
        _GreenColor("Green Color", Color) = (1,1,1,1)
        [NoScaleOffset] _GreenNormal("Green Normal", 2D) = "bump" {}
        _GreenMetallic("Green Metallic", Range(0,1)) = 0
        _GreenSmoothness("Green Smoothness", Range(0,1)) = 0.5
        _GreenTiling("Green Tiling", Vector) = (1,1,0,0)
        _GreenReflectionIntensity("Green Reflection Intensity", Range(0, 2)) = 1

        [Header(Green Emission)]
        [Toggle] _UseGreenEmission("Use Green Emission", Float) = 0
        [HDR] _GreenEmission("Green Emission", Color) = (0,0,0,1)
        _GreenEmissionMap("Green Emission Map", 2D) = "white" {}

        [Header(______________BLUE)]
        // Синий канал
        [Toggle] _UseBlueTexture("Use Blue Texture", Float) = 1
        _BlueAlbedo("Blue Albedo", 2D) = "white" {}
        _BlueColor("Blue Color", Color) = (1,1,1,1)
        [NoScaleOffset] _BlueNormal("Blue Normal", 2D) = "bump" {}
        _BlueMetallic("Blue Metallic", Range(0,1)) = 0
        _BlueSmoothness("Blue Smoothness", Range(0,1)) = 0.5
        _BlueTiling("Blue Tiling", Vector) = (1,1,0,0)
        _BlueReflectionIntensity("Blue Reflection Intensity", Range(0, 2)) = 1

        [Header(Blue Emission)]
        [Toggle] _UseBlueEmission("Use Blue Emission", Float) = 0
        [HDR] _BlueEmission("Blue Emission", Color) = (0,0,0,1)
        _BlueEmissionMap("Blue Emission Map", 2D) = "white" {}

        [Header(______________BLACK)]
        // Черный канал
        [Toggle] _UseBlackTexture("Use Black Texture", Float) = 1
        _BlackAlbedo("Black Albedo", 2D) = "white" {}
        _BlackColor("Black Color", Color) = (1,1,1,1)
        [NoScaleOffset] _BlackNormal("Black Normal", 2D) = "bump" {}
        _BlackMetallic("Black Metallic", Range(0,1)) = 0
        _BlackSmoothness("Black Smoothness", Range(0,1)) = 0.5
        _BlackTiling("Black Tiling", Vector) = (1,1,0,0)
        _BlackReflectionIntensity("Black Reflection Intensity", Range(0, 2)) = 1

        [Header(Black Emission)]
        [Toggle] _UseBlackEmission("Use Black Emission", Float) = 0
        [HDR] _BlackEmission("Black Emission", Color) = (0,0,0,1)
        _BlackEmissionMap("Black Emission Map", 2D) = "white" {}

    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Tags {"DisableBatching" = "True"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma shader_feature_local _USECUSTOMREFLECTION_ON

        sampler2D _MaskMap;
        UNITY_DECLARE_TEXCUBE(_ReflectionCube); // Здесь объявляем кубмап
        float _ReflectionIntensity;
        float _FresnelPower;

        // Красный канал
        sampler2D _RedAlbedo;
        float4 _RedColor;
        float _UseRedTexture;
        sampler2D _RedNormal;

        sampler2D _RedParallaxMap;
        float _ParallaxStrengthRed;

        float _RedMetallic;
        float _RedSmoothness;
        float4 _RedTiling;
        half _RedReflectionIntensity;

        

        // Зеленый канал
        sampler2D _GreenAlbedo;
        float4 _GreenColor;
        float _UseGreenTexture;
        sampler2D _GreenNormal;
        float _GreenMetallic;
        float _GreenSmoothness;
        float4 _GreenTiling;
        float _GreenReflectionIntensity;

        // Синий канал
        sampler2D _BlueAlbedo;
        float4 _BlueColor;
        float _UseBlueTexture;
        sampler2D _BlueNormal;
        float _BlueMetallic;
        float _BlueSmoothness;
        float4 _BlueTiling;
        float _BlueReflectionIntensity;

        // Черный канал
        sampler2D _BlackAlbedo;
        float4 _BlackColor;
        float _UseBlackTexture;
        sampler2D _BlackNormal;
        float _BlackMetallic;
        float _BlackSmoothness;
        float4 _BlackTiling;
        float _BlackReflectionIntensity;

        // Красный канал эмиссии
        sampler2D _RedEmissionMap;
        float4 _RedEmission;

        // Зеленый канал эмиссии
        sampler2D _GreenEmissionMap;
        float4 _GreenEmission;

        // Синий канал эмиссии
        sampler2D _BlueEmissionMap;
        float4 _BlueEmission;

        // Черный канал эмиссии
        sampler2D _BlackEmissionMap;
        float4 _BlackEmission;

        float _UseRedEmission;
        float _UseGreenEmission;
        float _UseBlueEmission;
        float _UseBlackEmission;

        


        struct Input
        {
            float2 uv_RedAlbedo;
            float2 uv2_MaskMap;
            float3 worldRefl;
            float3 viewDir;
            INTERNAL_DATA
        };

        float4 GetChannelColor(sampler2D tex, float2 uv, float4 solidColor, float useTexture)
        {
            return lerp(solidColor, tex2D(tex, uv), useTexture);
        }

        float FresnelEffect(float3 Normal, float3 ViewDir, float Power)
        {
            return pow(1.0 - saturate(dot(normalize(Normal), normalize(ViewDir))), Power);
        }

        float3 CalculateChannelEmission(float useEmission, sampler2D emissionMap, float2 uv, float4 emissionColor)
        {
            return useEmission * tex2D(emissionMap, uv).rgb * emissionColor.rgb;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 mask = tex2D(_MaskMap, IN.uv2_MaskMap);
            float2 baseUV = IN.uv_RedAlbedo;

            float blackWeight = 1 - (mask.r + mask.g + mask.b);
            blackWeight = max(0, blackWeight);

            // Сначала вычисляем базовые UV с тайлингом
            float2 uvRed = baseUV * _RedTiling.xy + _RedTiling.zw;
            float2 uvGreen = baseUV * _GreenTiling.xy + _GreenTiling.zw;
            float2 uvBlue = baseUV * _BlueTiling.xy + _BlueTiling.zw;
            float2 uvBlack = baseUV * _BlackTiling.xy + _BlackTiling.zw;
            /* 
            // Теперь вычисляем параллакс

            #if defined(_USEPARALAXRED)
                float heightR = tex2D(_RedParallaxMap, uvRed).r;
                float2 offsetR = ParallaxOffset(heightR, _ParallaxStrengthRed, IN.viewDir) * mask.r;
            uvRed += offsetR;
            #endif
            #ifdef _PARALLAX_GREEN
                float heightG = tex2D(_HeightMapGreen, uvGreen).r;
                float2 offsetG = ParallaxOffset(heightG, _ParallaxStrengthGreen, normalize(IN.viewDir)) * mask.g;
                uvGreen += offsetG;
            #endif
            #ifdef _PARALLAX_BLUE
                float heightB = tex2D(_HeightMapBlue, uvBlue).r;
                float2 offsetB = ParallaxOffset(heightB, _ParallaxStrengthBlue, normalize(IN.viewDir)) * mask.b;
                uvBlue += offsetB;
            #endif
            #ifdef _PARALLAX_BLACK
                float heightA = tex2D(_HeightMapBlack, uvBlack).r;
                float2 offsetA = ParallaxOffset(heightA, _ParallaxStrengthBlack, normalize(IN.viewDir)) * blackWeight;
                uvBlack += offsetA;
            #endif*/


            float4 redAlbedo = GetChannelColor(_RedAlbedo, uvRed, _RedColor, _UseRedTexture);
            float4 greenAlbedo = GetChannelColor(_GreenAlbedo, uvGreen, _GreenColor, _UseGreenTexture);
            float4 blueAlbedo = GetChannelColor(_BlueAlbedo, uvBlue, _BlueColor, _UseBlueTexture);
            float4 blackAlbedo = GetChannelColor(_BlackAlbedo, uvBlack, _BlackColor, _UseBlackTexture);

            float3 redNormal = UnpackNormal(tex2D(_RedNormal, uvRed));
            float3 greenNormal = UnpackNormal(tex2D(_GreenNormal, uvGreen));
            float3 blueNormal = UnpackNormal(tex2D(_BlueNormal, uvBlue));
            float3 blackNormal = UnpackNormal(tex2D(_BlackNormal, uvBlack));


            float4 finalAlbedo = mask.r * redAlbedo +
                                mask.g * greenAlbedo +
                                mask.b * blueAlbedo +
                                blackWeight * blackAlbedo;

            float3 finalNormal = normalize(mask.r * redNormal +
                                            mask.g * greenNormal +
                                            mask.b * blueNormal +
                                            blackWeight * blackNormal);

            float finalMetallic = mask.r * _RedMetallic +
                                    mask.g * _GreenMetallic +
                                    mask.b * _BlueMetallic +
                                    blackWeight * _BlackMetallic;

            float finalSmoothness = mask.r * _RedSmoothness +
                                    mask.g * _GreenSmoothness +
                                    mask.b * _BlueSmoothness +
                                    blackWeight * _BlackSmoothness;

            float3 worldRefl = WorldReflectionVector(IN, finalNormal);
            float fresnel = FresnelEffect(finalNormal, IN.viewDir, _FresnelPower);

            float finalReflectionIntensity = mask.r * _RedReflectionIntensity +
                                            mask.g * _GreenReflectionIntensity +
                                            mask.b * _BlueReflectionIntensity +
                                            blackWeight * _BlackReflectionIntensity;

            #if defined(_USECUSTOMREFLECTION_ON)
                float3 reflection = UNITY_SAMPLE_TEXCUBE(_ReflectionCube, worldRefl).rgb;
            #else
                float3 reflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl).rgb;
            #endif

            float3 finalReflection = reflection * finalReflectionIntensity * _ReflectionIntensity * fresnel * finalMetallic;

            float3 redEmission = CalculateChannelEmission(_UseRedEmission, _RedEmissionMap, uvRed, _RedEmission);
            float3 greenEmission = CalculateChannelEmission(_UseGreenEmission, _GreenEmissionMap, uvGreen, _GreenEmission);
            float3 blueEmission = CalculateChannelEmission(_UseBlueEmission, _BlueEmissionMap, uvBlue, _BlueEmission);
            float3 blackEmission = CalculateChannelEmission(_UseBlackEmission, _BlackEmissionMap, uvBlack, _BlackEmission);

            // Смешиваем эмиссию по маске
            float3 finalCustomEmission = mask.r * redEmission +
                mask.g * greenEmission +
                mask.b * blueEmission +
                blackWeight * blackEmission;

            // Добавляем к существующей эмиссии от отражений

            o.Emission = finalReflection + finalCustomEmission;

            o.Albedo = finalAlbedo.rgb;
            o.Normal = finalNormal;
            o.Metallic = finalMetallic;
            o.Smoothness = finalSmoothness;
            o.Alpha = finalAlbedo.a;
        }
        ENDCG
    }
        FallBack "Diffuse"
}
