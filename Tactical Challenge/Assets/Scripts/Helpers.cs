using Unity.VisualScripting;
using UnityEngine;

public static class Extend_TransformHelpers
{
    public static Transform FindChildByName(this Transform transform, string name)
    {
        Transform[] transforms = transform.GetComponentsInChildren<Transform>();

        foreach (Transform t in transforms)
        {
            if (t.gameObject.name.Equals(name))
                return t;
        }

        return null;
    }
}
public static class MathHelpers
{
    public static bool IsNearlyEqual(float a, float b, float tolerance = 1e-6f)
    {
        return Mathf.Abs(a - b) <= tolerance;
    }

    public static bool IsNearlyZero(float a, float b, float tolerance = 1e-6f)
    {
        return Mathf.Abs(a) <= tolerance;
    }
}