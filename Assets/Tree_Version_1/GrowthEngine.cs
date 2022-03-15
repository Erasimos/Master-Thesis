using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowthEngine : MonoBehaviour
{
    public Tree tree;
    public float GROWTH_RATE = 1; // Seconds
    private float last_growth;
    private bool growing;

    // Start is called before the first frame update
    void Start()
    {
        last_growth = GROWTH_RATE;
    }


    // Update is called once per frame
    void Update()
    {

        if (tree.stop) return;

        last_growth -= Time.deltaTime;
        if(last_growth <= 0)
        {
            tree.Grow();
            tree.grown = true;
            last_growth = GROWTH_RATE;
        }
    }
}
