using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassGenerator : MonoBehaviour
{

    public int rows;
    public int columns;
    public GameObject grass;
    public int spacing;

    // Start is called before the first frame update
    void Start()
    {

        Vector3 offset = new Vector3(35, 0, 35);

        if (grass == null) return;

        for(int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            for(int colIndex = 0; colIndex < columns; colIndex++)
            {
                Instantiate(grass, new Vector3(rowIndex * spacing, 0, colIndex * spacing) - offset, Quaternion.identity, this.transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
