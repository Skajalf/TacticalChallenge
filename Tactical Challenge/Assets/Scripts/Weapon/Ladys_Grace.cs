using UnityEngine;
using UnityEngine.Rendering;

//TODO: 무기 하나 만들어야 함
public class Ladys_Grace : Ranged
{
    [SerializeField] private string WeaponHolster = "Bip001_Weapon";
    [SerializeField] private string muzzleTransformName = "fire_01";

    private Transform WeaponTransform;

    protected Ranged ranged;


    protected override void Reset()
    {
        base.Reset();
    }

    protected override void Awake()
    {
        base.Awake();

        WeaponTransform = rootObject.transform.FindChildByName(WeaponHolster);
        Debug.Assert(WeaponTransform != null);
        transform.SetParent(WeaponTransform, false);

        muzzleTransform = rootObject.transform.FindChildByName(muzzleTransformName);
        Debug.Assert(muzzleTransform != null);

        gameObject.SetActive(true);

        Begin_Equip();

        weapondata.currentAmmo = weapondata.Ammo;
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();

        gameObject.SetActive(false);
    }

    //public override void Play_Particle()
    //{
    //    base.Play_Particle();

    //    if (weapondata.Particle == null) return;

    //    Vector3 position = muzzleTransform.position;
    //    Quaternion rotation = rootObject.transform.rotation;

    //    Instantiate<GameObject>(weapondata.Particle, position, rotation);
    //}

    public override void Begin_DoAction()
    {
        if (projectilePrefab == null)
            return;

        muzzlePosition += rootObject.transform.forward * 0.5f;
        base.Begin_DoAction();
    }

}