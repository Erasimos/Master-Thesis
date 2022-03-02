using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forest : MonoBehaviour
{

    public int NUMBER_OF_TREES;
    public float MIN_TREE_DISTANCE;
    public GameObject tree;
    public GameObject ground;
    private ShadowGrid shadowGrid;
    // Start is called before the first frame update
    void Start()
    {

        float ground_size_x = ground.GetComponent<MeshRenderer>().bounds.size.x;
        float ground_size_z = ground.GetComponent<MeshRenderer>().bounds.size.z;
        List<Vector2> spawnPoints = PossoinDiscSampling.GeneratePoints(MIN_TREE_DISTANCE, new Vector2(ground_size_x, ground_size_z), NUMBER_OF_TREES);

        foreach(Vector2 spawnPos in spawnPoints)
        {
            spawnTree(spawnPos);
        }
    }

    void spawnTree(Vector2 position)
    {
        TreeDNA dna = new TreeDNA();
        shadowGrid = new ShadowGrid();
        dna.MIN_DIAMETER = 0.1f;
        dna.BRANCH_DIAMTER_n = 2.2f;
        dna.ENERGY_COEEFICENT = 1f;
        dna.ENERGY_LAMBDA = 0.8f;
        dna.LEAF_ENERGY = 2;
        dna.BRANCHING_ANGLE_LATERAL = 24f;
        dna.BRANCHING_ANGLE_MAIN = 3f;
        dna.APICAL_DECLINE = 0.98f; // per season
        GameObject newTree = Instantiate(tree, new Vector3(position.x, 0, position.y), Quaternion.identity);
        newTree.GetComponent<GrowthEngine>().Grow(dna, shadowGrid);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
