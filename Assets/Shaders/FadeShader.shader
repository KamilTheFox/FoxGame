Shader "Custom/StandardOpaqueFadeSwitch"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
        _ParallaxMap("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0

        [Toggle] _IsTransparent("Is Transparent", Float) = 0
    }

        SubShader
        {
            Tags {"RenderType" = "Opaque" "PerformanceChecks" = "False"}
            LOD 300

            CGPROGRAM
            #pragma target 3.0
            #pragma surface surf Standard fullforwardshadows keepalpha

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _DETAIL_MULX2
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _PARALLAXMAP

            #include "UnityStandardUtils.cginc"

            sampler2D _MainTex;
            sampler2D _DetailAlbedoMap;
            sampler2D _BumpMap;
            sampler2D _DetailMask;
            sampler2D _DetailNormalMap;
            sampler2D _MetallicGlossMap;
            sampler2D _ParallaxMap;
            sampler2D _OcclusionMap;
            sampler2D _EmissionMap;

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_DetailAlbedoMap;
                float3 viewDir;
                INTERNAL_DATA
            };

            fixed4 _Color;
            fixed _Cutoff;
            half _Glossiness;
            half _Metallic;
            fixed _BumpScale;
            fixed _DetailNormalMapScale;
            half _Parallax;
            half _OcclusionStrength;
            fixed4 _EmissionColor;
            float _UVSec;
            float _IsTransparent;

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                float2 uv = IN.uv_MainTex;

                #if defined(_PARALLAXMAP)
                    half h = tex2D(_ParallaxMap, uv).g;
                    float2 offset = ParallaxOffset1Step(h, _Parallax, IN.viewDir);
                    uv += offset;
                #endif

                    // Apply gamma correction
                    fixed4 c = tex2D(_MainTex, uv);
                    c.rgb = GammaToLinearSpace(c.rgb);
                    c *= _Color;
                    c.rgb = LinearToGammaSpace(c.rgb);
                    o.Albedo = c.rgb;

                    #if defined(_DETAIL_MULX2)
                        fixed4 detailAlbedo = tex2D(_DetailAlbedoMap, IN.uv_DetailAlbedoMap);
                        o.Albedo *= LerpWhiteTo(detailAlbedo.rgb, tex2D(_DetailMask, uv).a);
                    #endif

                    #if defined(_METALLICGLOSSMAP)
                        fixed4 mg = tex2D(_MetallicGlossMap, uv);
                        o.Metallic = mg.r;
                        o.Smoothness = mg.a;
                    #else
                        o.Metallic = _Metallic;
                        o.Smoothness = _Glossiness;
                    #endif

                    #if defined(_NORMALMAP)
                        fixed3 normalTangent = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
                        #if defined(_DETAIL_MULX2)
                            fixed3 detailNormalTangent = UnpackScaleNormal(tex2D(_DetailNormalMap, IN.uv_DetailAlbedoMap), _DetailNormalMapScale);
                            normalTangent = BlendNormals(normalTangent, detailNormalTangent);
                        #endif
                        o.Normal = normalTangent;
                    #endif

                    fixed occ = tex2D(_OcclusionMap, uv).g;
                    o.Occlusion = LerpOneTo(occ, _OcclusionStrength);

                    #if defined(_EMISSION)
                        o.Emission = tex2D(_EmissionMap, uv).rgb * _EmissionColor;
                    #endif

                    o.Alpha = lerp(1.0, c.a, _IsTransparent);
                }
                ENDCG
        }
        FallBack "Diffuse"
        CustomEditor "StandardShaderGUI"
}