using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StatComponent))]
public class Player : Character
{

    public void Awake()
    {
        base.Init();
        Init();
    }

    public void Update()
    {
        APRegenTest();
    }

    
    private void APRegenTest()
    {
        stat.GetStat("AP").DefaultValue += 0.1f;
    }

}
