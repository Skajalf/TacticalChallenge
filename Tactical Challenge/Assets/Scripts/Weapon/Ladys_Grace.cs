using UnityEngine;
using UnityEngine.Rendering;

//TODO: ���� �ϳ� ������ ��
public class Ladys_Grace : Ranged
{
    [SerializeField] private string WeaponHolster = "Bip001_Weapon";
    [SerializeField] private string muzzleTransformName = "fire_01";

    private Transform WeaponTransform;
    private Transform muzzleTransform;

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
        base.Begin_DoAction();

        if (projectilePrefab == null)
            return;

        Vector3 muzzlePosition = muzzleTransform.position;
        muzzlePosition += rootObject.transform.forward * 0.5f;

        GameObject obj = Instantiate<GameObject>(projectilePrefab, muzzlePosition, rootObject.transform.rotation);
        Projectile projectile = obj.GetComponent<Projectile>();
        {
            projectile.OnProjectileHit += OnProjectileHit;
        }
        obj.SetActive(true);
    }

    private void OnProjectileHit(Collider self, Collider hitCollider, Vector3 hitPoint)
    {
        IDamagable damageable = hitCollider.GetComponent<IDamagable>();

        if (damageable != null)
        {
            StatComponent stat = hitCollider.GetComponent<StatComponent>();
            float damageAmount = weapondata.Power;

            // `IDamagable.OnDamage` ȣ��
            damageable.OnDamage(rootObject, this, hitPoint, damageAmount);

            // �߰��� ü�� ���¸� �α׷� ���
            if (stat != null)
            {
                Debug.Log($"{hitCollider.name}�� ���� ü��: {stat.GetCurrentHealth()}");
            }
        }

        //if (weapondata.HitParticle != null)
        //    Instantiate<GameObject>(weapondata.HitParticle, point, rootObject.transform.rotation);
    }

}