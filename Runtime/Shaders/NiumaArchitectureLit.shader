Shader "Niuma/Architecture/Lit"
{
    Properties
    {
        [Header(Material Template)]
        [Enum(Wood,0,Roof Tile,1,Stone,2,Wall,3,Painted,4)] _SurfaceType ("材质模板分类", Float) = 0

        [Header(Base)]
        [MainTexture] _BaseMap ("基础颜色贴图", 2D) = "white" {}
        [MainColor] _BaseColor ("基础颜色乘色", Color) = (1, 1, 1, 1)

        [Header(Weathering Placeholder)]
        [Range(0, 1)] _PaintFadeSaturation ("彩绘褪色保留饱和度", Float) = 0.45

        [Header(Debug)]
        [Enum(Final,0,BaseColor,1,VertexColor,2)] _DebugView ("调试视图", Float) = 0
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
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "NiumaArchitectureInput.hlsl"
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Simple Lit"
    CustomEditor "NiumaShader.Editor.NiumaArchitectureLitShaderGUI"
}
