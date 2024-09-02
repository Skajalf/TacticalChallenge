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

    StatComponent dummy;

    public void Awake()
    {
        obj = GetComponent<GameObject>();
        component = GameObject.Find("CH0137").GetComponent<WeaponComponent>();
        weapon = component.weapon;
        text = gameObject.GetComponent<Text>();
        
        GetDummyInfo();
    }

    private void GetDummyInfo()
    {
        dummy = GameObject.Find("Dummy").GetComponent<StatComponent>();
    }

    public void Update()
    {
        weapon = component.weapon;
        text.text = $"{weapon.weapondata.currentAmmo}/{weapon.weapondata.Ammo} Dummy's HP:{dummy.CurrentHP}/{dummy.maxHealthPoint}";
    }
}
