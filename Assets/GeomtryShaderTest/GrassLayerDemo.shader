Shader "GrassLayerDemo" {
    Properties
    {
        _GrassColor("Grass color", color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        // Forward Lit Pass
        Pass {
            Name = "ForwardLid"
            Tags{"LightMode" = "UniversalForward"}
            Cull Back

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma require geometry

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma vertex Vertex
            #pragma geometry Geomtry
            #pragma fragment Fragment
            
            #include "GrassLayerDemo.hlsl"

            ENDHLSLS
        }
    }
}

