using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Obstacles : MonoBehaviour
{
    public static event Action<Obstacles, Transform> OnCoverAvailable;
    public static event Action<Obstacles, Transform> OnCoverUnavailable;

    [SerializeField] private float detectionRadius = 1.0f;
    [SerializeField] private LayerMask playerLayer;

    private bool playerInRange = false;
    private Transform targetPlayer;

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        if (hits.Length > 0)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                targetPlayer = hits[0].transform;
                OnCoverAvailable?.Invoke(this, targetPlayer);
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                OnCoverUnavailable?.Invoke(this, targetPlayer);
                targetPlayer = null;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}