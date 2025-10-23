using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private float destroyTime = 10.0f;

    private Rigidbody rb;
    private Collider projectileCollider;
    public WeaponBase weapon;

    private List<Vector3> trajectoryPoints = new List<Vector3>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if (gameObject.activeInHierarchy)
        {
            trajectoryPoints.Add(transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Environment"))
        {
            ObjectPoolingManager.Instance.ReturnToPool(gameObject);
            gameObject.SetActive(false);
        }
    }

    public bool Shoot(Vector3 initialLocation, Vector3 direction, float speed, float time)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        trajectoryPoints.Clear();
        trajectoryPoints.Add(initialLocation);

        transform.position = initialLocation;
        transform.rotation = Quaternion.LookRotation(direction);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = direction * speed;

        StartCoroutine(ReturnToPool(time));

        return true;
    }


    private IEnumerator ReturnToPool(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPoolingManager.Instance.ReturnToPool(gameObject);
    }

    public void OnDisable()
    {
        trajectoryPoints.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (projectileCollider == null)
        {
            projectileCollider = GetComponent<Collider>();
            if (projectileCollider == null) return;
        }

        if (projectileCollider is SphereCollider sphereCollider)
        {
            Vector3 center = transform.position + transform.TransformVector(sphereCollider.center);
            float radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, radius);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(center, transform.forward * radius * 3f);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(projectileCollider.bounds.center, projectileCollider.bounds.size);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * projectileCollider.bounds.extents.magnitude * 2f);
        }

        if (trajectoryPoints.Count > 1)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < trajectoryPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
            }
        }
    }
}