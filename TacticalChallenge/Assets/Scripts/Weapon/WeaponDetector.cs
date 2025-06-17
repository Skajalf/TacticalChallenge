using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WeaponDetector : MonoBehaviour
{
    private WeaponComponent weaponComp;

    private void Awake()
    {
        weaponComp = GetComponentInParent<WeaponComponent>();

        Debug.Log("Detect Start");

        if (weaponComp == null)
            Debug.LogError("WeaponComponent가 부모에 없다!!!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            var wb = other.GetComponent<WeaponBase>();
            if (wb != null)
                weaponComp.SetDetectedWeapon(wb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            var wb = other.GetComponent<WeaponBase>();
            if (wb != null && weaponComp.GetDetectedWeapon() == wb)
                weaponComp.ClearDetectedWeapon(wb);
        }
    }
}
