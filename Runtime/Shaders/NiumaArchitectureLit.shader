Shader "Niuma/Architecture/Lit"
{
    Properties
    {
        [Header(Material Template)]
        [Enum(Wood,0,Roof Tile,1,Stone,2,Wall,3,Painted,4)] _SurfaceType ("材质模板分类", Float) = 0

        [Header(Base)]
        [MainTexture] _BaseMap ("基础颜色贴图", 2D) = "white" {}
        [MainColor] _BaseColor ("基础颜色乘色", Color) = (1, 1, 1, 1)

        [Header(Normal)]
        [Normal] _NormalMap ("法线贴图", 2D) = "bump" {}
        [Range(0, 2)] _NormalScale ("法线强度", Float) = 1

        [Header(Niuma MaskMap)]
        _MaskMap ("Niuma 遮罩贴图", 2D) = "white" {}
        [Range(0, 1)] _OcclusionStrength ("AO 强度", Float) = 1
        [Range(0, 1)] _Smoothness ("整体光滑度", Float) = 0.35

        [Header(Weathering)]
        _WeatherMap ("旧化遮罩贴图", 2D) = "black" {}
        [Range(0, 1)] _WeatherStrength ("整体旧化强度", Float) = 0.45
        _DirtColor ("灰尘颜色", Color) = (0.55, 0.50, 0.42, 1)
        [Range(0, 1)] _DirtStrength ("灰尘强度", Float) = 0.45
        _MossColor ("苔痕颜色", Color) = (0.30, 0.42, 0.22, 1)
        [Range(0, 1)] _MossStrength ("苔痕强度", Float) = 0.35
        [Range(0, 1)] _PaintAgeStrength ("彩绘褪色强度", Float) = 0.35
        [Range(0, 1)] _PaintFadeSaturation ("彩绘褪色保留饱和度", Float) = 0.45
        [Range(0, 1)] _RainStreakStrength ("雨痕压暗强度", Float) = 0.25
        _EdgeWearColor ("边缘磨损颜色", Color) = (0.72, 0.66, 0.54, 1)
        [Range(0, 1)] _EdgeWearStrength ("边缘磨损强度", Float) = 0
        [Range(0, 1)] _VertexWeatherStrength ("顶点色旧化加成", Float) = 0

        [Header(Debug)]
        [Enum(Final,0,BaseColor,1,Normal,2,AO,3,Smoothness,4,EdgeWear,5,Dirt,6,Moss,7,PaintFade,8,Rain,9,VertexColor,10)] _DebugView ("调试视图", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex NiumaArchitectureVertex
            #pragma fragment NiumaArchitectureFragment
            #pragma shader_feature_local _NIUMA_NORMALMAP
            #pragma shader_feature_local _NIUMA_WEATHERING
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "NiumaArchitectureInput.hlsl"
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Simple Lit"
    CustomEditor "NiumaShader.Editor.NiumaArchitectureLitShaderGUI"
}
