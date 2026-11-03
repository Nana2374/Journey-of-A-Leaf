Shader "Custom/URP_Flocked"
{
    Properties
    {
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1,1,1,1)

        [Header(Fuzz Texture)]
        _FuzzTex ("Fuzz Noise (grayscale, tiling)", 2D) = "gray" {}
        _FuzzTiling ("Fuzz Tiling", Float) = 20
        _FuzzStrength ("Fuzz Strength", Range(0,1)) = 0.35

        [Header(Velvet Sheen)]
        _SheenColor ("Sheen Color", Color) = (1,1,1,1)
        _SheenPower ("Sheen Power (higher = thinner rim)", Range(0.5,8)) = 2.5
        _SheenStrength ("Sheen Strength", Range(0,2)) = 0.8

        [Header(Soft Lighting)]
        _Wrap ("Wrap Amount (softens shadow edge)", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
            TEXTURE2D(_FuzzTex);  SAMPLER(sampler_FuzzTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half4  _SheenColor;
                half   _FuzzTiling;
                half   _FuzzStrength;
                half   _SheenPower;
                half   _SheenStrength;
                half   _Wrap;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                half3  normalWS   : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                VertexPositionInputs p = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = p.positionCS;
                OUT.positionWS = p.positionWS;
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                half3 N = normalize(IN.normalWS);
                half3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));

                // Albedo + fuzz breakup (one extra texture sample)
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
                half fuzz = SAMPLE_TEXTURE2D(_FuzzTex, sampler_FuzzTex, IN.uv * _FuzzTiling).r;
                albedo *= lerp(1.0h, fuzz * 2.0h, _FuzzStrength);

                // Main light with soft wrapped diffuse
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half ndl  = dot(N, mainLight.direction);
                half wrap = saturate((ndl + _Wrap) / (1.0h + _Wrap));
                half3 lightColor = mainLight.color * (wrap * mainLight.shadowAttenuation);

                // Ambient from spherical harmonics (cheap)
                half3 ambient = SampleSH(N);

                half3 lit = albedo * (lightColor + ambient);

                // Velvet sheen: bright, slightly dusty rim, broken up by fuzz
                half fres = pow(1.0h - saturate(dot(N, V)), _SheenPower);
                half3 sheen = fres * _SheenStrength * _SheenColor.rgb * (0.5h + fuzz)
                              * (lightColor + ambient);

                return half4(lit + sheen, 1.0h);
            }
            ENDHLSL
        }

        // Reuse URP's shadow caster and depth passes so the object still casts shadows
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
