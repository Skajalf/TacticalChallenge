using UnityEngine;

public class Ladys_Grace : Ranged
{
    [SerializeField] private string weaponHolsterName = "Bip001_Weapon";
    [SerializeField] private string muzzleTransformName = "fire_01";

    private Transform weaponTransform;
    protected Transform muzzleTransform;
    protected Vector3 muzzlePosition;

    protected override void Awake()
    {
        base.Awake();

        weaponTransform = rootObject.transform.FindChildByName(weaponHolsterName);
        Debug.Assert(weaponTransform != null);
        transform.SetParent(weaponTransform, false);

        muzzleTransform = rootObject.transform.FindChildByName(muzzleTransformName);
        Debug.Assert(muzzleTransform != null);

        gameObject.SetActive(true);
    }

    public override void Equip()
    {
        base.Equip();
        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();
        gameObject.SetActive(false);
    }
}
