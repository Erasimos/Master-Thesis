using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forest : MonoBehaviour
{

    public int NUMBER_OF_TREES;
    public float MIN_TREE_DISTANCE;
    public GameObject tree_obj;
    public GameObject tree_obj_GPU_INSTANCING;
    public GameObject ground;
    public GameObject shadowBox;
    private ShadowGrid shadowGrid;

    // Start is called before the first frame update
    void Start()
    {

        float ground_size_x = ground.GetComponent<MeshRenderer>().bounds.size.x;
        float ground_size_z = ground.GetComponent<MeshRenderer>().bounds.size.z;
        List<Vector2> spawnPoints = PossoinDiscSampling.GeneratePoints(MIN_TREE_DISTANCE, new Vector2(ground_size_x, ground_size_z), NUMBER_OF_TREES);
        shadowGrid = new ShadowGrid();
        GameObject sBox = Instantiate(shadowBox, new Vector3(0, 100, 0), Quaternion.identity);
        shadowGrid.shadowBox = sBox;

        

        TreeDNA dna = new TreeDNA();
        dna.MIN_DIAMETER = 0.02f;
        dna.BRANCH_DIAMTER_n = 2.0f;
        dna.ENERGY_COEEFICENT = 3f;
        dna.ENERGY_LAMBDA = 0.5f;
        dna.LEAF_ENERGY = 1.4f;
        dna.DIRECTION_SAMPLES = 10;
        dna.SHADOW_SAMPLES = 1;
        dna.PERCEPTION_ANGLE = 100f;
        dna.APICAL_DECLINE = 0.9f;//0.87f; // per season
        dna.GRAVITROPISM_WIEGHT = 0.78f;
        dna.GRAVITROPISM_DECLINE = 0.1f;
        dna.GRAVITROPISM = new Vector3(0, 1, 0);
        dna.SELFTROPISM_WEIGHT = 0.3f;
        dna.MAX_AGE = 7;
        dna.AGE_WEIGHT = 0.0003f;
        dna.DEPTH_WEIGTH = 0.9f;
        dna.MAX_BUDS_PER_SEGMENT = 3;
        dna.MIN_BUDS_PER_SEGMENT = 2;
        dna.SPROUT_ENERGY = 1f;
        dna.BUD_SPREAD = 0.4f;
        dna.BUD_DEATH_TRESHOLD = 4f;

        
        foreach (Vector2 pos in spawnPoints)
        {
            //spawnTree_GPU_INSTANCING(new Vector3(pos.x, 0, pos.y), dna);
            spawnTree(new Vector3(pos.x, 0, pos.y), dna);
        }
        
    }


    void spawnTree(Vector3 position, TreeDNA dna)
    {
        GameObject newTree = Instantiate(tree_obj, position, Quaternion.identity);
        Tree tree = new Tree(dna, shadowGrid);
        newTree.GetComponent<TreeRenderer>().tree = tree;
        newTree.GetComponent<GrowthEngine>().tree = tree;
    }

    void spawnTree_GPU_INSTANCING(Vector3 position, TreeDNA dna)
    {
        GameObject newTree_GPU_INSTANCING = Instantiate(tree_obj_GPU_INSTANCING, position, Quaternion.identity);
        Tree tree = new Tree(dna, shadowGrid);
        newTree_GPU_INSTANCING.GetComponent<TreeRenderer_GPU_INSTANCING>().tree = tree;
        newTree_GPU_INSTANCING.GetComponent<GrowthEngine>().tree = tree;
    }

}
