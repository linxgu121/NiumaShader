#ifndef NIUMA_ARCHITECTURE_SHADOW_CASTER_INCLUDED
#define NIUMA_ARCHITECTURE_SHADOW_CASTER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "NiumaArchitectureMaterial.hlsl"

// URP 在渲染阴影贴图时写入这两个变量，用于计算 Shadow Bias。
float3 _LightDirection;
float3 _LightPosition;

struct NiumaShadowAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct NiumaShadowVaryings
{
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 NiumaGetShadowPositionHClip(NiumaShadowAttributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

#if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    return ApplyShadowClamping(positionCS);
}

NiumaShadowVaryings NiumaArchitectureShadowVertex(NiumaShadowAttributes input)
{
    NiumaShadowVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    output.positionCS = NiumaGetShadowPositionHClip(input);
    return output;
}

half4 NiumaArchitectureShadowFragment(NiumaShadowVaryings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    return 0;
}

#endif
