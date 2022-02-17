using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LeafGenerator : MonoBehaviour
{

    public List<Vector3> leafPositions;
    public Vector3 treePosition;

    Mesh mesh;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    int triangleIndex = 0;

    // LEAF PARAMETERS
    float LEAF_WIDTH = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = new Material(Shader.Find("Specular"));
        GetComponent<MeshRenderer>().material.SetColor("_Color", new Vector4(0.0f, 0.5f, 0.3f, 0.5f));
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
            Vector3 leafPosition = leafPos; // + treePosition;

            int offset = triangleIndex * 8;

            vertices.Add(new Vector3(0, 0, 0) + leafPosition);
            vertices.Add(new Vector3(LEAF_WIDTH, 0, 0) + leafPosition);
            vertices.Add(new Vector3(0, 0, LEAF_WIDTH) + leafPosition);
            vertices.Add(new Vector3(LEAF_WIDTH, 0, LEAF_WIDTH) + leafPosition);

            vertices.Add(new Vector3(0, 0.1f, 0) + leafPosition);
            vertices.Add(new Vector3(LEAF_WIDTH, 0.1f, 0) + leafPosition);
            vertices.Add(new Vector3(0, 0.1f, LEAF_WIDTH) + leafPosition);
            vertices.Add(new Vector3(LEAF_WIDTH, 0.1f, LEAF_WIDTH) + leafPosition);

            // Triangle 1
            triangles.Add(offset + 0);
            triangles.Add(offset + 1);
            triangles.Add(offset + 2);
            // Triangle 2
            triangles.Add(offset + 1);
            triangles.Add(offset + 3);
            triangles.Add(offset + 2);
            // Triangle 3
            triangles.Add(offset + 4);
            triangles.Add(offset + 6);
            triangles.Add(offset + 5);
            // Triangle 4
            triangles.Add(offset + 5);
            triangles.Add(offset + 6);
            triangles.Add(offset + 7);

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

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
    }
}
