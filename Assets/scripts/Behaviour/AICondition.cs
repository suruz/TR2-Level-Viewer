using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICondition
{
    public static bool IsInRange(float distance)
    {
        return false;
    }

    public static void SetActive(GameObject go, bool active)
    {
#if UNITY_5_3_OR_NEWER
        go.SetActiveRecursively(active);
#else
        go.SetActiveRecursively(active);
#endif

    }

}
