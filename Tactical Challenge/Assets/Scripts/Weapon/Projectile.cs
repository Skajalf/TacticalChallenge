using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float force = 1000.0f;
    [SerializeField] private float DestroyTime = 10.0f;

    public Weapon owner;

    private new Rigidbody rigidbody;
    private new Collider collider;

    public event Action<Collider, Collider, Vector3> OnProjectileHit;

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

    private void OnTriggerEnter(Collider other) // Character이면 데미지를 입힌다.
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Character"))
            other.gameObject.GetComponent<Character>().OnDamage(owner.weapondata.Power);
        else
            Destroy(gameObject);
    }
}