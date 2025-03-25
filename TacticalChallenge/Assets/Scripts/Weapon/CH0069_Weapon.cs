using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class CH0069_Weapon : Weapon
{
    protected override void Awake()
    {
        base.Awake();
        IsFiring = false;

        ammo = stats.FirstOrDefault(x => x.ID == "AMMO");
        magazine = stats.FirstOrDefault(x => x.ID == "MAGAZINE");
    }

    public override void Equip()
    {
        
    }

    public override void Reload()
    {
        
    }

    public override void UnEquip()
    {
        
    }

    protected override void Attack()
    {
        if (IsFiring || IsReload || ammo.IsMin)
            return;


    }

    protected override void Impulse()
    {
        
    }

    protected override void Particle()
    {
        
    }

    protected override void Sound()
    {
        
    }
}
