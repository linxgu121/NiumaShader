#ifndef NIUMA_ARCHITECTURE_DETAIL_INCLUDED
#define NIUMA_ARCHITECTURE_DETAIL_INCLUDED

void NiumaApplyArchitectureDetail(inout NiumaArchitectureSurfaceData surface, float2 detailUV)
{
#if defined(_NIUMA_DETAILMAP)
    half4 detail = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, detailUV);
    half detailMask = saturate(detail.a * _DetailStrength);
    half3 detailMultiplier = lerp(half3(1.0, 1.0, 1.0), detail.rgb * 2.0, detailMask);

    // DetailMap 只负责近景微观颜色变化，不承担灰尘、苔痕等旧化语义。
    surface.detailMask = detailMask;
    surface.albedo = saturate(surface.albedo * detailMultiplier);
#else
    surface.detailMask = 0.0;
#endif
}

#endif
