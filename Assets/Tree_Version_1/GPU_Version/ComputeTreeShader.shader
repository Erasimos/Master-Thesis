Shader "Unlit/ComputeTreeShader"
{
    Properties
    {
        _Color("Tint", Color) = (0, 1, 0, 1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            fixed4 _Color;

            uniform StructuredBuffer<float4x4> branch_TRS_matrices;
            uniform StructuredBuffer<int> triangles;
            uniform StructuredBuffer<float3> vertices;
            uniform sampler2D shadowMapTexture;

            float4 vert(uint vertex_id: SV_vertexID, uint instance_id : SV_InstanceID) : SV_POSITION
            {
                int positionIndex = triangles[vertex_id];
                float3 position = vertices[positionIndex];
                position = mul(branch_TRS_matrices[instance_id], float4(position, 1)).xyz;
                return mul(UNITY_MATRIX_VP, float4(position, 1));
            }

                float4 frag(UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
                float4 shadowSample = tex2D(shadowMapTexture, screenPos.xy);

                return _Color;
            }
            ENDCG
        }
    }
}
