using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowGrid
{
    //private
    public readonly int CELL_SIZE = 2;
    public readonly int Q_MAX = 8;
    public readonly float C = 0.2f; // full light exposure
    public readonly float a = 0.2f; // delta shadow = a * b ^-q
    public readonly float b = 1.5f; // delta shadow = a * b ^-q

    public GameObject shadowBox;

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
        if (shadowBox.GetComponent<Renderer>().bounds.Contains(pos))
        {
            return Mathf.Infinity;
        }

        Vector3 key = genKey(pos);
        if (grid.TryGetValue(key, out float shadowValue)) return shadowValue;
        else return 0;
    }

    public float getShadowValueKey(Vector3 key)
    {
        if (grid.TryGetValue(key, out float shadowValue)) return shadowValue;
        else return 0;
    }

    public float getLightExposure(Vector3 pos)
    {
        float s = getShadowValuePos(pos);
        float Q = Mathf.Max(C - s + a, 0);
        return Mathf.Max(Q, 0.01f);
    }

    public float deltaShadowValue(int q)
    {
        return a * Mathf.Pow(b, -q);
    }

    public void updateGrid(Vector3 key, int q)
    {
        for (int x = -q; x <= q; x++)
        {
            for (int z = -q; z <= q; z++)
            {
                Vector3 newKey = new Vector3(key.x + x, key.y - q, key.z + z);
                float oldVal = getShadowValueKey(newKey);
                float newVal = oldVal + deltaShadowValue(q);
                grid[key] = newVal;
            }
        }
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

        for (int q = 0; q <= Q_MAX; q ++)
        {
            updateGrid(key, q);
        }
    }

    public Vector3 findOptimalGrowthDirection(Vector3 pos)
    {
        Vector3 key = genKey(pos);
        float minShadowValue = getShadowValueKey(key);
        Vector3 optimalKey = key;

        for (int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                for(int z = -1; z <= 1; z++)
                {
                    Vector3 newKey = new Vector3(key.x + x, key.y + y, key.z + z);
                    float shadowValue = getShadowValueKey(newKey);
                    if (shadowValue < minShadowValue) optimalKey = newKey;
                }
            }
        }

        // convert optimal key to optimal direction
        Vector3 optimalDirection = new Vector3(optimalKey.x * CELL_SIZE, optimalKey.y * CELL_SIZE, optimalKey.z * CELL_SIZE).normalized;
        return optimalDirection;
    }
}
