using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TreeRenderer : MonoBehaviour
{
    public Material treeMaterial;
    public Tree tree;
    private Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = treeMaterial;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (tree.grown)
        {
            tree.grown = false;
            vertices = new List<Vector3>();
            triangles = new List<int>();
            tree.CalculateBranchDiameters();
            generateMesh(tree.root);
            UpdateMesh();
        }
    }

    void generateMesh(Tree.Metamer metamer)
    {
        if (metamer == null) return;
        else
        {

            float bottomDiameter = metamer.diameter;
            //float topDiameter = tree.MIN_DIAMETER;
            float mainTopDiameter = (metamer.main != null) ? metamer.main.diameter : tree.dna.MIN_DIAMETER;
            float lateralTopDiameter = (metamer.lateral != null) ? metamer.lateral.diameter : tree.dna.MIN_DIAMETER;
            float topDiamter = Mathf.Max(mainTopDiameter, lateralTopDiameter);
            MeshHelper.cylinder(vertices, triangles, metamer.bottom, metamer.top, bottomDiameter, topDiamter, 5);
            generateMesh(metamer.main);
            generateMesh(metamer.lateral);

            // STOP IF MESH TOO BIG
            if (vertices.Count >= 8000)
            {
                tree.stop = true;
                return;
            }
        }
    }


    void UpdateMesh()
    {
        mesh.Clear();

        Vector3[] meshVertices = new Vector3[vertices.Count];
        vertices.CopyTo(meshVertices);

        int[] meshTriangles = new int[triangles.Count];
        triangles.CopyTo(meshTriangles);

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
    }
}
