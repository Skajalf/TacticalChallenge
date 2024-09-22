using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quis_ut_Deus : WeaponBase
{
    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    protected override void Init()
    {
        base.Init();
    }

    public override void Equip()
    {
        base.Equip();//Attach�ϵ��� �����ؾ� ��.

        weaponTransform = transform.root.FindChildByName(weaponHolsterName);
        Debug.Assert(weaponTransform != null, "���� Ȧ���͸� ã�� �� �����ϴ�.");

        transform.SetParent(weaponTransform, false);
        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();//�÷��̾� ���� 0.5f �κ��� ���鿡 Detach�ϵ��� �����ؾ� ��.
        transform.SetParent(null);
        gameObject.SetActive(false);
    }
}
