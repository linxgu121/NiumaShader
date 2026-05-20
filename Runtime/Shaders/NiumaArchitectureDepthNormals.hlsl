#ifndef NIUMA_ARCHITECTURE_DEPTH_NORMALS_INCLUDED
#define NIUMA_ARCHITECTURE_DEPTH_NORMALS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "NiumaArchitectureMaterial.hlsl"

struct NiumaDepthNormalsAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct NiumaDepthNormalsVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    half3 normalWS : TEXCOORD1;
#if defined(_NIUMA_NORMALMAP)
    half3 tangentWS : TEXCOORD2;
    half3 bitangentWS : TEXCOORD3;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

NiumaDepthNormalsVaryings NiumaArchitectureDepthNormalsVertex(NiumaDepthNormalsAttributes input)
{
    NiumaDepthNormalsVaryings output = (NiumaDepthNormalsVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
#if defined(_NIUMA_NORMALMAP)
    output.tangentWS = normalInput.tangentWS;
    output.bitangentWS = normalInput.bitangentWS;
#endif
    return output;
}

half3 NiumaGetDepthNormalWS(NiumaDepthNormalsVaryings input)
{
#if defined(_NIUMA_NORMALMAP)
    half4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv);
    half3 normalTS = UnpackNormalScale(normalSample, _NormalScale);
    half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
    return NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, tangentToWorld));
#else
    return NormalizeNormalPerPixel(input.normalWS);
#endif
}

void NiumaArchitectureDepthNormalsFragment(
    NiumaDepthNormalsVaryings input,
    out half4 outNormalWS : SV_Target0)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half3 normalWS = NiumaGetDepthNormalWS(input);

#if defined(_GBUFFER_NORMALS_OCT)
    float2 octNormalWS = PackNormalOctQuadEncode(normalize(normalWS));
    float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
    half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
    outNormalWS = half4(packedNormalWS, 0.0);
#else
    outNormalWS = half4(normalWS, 0.0);
#endif
}

#endif
