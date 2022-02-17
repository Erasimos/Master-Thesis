using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestGenerator : MonoBehaviour
{

    public int NUMBER_OF_TREES = 10;
    public float MIN_TREE_DISTANCE = 4;
    public Vector2 FOREST_SIZE = new Vector2(2000, 2000);

    public GameObject tree;

    // Start is called before the first frame update
    void Start()
    {
        List<Vector2> treePositions = PossoinDiscSampling.GeneratePoints(MIN_TREE_DISTANCE, FOREST_SIZE, NUMBER_OF_TREES);

        foreach(Vector2 treePosition in treePositions)
        {
            tree.GetComponent<TreeGenerator>().treePosition = new Vector3(treePosition.x, 0, treePosition.y);
            Instantiate(tree, new Vector3(treePosition.x, 0, treePosition.y), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
