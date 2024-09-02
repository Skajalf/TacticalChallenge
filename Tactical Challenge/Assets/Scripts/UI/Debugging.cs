using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Debugging : MonoBehaviour
{
    WeaponComponent component;
    Weapon weapon;
    GameObject obj;
    Text text;

    public void Awake()
    {
        obj = GetComponent<GameObject>();
        component = GameObject.Find("CH0137").GetComponent<WeaponComponent>();
        weapon = component.weapon;
        text = gameObject.GetComponent<Text>();
    }

    public void Update()
    {
        weapon = component.weapon;
        text.text = $"{weapon.weapondata.currentAmmo}/{weapon.weapondata.Ammo}";
    }
}
