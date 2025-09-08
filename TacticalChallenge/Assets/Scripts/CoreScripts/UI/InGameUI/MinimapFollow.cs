using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 50, 0);

    void LateUpdate()
    {
        if (player == null) return;

        transform.position = player.position + offset;

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}