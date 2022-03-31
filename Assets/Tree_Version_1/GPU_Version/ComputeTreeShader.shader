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

            #include "UnityCG.cginc"

            fixed4 _Color;

            uniform StructuredBuffer<float3> mesh_triangles;

            float4 vert(uint vertex_id: SV_vertexID, float3 pos : POSITION) : SV_POSITION
            {
                //return pos;
                float3 position = mesh_triangles[vertex_id];
                return mul(UNITY_MATRIX_VP, float4(position, 1));
                //return mul(UNITY_MATRIX_VP, float4(pos, 1));
                //return float4(position, 1);
            }

            float4 frag() : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
