#ifndef NIUMA_ARCHITECTURE_LIGHTING_INCLUDED
#define NIUMA_ARCHITECTURE_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct NiumaArchitectureSurfaceData
{
    half3 baseAlbedo;
    half3 albedo;
    half alpha;
    half3 normalWS;
#if defined(_NIUMA_ANISOTROPY)
    half3 tangentWS;
    half3 bitangentWS;
#endif
    half occlusion;
    half smoothness;
    half detailMask;
    half parallaxHeight;
    half edgeWear;
    half dirtMask;
    half mossMask;
    half paintFadeMask;
    half rainMask;
};

half3 NiumaApplyAnisotropicHighlight(NiumaArchitectureSurfaceData surface, half3 viewDirectionWS, Light light, half3 halfDirection, half attenuation)
{
#if defined(_NIUMA_ANISOTROPY)
    half anisotropyStrength = saturate(_AnisotropyStrength);
    half3 anisotropyAxis = SafeNormalize(lerp(surface.tangentWS, surface.bitangentWS, saturate(_AnisotropyDirection)));
    half axisAlignment = 1.0 - abs(dot(halfDirection, anisotropyAxis));
    half normalAlignment = saturate(dot(surface.normalWS, halfDirection));
    half lightFacing = saturate(dot(surface.normalWS, light.direction));
    half anisotropicShape = pow(saturate(axisAlignment), exp2(lerp(4.0, 8.0, surface.smoothness)));
    half grazingFade = saturate(1.0 - abs(dot(viewDirectionWS, surface.normalWS)) * 0.65);
    half strength = anisotropicShape * normalAlignment * lightFacing * surface.smoothness * anisotropyStrength * grazingFade * 0.42;
    return light.color * _AnisotropyColor.rgb * strength * attenuation;
#else
    return half3(0.0, 0.0, 0.0);
#endif
}

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
    half3 anisotropic = NiumaApplyAnisotropicHighlight(surface, viewDirectionWS, light, halfDirection, attenuation);

    return diffuse + light.color * specular * specularStrength * attenuation + anisotropic;
}

half3 NiumaApplyArchitectureLighting(NiumaArchitectureSurfaceData surface, half3 viewDirectionWS, float4 shadowCoord, half3 bakedGI)
{
    Light mainLight = GetMainLight(shadowCoord);

    half3 indirect = surface.albedo * bakedGI * surface.occlusion;
    half3 direct = NiumaApplyMainLight(surface, viewDirectionWS, mainLight);
    return indirect + direct;
}

#endif
