#ifndef NIUMA_ARCHITECTURE_DEBUG_INCLUDED
#define NIUMA_ARCHITECTURE_DEBUG_INCLUDED

bool NiumaTryGetDebugColor(half debugView, NiumaArchitectureSurfaceData surface, half4 vertexColor, out half4 color)
{
    color = half4(0.0, 0.0, 0.0, surface.alpha);

    if (debugView < 0.5)
    {
        return false;
    }

    if (debugView < 1.5)
    {
        color.rgb = surface.baseAlbedo;
        return true;
    }

    if (debugView < 2.5)
    {
        color.rgb = surface.normalWS * 0.5 + 0.5;
        return true;
    }

    if (debugView < 3.5)
    {
        color.rgb = half3(surface.occlusion, surface.occlusion, surface.occlusion);
        return true;
    }

    if (debugView < 4.5)
    {
        color.rgb = half3(surface.smoothness, surface.smoothness, surface.smoothness);
        return true;
    }

    if (debugView < 5.5)
    {
        color.rgb = half3(surface.edgeWear, surface.edgeWear, surface.edgeWear);
        return true;
    }

    if (debugView < 6.5)
    {
        color.rgb = half3(surface.dirtMask, surface.dirtMask, surface.dirtMask);
        return true;
    }

    if (debugView < 7.5)
    {
        color.rgb = half3(surface.mossMask, surface.mossMask, surface.mossMask);
        return true;
    }

    if (debugView < 8.5)
    {
        color.rgb = half3(surface.paintFadeMask, surface.paintFadeMask, surface.paintFadeMask);
        return true;
    }

    if (debugView < 9.5)
    {
        color.rgb = half3(surface.rainMask, surface.rainMask, surface.rainMask);
        return true;
    }

    if (debugView < 10.5)
    {
        color.rgb = vertexColor.rgb;
        color.a = vertexColor.a;
        return true;
    }

    if (debugView < 11.5)
    {
        color.rgb = half3(surface.detailMask, surface.detailMask, surface.detailMask);
        return true;
    }

    return false;
}

#endif
