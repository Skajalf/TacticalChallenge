using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float force = 250.0f;
    [SerializeField] private float DestroyTime = 10.0f;

    public WeaponBase weapon;

    private new Rigidbody rigidbody;
    private new Collider collider;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        Destroy(gameObject, DestroyTime);

        rigidbody.AddForce(transform.forward * force);
    }
}