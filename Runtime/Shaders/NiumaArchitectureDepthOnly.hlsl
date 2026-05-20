#ifndef NIUMA_ARCHITECTURE_DEPTH_ONLY_INCLUDED
#define NIUMA_ARCHITECTURE_DEPTH_ONLY_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "NiumaArchitectureMaterial.hlsl"

struct NiumaDepthOnlyAttributes
{
    float4 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct NiumaDepthOnlyVaryings
{
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

NiumaDepthOnlyVaryings NiumaArchitectureDepthOnlyVertex(NiumaDepthOnlyAttributes input)
{
    NiumaDepthOnlyVaryings output = (NiumaDepthOnlyVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

half NiumaArchitectureDepthOnlyFragment(NiumaDepthOnlyVaryings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    return input.positionCS.z;
}

#endif
