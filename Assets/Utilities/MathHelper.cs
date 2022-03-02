using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    public static Vector3 GetPerpendicularVector(Vector3 v)
    {
        return Vector3.Normalize(new Vector3(v.y, -v.x, 0));
    }

    public static float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

}
