#ifndef NIUMA_ARCHITECTURE_WEATHERING_INCLUDED
#define NIUMA_ARCHITECTURE_WEATHERING_INCLUDED

half NiumaApplyVertexWeatherBonus(half mask, half vertexChannel)
{
    return saturate(mask * (1.0 + saturate(vertexChannel) * _VertexWeatherStrength));
}

void NiumaApplyEdgeWear(inout NiumaArchitectureSurfaceData surface, half4 vertexColor)
{
    half edge = saturate(surface.edgeWear * _EdgeWearStrength);
    edge = NiumaApplyVertexWeatherBonus(edge, vertexColor.b);
    surface.edgeWear = edge;

    // 边缘磨损来自 MaskMap.B，与 WeatherMap 无关，保证没有旧化贴图时仍可表现石阶、木梁边缘的磨损。
    surface.albedo = lerp(surface.albedo, _EdgeWearColor.rgb, edge);
}

void NiumaApplyArchitectureWeathering(inout NiumaArchitectureSurfaceData surface, float2 weatherUV, half4 vertexColor)
{
    NiumaApplyEdgeWear(surface, vertexColor);

#if defined(_NIUMA_WEATHERING)
    half4 weather = SAMPLE_TEXTURE2D(_WeatherMap, sampler_WeatherMap, weatherUV);

    half dirt = saturate(weather.r * _WeatherStrength * _DirtStrength);
    dirt = NiumaApplyVertexWeatherBonus(dirt, vertexColor.r);
    surface.dirtMask = dirt;
    surface.albedo = lerp(surface.albedo, _DirtColor.rgb, dirt);

    half moss = saturate(weather.g * _WeatherStrength * _MossStrength);
    moss = NiumaApplyVertexWeatherBonus(moss, vertexColor.g);
    surface.mossMask = moss;
    half3 mossMul = lerp(half3(1.0, 1.0, 1.0), _MossColor.rgb * 1.25, moss);
    surface.albedo *= mossMul;

    half paintFade = saturate(weather.b * _WeatherStrength * _PaintAgeStrength);
    paintFade = NiumaApplyVertexWeatherBonus(paintFade, vertexColor.r);
    surface.paintFadeMask = paintFade;
    half luminance = dot(surface.albedo, half3(0.299, 0.587, 0.114));
    half3 faded = lerp(half3(luminance, luminance, luminance), surface.albedo, _PaintFadeSaturation);
    surface.albedo = lerp(surface.albedo, faded, paintFade);

    half rain = saturate(weather.a * _WeatherStrength * _RainStreakStrength);
    surface.rainMask = rain;
    surface.albedo *= lerp(1.0, 0.82, rain);
#endif
}

#endif
