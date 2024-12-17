using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class StatOverride
{
    [SerializeField] private Stat stat;
    [SerializeField] private bool isUseOverride;
    [SerializeField] private bool MakeMaxValueEqual;
    [SerializeField] private float overrideDefaultValue;


    public StatOverride(Stat stat)
        => this.stat = stat;

    public Stat CreateStat()
    {
        var newStat = stat.Clone() as Stat;
        if (isUseOverride)
        {
            newStat.DefaultValue = overrideDefaultValue;
            if(MakeMaxValueEqual)
                newStat.MaxValue = overrideDefaultValue;
        }
            
        return newStat;
    }
}
