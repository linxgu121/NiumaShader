#ifndef NIUMA_ARCHITECTURE_PARALLAX_INCLUDED
#define NIUMA_ARCHITECTURE_PARALLAX_INCLUDED

float2 NiumaApplyArchitectureParallax(
    float2 uv,
    half3 viewDirectionWS,
    half3 tangentWS,
    half3 bitangentWS,
    half3 normalWS,
    out half sampledHeight)
{
    sampledHeight = 0.0;

#if defined(_NIUMA_PARALLAX)
    float2 heightUV = uv * _HeightMap_ST.xy + _HeightMap_ST.zw;
    sampledHeight = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, heightUV).r;

    half3 viewDirectionTS = half3(
        dot(viewDirectionWS, tangentWS),
        dot(viewDirectionWS, bitangentWS),
        dot(viewDirectionWS, normalWS));
    viewDirectionTS = SafeNormalize(viewDirectionTS);

    half centeredHeight = sampledHeight - _ParallaxCenter;
    half viewZ = max(abs(viewDirectionTS.z), 0.25);
    float2 parallaxOffset = viewDirectionTS.xy * (centeredHeight * _ParallaxStrength / viewZ);

    // 2.0-B 使用单步 Offset Mapping，只移动采样 UV，不改变几何轮廓和深度写入。
    return uv + parallaxOffset;
#else
    return uv;
#endif
}

#endif
