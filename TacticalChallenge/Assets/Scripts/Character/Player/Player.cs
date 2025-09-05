using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

}
