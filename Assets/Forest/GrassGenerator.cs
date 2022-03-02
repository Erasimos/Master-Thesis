using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassGenerator : MonoBehaviour
{

    public int rows;
    public int columns;
    public GameObject grass;
    public GameObject ground;
    public int spacing;

    // Start is called before the first frame update
    void Start()
    {

        if (grass == null) return;

        for(int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            for(int colIndex = 0; colIndex < columns; colIndex++)
            {
                Vector3 offset = new Vector3(ground.GetComponent<MeshRenderer>().bounds.size.x/2, 0, ground.GetComponent<MeshRenderer>().bounds.size.z/2);
                Vector3 position = ground.transform.position + new Vector3(rowIndex * spacing, 0, colIndex * spacing) - offset;
                Instantiate(grass, position, Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
