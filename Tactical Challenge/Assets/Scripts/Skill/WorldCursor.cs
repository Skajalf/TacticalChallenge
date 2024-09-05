using UnityEngine;

public class WorldCursor : MonoBehaviour
{
    private float traceDistance;
    public float TraceDistance { set => traceDistance = value; }
    private LayerMask mask;
    public LayerMask Mask { set => mask = value; }

    private void Awake()
    {
        Transform scene = GameObject.Find("Scenes").transform;
        transform.SetParent(scene, false);
    }

    private void Update()
    {
        Vector3 position;
        Vector3 normal;

        if (CameraHelpers.GetCursorLocation(out position, out normal, traceDistance, mask) == false)
            return;

        position += normal * 0.05f;
        transform.localPosition = position;

        Vector3 up = Quaternion.Euler(-90, 0, 0) * Vector3.up;
        Quaternion rotation = Quaternion.FromToRotation(up, normal);
        transform.localRotation = rotation;

        position.x += 0.1f;
    }
}