Shader "Custom/URP_FlockedVelvet"
{
    Properties
    {
        [MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull (Off = two-sided)", Float) = 2

        [Header(Fibres)]
        _FibreMap ("Fibre Map (fibre_map.png)", 2D) = "black" {}
        _FibreTiling ("Fibre Tiling (bigger = finer fibres)", Float) = 6
        _Grain ("Fibre Colour Variation", Range(0,1)) = 0.2
        _ShellLength ("Fibre Length (world units)", Range(0,0.3)) = 0.03
        _Taper ("Fibre Taper (thinner tips)", Range(0,1)) = 0.5
        _RootDarken ("Root Darkening", Range(0,1)) = 0.15
        _TipFrost ("Tip Frosting (lighter tips)", Range(0,1)) = 0.4

        [Header(Soft Velvet Lighting)]
        _Wrap ("Wrap (softens light falloff)", Range(0,1)) = 0.8
        _AmbientStrength ("Ambient Strength", Range(0,1)) = 0.7
        _RimStrength ("Frosty Rim Strength", Range(0,2)) = 0.7
        _RimPower ("Frosty Rim Power (higher = thinner)", Range(0.5,8)) = 2.5

        [Header(Distance Fade (saves performance))]
        _FadeStart ("Fibres start fading at (world units)", Float) = 8
        _FadeEnd ("Fibres gone at (world units)", Float) = 15
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);   SAMPLER(sampler_BaseMap);
        TEXTURE2D(_FibreMap);  SAMPLER(sampler_FibreMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4  _BaseColor;
            half   _FibreTiling;
            half   _Grain;
            half   _ShellLength;
            half   _Taper;
            half   _RootDarken;
            half   _TipFrost;
            half   _Wrap;
            half   _AmbientStrength;
            half   _RimStrength;
            half   _RimPower;
            half   _FadeStart;
            half   _FadeEnd;
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
            half   layer      : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        // layer 0 = the solid body, 0.2 ... 0.8 = fibre shells
        Varyings VertCore(Attributes IN, float layer)
        {
            Varyings OUT = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

            float3 nWS = TransformObjectToWorldNormal(IN.normalOS);
            float3 pWS = TransformObjectToWorld(IN.positionOS.xyz);

            // Fibres fade out with distance: cheaper and no shimmer far away
            // measure from the object pivot so every vertex fades together (per-vertex fading stretches triangles)
            float dist = distance(_WorldSpaceCameraPos, TransformObjectToWorld(float3(0, 0, 0)));
            float fade = saturate((_FadeEnd - dist) / max(_FadeEnd - _FadeStart, 0.001));

            pWS += nWS * (_ShellLength * layer * 1.25 * fade);
            OUT.positionCS = TransformWorldToHClip(pWS);
            if (layer > 0.0 && fade <= 0.0)
                OUT.positionCS = float4(2, 2, 2, 1);   // outside the view: shell costs nothing

            OUT.positionWS = pWS;
            OUT.normalWS   = nWS;
            OUT.uv         = TRANSFORM_TEX(IN.uv, _BaseMap);
            OUT.layer      = layer;
            return OUT;
        }

        half3 Shade(half3 albedo, float3 positionWS, half3 normalWS)
        {
            half3 N = normalize(normalWS);
            half3 V = normalize(GetWorldSpaceViewDir(positionWS));

            Light L = GetMainLight(TransformWorldToShadowCoord(positionWS));
            half wrap = saturate((dot(N, L.direction) + _Wrap) / (1.0h + _Wrap));
            half3 light = L.color * (wrap * L.shadowAttenuation) + SampleSH(N) * _AmbientStrength;

            // Matte body (no specular) + a pale, chalky glow at grazing angles
            half fres = pow(1.0h - saturate(dot(N, V)), _RimPower);
            half3 frost = lerp(albedo, half3(1, 1, 1), 0.5h);

            return albedo * light + frost * (fres * _RimStrength) * light;
        }

        Varyings VertBase(Attributes IN) { return VertCore(IN, 0.0); }
        Varyings VertS1(Attributes IN)   { return VertCore(IN, 0.2); }
        Varyings VertS2(Attributes IN)   { return VertCore(IN, 0.4); }
        Varyings VertS3(Attributes IN)   { return VertCore(IN, 0.6); }
        Varyings VertS4(Attributes IN)   { return VertCore(IN, 0.8); }

        half4 FragBase(Varyings IN) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(IN);
            half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;

            // Fibre tips seen from above: tiny light and dark dots (same pattern as the shells)
            half4 fibre = SAMPLE_TEXTURE2D(_FibreMap, sampler_FibreMap, IN.uv * _FibreTiling);
            albedo *= lerp(1.0h, fibre.b * 2.0h, _Grain);

            return half4(Shade(albedo, IN.positionWS, IN.normalWS), 1.0h);
        }

        half4 FragShell(Varyings IN) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(IN);
            half4 fibre = SAMPLE_TEXTURE2D_LOD(_FibreMap, sampler_FibreMap, IN.uv * _FibreTiling, 0);

            clip(fibre.r - IN.layer);                      // fibre must be tall enough to reach this layer
            clip(1.0h - IN.layer * _Taper - fibre.g);      // and gets thinner toward the tip

            half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
            albedo *= lerp(1.0h, fibre.b * 2.0h, _Grain);

            half3 tip = lerp(albedo, half3(1, 1, 1), 0.5h);
            albedo = lerp(albedo * (1.0h - _RootDarken), tip, IN.layer * _TipFrost);

            return half4(Shade(albedo, IN.positionWS, IN.normalWS), 1.0h);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex VertBase
            #pragma fragment FragBase
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "Fibres1"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex VertS1
            #pragma fragment FragShell
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "Fibres2"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex VertS2
            #pragma fragment FragShell
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "Fibres3"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex VertS3
            #pragma fragment FragShell
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "Fibres4"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex VertS4
            #pragma fragment FragShell
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
