using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassGenerator : MonoBehaviour
{
    public GameObject grass;
    public GameObject ground;
    public int spacing;
    public float grass_tile_area = 1000f;

    // Start is called before the first frame update
    void Start()
    {

        float cover_area = ground.GetComponent<MeshRenderer>().bounds.size.x * ground.GetComponent<MeshRenderer>().bounds.size.z;
        int required_grass_tiles = Mathf.CeilToInt(cover_area / grass_tile_area);


        //Vector3 offset = new Vector3(ground.GetComponent<MeshRenderer>().bounds.size.x/2, 0, ground.GetComponent<MeshRenderer>().bounds.size.z/2);
        Vector3 offset = new Vector3(ground.transform.position.x, ground.transform.position.y, ground.transform.position.z);
        offset -= new Vector3(ground.GetComponent<MeshRenderer>().bounds.size.x / 2, 0, ground.GetComponent<MeshRenderer>().bounds.size.z / 2);
        offset += new Vector3(Mathf.Sqrt(grass_tile_area) / 2, 0, Mathf.Sqrt(grass_tile_area) / 2);

        if (grass == null) return;

        int rows = required_grass_tiles / 2;
        int columns = required_grass_tiles / 2;

        for(int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            for(int colIndex = 0; colIndex < columns; colIndex++)
            {
                
                Vector3 position = new Vector3(rowIndex * spacing, 0, colIndex * spacing) + offset;
                Instantiate(grass, position, Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
