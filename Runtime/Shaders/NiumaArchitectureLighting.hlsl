#ifndef NIUMA_ARCHITECTURE_LIGHTING_INCLUDED
#define NIUMA_ARCHITECTURE_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct NiumaArchitectureSurfaceData
{
    half3 baseAlbedo;
    half3 albedo;
    half alpha;
    half3 normalWS;
    half occlusion;
    half smoothness;
    half edgeWear;
    half dirtMask;
    half mossMask;
    half paintFadeMask;
    half rainMask;
};

half3 NiumaApplyMainLight(NiumaArchitectureSurfaceData surface, half3 viewDirectionWS, Light light)
{
    half ndotl = saturate(dot(surface.normalWS, light.direction));
    half attenuation = light.distanceAttenuation * light.shadowAttenuation;
    half3 diffuse = surface.albedo * light.color * ndotl * attenuation;

    // 古建筑材质以木、瓦、石、灰墙为主，高光只做克制的湿润感，不走强金属/塑料质感。
    half3 halfDirection = SafeNormalize(light.direction + viewDirectionWS);
    half specPower = exp2(lerp(3.0, 8.0, surface.smoothness));
    half specular = pow(saturate(dot(surface.normalWS, halfDirection)), specPower);
    half specularStrength = surface.smoothness * 0.18;

    return diffuse + light.color * specular * specularStrength * attenuation;
}

half3 NiumaApplyArchitectureLighting(NiumaArchitectureSurfaceData surface, half3 viewDirectionWS, float4 shadowCoord, half3 bakedGI)
{
    Light mainLight = GetMainLight(shadowCoord);

    half3 indirect = surface.albedo * bakedGI * surface.occlusion;
    half3 direct = NiumaApplyMainLight(surface, viewDirectionWS, mainLight);
    return indirect + direct;
}

#endif
