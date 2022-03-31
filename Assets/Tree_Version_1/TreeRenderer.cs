using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeRenderer : MonoBehaviour
{
    public Material treeMaterial;
    public Tree tree;
    //private Mesh mesh;
    private float shoot_lenght_percentage;
    private float elapsed_time;
    public float GROWTH_RATE;
    public float SCALE = 1;
    private bool noLeafs;
    public GameObject treeMesh;
    private List<GameObject> treeMeshes;
    private GameObject currentTreeMesh;

    // Start is called before the first frame update
    void Start()
    {
        treeMeshes = new List<GameObject>();
        currentTreeMesh = Instantiate(treeMesh, transform.position, Quaternion.identity);
        elapsed_time = 0f;
        noLeafs = true;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed_time += Time.deltaTime;
        if (elapsed_time >= GROWTH_RATE) elapsed_time = 0f;
        shoot_lenght_percentage = elapsed_time / GROWTH_RATE;

        if (!tree.stop) //tree.grown)
        {
            tree.grown = false;
            foreach (GameObject treeMesh in treeMeshes) Destroy(treeMesh);
            treeMeshes = new List<GameObject>();
            currentTreeMesh = Instantiate(treeMesh, transform.position, Quaternion.identity);
            generateMesh(tree.root);
            treeMeshes.Add(currentTreeMesh);
            UpdateMesh();
        }
        if (noLeafs & tree.stop)
        {
            noLeafs = false;
            //generateLeafMeshes(tree.root);
            //UpdateMesh();
        }
    }

    void generateLeafMeshes(Tree.Branch metamer)
    {
        //if (metamer == null) return;

        //if (metamer.isTerminal) generateLeafMesh(metamer.top);
        //else
        //{
        //    generateLeafMeshes(metamer.main);
        //    generateLeafMeshes(metamer.lateral);
        //}
    }

    void generateLeafMesh(Vector3 position)
    {
        MeshHelper.square(currentTreeMesh.GetComponent<TreeMesh>().vertices, currentTreeMesh.GetComponent<TreeMesh>().triangles, position, 1);
    }

    void generateMesh(Tree.Branch branch)
    {
        if (currentTreeMesh.GetComponent<TreeMesh>().vertices.Count > 60000)
        {
            treeMeshes.Add(currentTreeMesh);
            currentTreeMesh = Instantiate(treeMesh, transform.position, Quaternion.identity);
        }

        Vector3 top = branch.top;
        float bottomDiameter = branch.diameter;
        float topDiameter = tree.dna.MIN_DIAMETER;

        // terminal branches
        if (branch.growing)
        {
            float branch_length = (branch.top - branch.bottom).magnitude;
            top = branch.bottom + (branch.direction * branch_length * shoot_lenght_percentage);
            if ((top - branch.top).magnitude <= 0.1f) branch.growing = false;
        }
        if (branch.main != null)
        {
            generateMesh(branch.main);
            topDiameter = branch.main.diameter;
        }
        if (branch.lateral != null)
        {
            generateMesh(branch.lateral);
            topDiameter = Mathf.Max(topDiameter, branch.lateral.diameter);
        }
        MeshHelper.cylinder(currentTreeMesh.GetComponent<TreeMesh>().vertices, currentTreeMesh.GetComponent<TreeMesh>().triangles, branch.bottom, top, bottomDiameter, topDiameter, 20, SCALE);
    }


    void UpdateMesh()
    {
        foreach(GameObject treeMesh in treeMeshes)
        {
            treeMesh.GetComponent<TreeMesh>().UpdateMesh();
        }
    }
}
