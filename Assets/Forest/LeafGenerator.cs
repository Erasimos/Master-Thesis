using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LeafGenerator : MonoBehaviour
{
    int MAX_NUMBER_OF_VERTICES = 65535;

    public List<Vector3> leafPositions;
    public Vector3 treePosition;
    public Material leafMaterial;

    Mesh mesh;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>(); 
    int triangleIndex = 0;

    // LEAF PARAMETERS
    float LEAF_WIDTH = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Color"));
        //GetComponent<MeshRenderer>().material.SetColor("_Color", new Vector4(0.0f, 0.5f, 0.3f, 0.5f));
        GetComponent<MeshRenderer>().material = leafMaterial;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        triangleIndex = 0;
        GenerateLeafs();
        UpdateMesh();
    } 

    public void Grow(List<Vector3> leafPositions)
    {
        
        this.leafPositions = leafPositions;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        triangleIndex = 0;
        GenerateLeafs();
        UpdateMesh();   
    }

    void GenerateLeafs()
    {
        foreach (Vector3 leafPos in this.leafPositions)
        {

            if (vertices.Count >= MAX_NUMBER_OF_VERTICES) return;

            Vector3 leafPosition = leafPos; // + treePosition;

            int offset = triangleIndex * 4;

            vertices.Add(new Vector3(0, 0, 0) + leafPosition);
            vertices.Add(new Vector3(LEAF_WIDTH, 0, 0) + leafPosition);
            vertices.Add(new Vector3(0, 0, LEAF_WIDTH) + leafPosition);
            vertices.Add(new Vector3(LEAF_WIDTH, 0, LEAF_WIDTH) + leafPosition);

            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));

    

            // Triangle 1
            triangles.Add(offset + 0);
            triangles.Add(offset + 1);
            triangles.Add(offset + 2);
            // Triangle 2
            triangles.Add(offset + 1);
            triangles.Add(offset + 3);
            triangles.Add(offset + 2);
            

            triangleIndex++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        Vector3[] meshVertices = new Vector3[vertices.Count];

        // Copy vertices from vertices list
        int index = 0;
        foreach (Vector3 vertex in vertices)
        {
            meshVertices[index] = vertex;
            index++;
        }

        // Copy triangle indexes
        int[] meshTriangles = new int[triangles.Count];
        index = 0;
        foreach (int triangle in triangles)
        {
            meshTriangles[index] = triangle;
            index++;
        }

        // Copy uv-coordinates
        Vector2[] meshUvs = new Vector2[uvs.Count];
        index = 0;
        foreach(Vector2 uv in uvs)
        {
            meshUvs[index] = uv;
            index++;
        }

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.uv = meshUvs;
        mesh.RecalculateNormals();
    }
}
