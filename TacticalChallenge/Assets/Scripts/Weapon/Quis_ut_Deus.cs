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
        base.Equip();//Attach하도록 변경해야 함.

        weaponTransform = transform.root.FindChildByName(weaponHolsterName);
        Debug.Assert(weaponTransform != null, "무기 홀스터를 찾을 수 없습니다.");

        transform.SetParent(weaponTransform, false);
        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();//플레이어 전방 0.5f 부분의 지면에 Detach하도록 변경해야 함.
        transform.SetParent(null);
        gameObject.SetActive(false);
    }
}
