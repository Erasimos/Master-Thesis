// MIT License

// Copyright (c) 2020 NedMakesGames

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Make sure this file is not included twice
#ifndef GRASSLAYERS_INCLUDED
#define GRASSLAYERS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "NMGGrassLayersHelpers.hlsl"


struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
};
    
struct VertexOutput
{
    float3 positionWS : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float2 uv : TEXCOORD2;
};

struct GeomtryOutput
{
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    
    float4 positionCS : SV_Position;
};

float4 _GrassColor;

VertexOutput Vertex(Attributes input)
{
    VertexOutput output = (VertexOutput) 0;
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.positionWS = vertexInput.positionWS;
    output.normalWS = normalInput.normalWS;
    
    output.uv = input.uv;
    return output;

}

[maxvertexcount(3)]
void Geometry(BuiltInTriangleIntersectionAttributes VertexOutputinputs[3], inout TriangleStream<GeomtryOutput> outputStream)
{
    GeomtryOutput output = (GeometryOutput) 0;
    
    for (int t = 0; t < 3; t++)
    {
        SetupVertex(inputs[t], output);
        outoutStream.Append(output);

    }

}

half4 Fragment(GeomtryOutput input) : SV_Target{
    
    InputData lightingInput = (InputData) 0;
    lightingInput.positionWS = input.positionsWS;
    lightingInput.normalWS = NormalizeNormalPerPixel(input.normalWS);
    lightingInput.viewDirectionWS = GetViewDirectionFromPosition(input.positionWS, input.positionsCS);
    lightingInput.shadowCoord = CalculateShadowCoord(input.positiionWS, input.positionCS);
    
    float3 albedo = _GrassColor.rgb;
    
    return UniversalFragmentBlinnPhong(lightingInput, albedo, 1, 0, 0, 1);
    
    }
#endif