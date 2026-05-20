#ifndef NIUMA_ARCHITECTURE_MATERIAL_INCLUDED
#define NIUMA_ARCHITECTURE_MATERIAL_INCLUDED

// 所有 Pass 共享同一份材质常量布局，避免 Forward / Shadow / Depth Pass 的参数声明漂移。
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _WeatherMap_ST;
    half4 _BaseColor;
    half _SurfaceType;
    half _NormalScale;
    half _OcclusionStrength;
    half _Smoothness;
    half4 _DirtColor;
    half4 _MossColor;
    half4 _EdgeWearColor;
    half _WeatherStrength;
    half _DirtStrength;
    half _MossStrength;
    half _PaintAgeStrength;
    half _PaintFadeSaturation;
    half _RainStreakStrength;
    half _EdgeWearStrength;
    half _VertexWeatherStrength;
    half _DebugView;
CBUFFER_END

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);
TEXTURE2D(_WeatherMap);
SAMPLER(sampler_WeatherMap);

#endif
