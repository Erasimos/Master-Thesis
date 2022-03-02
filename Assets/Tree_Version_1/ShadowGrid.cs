using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowGrid
{
    //private
    private readonly int CELL_SIZE = 1;
    private IDictionary<Vector3, float> grid;

    public ShadowGrid()
    {
        grid = new Dictionary<Vector3, float>();
    }

    public Vector3 genKey(Vector3 pos)
    {
        int x_index = Mathf.FloorToInt(pos.x / CELL_SIZE);
        int y_index = Mathf.FloorToInt(pos.y / CELL_SIZE);
        int z_index = Mathf.FloorToInt(pos.z / CELL_SIZE);
        return new Vector3(x_index, y_index, z_index);
    }

    public float getShadowValuePos(Vector3 pos)
    {
        Vector3 key = genKey(pos);
        if (grid.TryGetValue(key, out float shadowValue)) return shadowValue;
        else return 0;
    }

    public float getShadowValueKey(Vector3 key)
    {
        if (grid.TryGetValue(key, out float shadowValue)) return shadowValue;
        else return 0;
    }

    public void updateGrid(Vector3 pos)
    {
        Vector3 key = genKey(pos);
        float oldVal;
        float newVal;

        // Top of pyramid
        oldVal = getShadowValueKey(key);
        newVal = Mathf.Min(0.99f, oldVal + 0.6f);
        grid[key] = newVal;

        // Middle of pyramid
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 newKey = new Vector3(key.x + x, key.y - 1, key.z + z);
                oldVal = getShadowValueKey(newKey);
                newVal = Mathf.Min(0.99f, oldVal + 0.2f);
            }
        }

        // Bottom of pyramid
        for (int x = -2; x <= 2; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                Vector3 newKey = new Vector3(key.x + x, key.y - 2, key.z + z);
                oldVal = getShadowValueKey(newKey);
                newVal = Mathf.Min(0.99f, oldVal + 0.1f);
            }
        }
    }
}
