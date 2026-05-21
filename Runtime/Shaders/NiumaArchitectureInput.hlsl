#ifndef NIUMA_ARCHITECTURE_INPUT_INCLUDED
#define NIUMA_ARCHITECTURE_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "NiumaArchitectureMaterial.hlsl"
#include "NiumaArchitectureLighting.hlsl"
#include "NiumaArchitectureParallax.hlsl"
#include "NiumaArchitectureDetail.hlsl"
#include "NiumaArchitectureWeathering.hlsl"
#include "NiumaArchitectureDebug.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
#if defined(_NIUMA_NORMALMAP) || defined(_NIUMA_PARALLAX) || defined(_NIUMA_ANISOTROPY)
    half3 tangentWS : TEXCOORD3;
    half3 bitangentWS : TEXCOORD4;
#endif
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
    float4 shadowCoord : TEXCOORD6;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings NiumaArchitectureVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs positionInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.positionCS = positionInput.positionCS;
    output.positionWS = positionInput.positionWS;
    output.uv = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
#if defined(_NIUMA_NORMALMAP) || defined(_NIUMA_PARALLAX) || defined(_NIUMA_ANISOTROPY)
    output.tangentWS = normalInput.tangentWS;
    output.bitangentWS = normalInput.bitangentWS;
#endif
    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    OUTPUT_SH(output.normalWS, output.vertexSH);
    output.shadowCoord = TransformWorldToShadowCoord(positionInput.positionWS);
    output.color = input.color;
    return output;
}

float2 NiumaGetMaterialUV(Varyings input, half3 viewDirectionWS, out half parallaxHeight)
{
#if defined(_NIUMA_PARALLAX)
    return NiumaApplyArchitectureParallax(
        input.uv,
        viewDirectionWS,
        input.tangentWS,
        input.bitangentWS,
        input.normalWS,
        parallaxHeight);
#else
    parallaxHeight = 0.0;
    return input.uv;
#endif
}

half3 NiumaGetNormalWS(Varyings input, float2 materialUV)
{
#if defined(_NIUMA_NORMALMAP)
    half4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, materialUV);
    half3 normalTS = UnpackNormalScale(normalSample, _NormalScale);
    half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
    return NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, tangentToWorld));
#else
    return NormalizeNormalPerPixel(input.normalWS);
#endif
}

NiumaArchitectureSurfaceData NiumaBuildSurfaceData(Varyings input, float2 materialUV, half3 normalWS, half parallaxHeight)
{
    half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, materialUV);
    half4 maskSample = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, materialUV);

    NiumaArchitectureSurfaceData surface;
    surface.baseAlbedo = baseSample.rgb * _BaseColor.rgb;
    surface.albedo = surface.baseAlbedo;
    surface.alpha = baseSample.a * _BaseColor.a;
    surface.normalWS = normalWS;
#if defined(_NIUMA_ANISOTROPY)
    surface.tangentWS = NormalizeNormalPerPixel(input.tangentWS);
    surface.bitangentWS = NormalizeNormalPerPixel(input.bitangentWS);
#endif
    surface.occlusion = lerp(1.0, saturate(maskSample.r), saturate(_OcclusionStrength));
    surface.smoothness = saturate(maskSample.g * _Smoothness);
    surface.detailMask = 0.0;
    surface.parallaxHeight = parallaxHeight;
    surface.edgeWear = saturate(maskSample.b);
    surface.dirtMask = 0.0;
    surface.mossMask = 0.0;
    surface.paintFadeMask = 0.0;
    surface.rainMask = 0.0;

    float2 detailUV = materialUV * _DetailMap_ST.xy + _DetailMap_ST.zw;
    NiumaApplyArchitectureDetail(surface, detailUV);

    float2 weatherUV = materialUV * _WeatherMap_ST.xy + _WeatherMap_ST.zw;
    NiumaApplyArchitectureWeathering(surface, weatherUV, input.color);
    return surface;
}

half4 NiumaArchitectureFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half parallaxHeight;
    float2 materialUV = NiumaGetMaterialUV(input, viewDirectionWS, parallaxHeight);
    half3 normalWS = NiumaGetNormalWS(input, materialUV);
    NiumaArchitectureSurfaceData surface = NiumaBuildSurfaceData(input, materialUV, normalWS, parallaxHeight);

    half4 debugColor;
    if (NiumaTryGetDebugColor(_DebugView, surface, input.color, debugColor))
    {
        return debugColor;
    }

    half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, surface.normalWS);
    half3 litColor = NiumaApplyArchitectureLighting(surface, viewDirectionWS, input.shadowCoord, bakedGI);
    return half4(litColor, surface.alpha);
}

#endif
