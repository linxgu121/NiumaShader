#ifndef NIUMA_ARCHITECTURE_MATERIAL_INCLUDED
#define NIUMA_ARCHITECTURE_MATERIAL_INCLUDED

// 所有 Pass 共享同一份材质常量布局，避免 Forward / Shadow / Depth Pass 的参数声明漂移。
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _DetailMap_ST;
    float4 _HeightMap_ST;
    float4 _WeatherMap_ST;
    half4 _BaseColor;
    half _SurfaceType;
    half _NormalScale;
    half _DetailStrength;
    half _ParallaxStrength;
    half _ParallaxCenter;
    half _AnisotropyStrength;
    half _AnisotropyDirection;
    half _OcclusionStrength;
    half _Smoothness;
    half4 _DirtColor;
    half4 _MossColor;
    half4 _EdgeWearColor;
    half4 _AnisotropyColor;
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
TEXTURE2D(_DetailMap);
SAMPLER(sampler_DetailMap);
TEXTURE2D(_HeightMap);
SAMPLER(sampler_HeightMap);
TEXTURE2D(_MaskMap);
SAMPLER(sampler_MaskMap);
TEXTURE2D(_WeatherMap);
SAMPLER(sampler_WeatherMap);

#endif
