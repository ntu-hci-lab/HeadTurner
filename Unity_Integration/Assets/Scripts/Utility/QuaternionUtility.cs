using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionUtility : MonoBehaviour
{
    public static Quaternion AverageRotation(List<Quaternion> multipleRotations)
    {
        int addAmount = 0;

        //Global variable which represents the additive quaternion
        Quaternion addedRotation = Quaternion.identity;

        //The averaged rotational value
        Quaternion averageRotation = Quaternion.identity;

        //Loop through all the rotational values.
        foreach (Quaternion singleRotation in multipleRotations)
        {
            //Temporary values
            float w;
            float x;
            float y;
            float z;

            //Amount of separate rotational values so far
            addAmount++;

            float addDet = 1.0f / (float)addAmount;
            addedRotation.w += singleRotation.w;
            w = addedRotation.w * addDet;
            addedRotation.x += singleRotation.x;
            x = addedRotation.x * addDet;
            addedRotation.y += singleRotation.y;
            y = addedRotation.y * addDet;
            addedRotation.z += singleRotation.z;
            z = addedRotation.z * addDet;

            //Normalize. Note: experiment to see whether you
            //can skip this step.
            float D = 1.0f / (w * w + x * x + y * y + z * z);
            w *= D;
            x *= D;
            y *= D;
            z *= D;

            //The result is valid right away, without
            //first going through the entire array.
            averageRotation = new Quaternion(x, y, z, w);
        }
        return averageRotation;
    }
}
