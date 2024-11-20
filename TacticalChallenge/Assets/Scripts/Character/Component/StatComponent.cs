using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(StateComponent))]

public class StatComponent : MonoBehaviour
{
    // stat component의 요구사항
    // 1. 이 Object가 갖고 있는 Stat을 배열로 가지고 있을 것.
    // 2. Stat을 조작하는 인터페이스를 제공할 것.

    [Header("Field")]
    [SerializeField] private Stat HP;
    [SerializeField] private Stat AP;
    
    [SerializeField] private Stat defence; 
    [SerializeField] private Stat criticalRate; 

    [Header("Option")]
    [SerializeField] private float apRegenTime;
    [SerializeField] private float apRegenAmount;

    private Stat[] stats;

    public Character Owner { get; private set; }
    public Stat HPstat { get; private set; }
    public Stat APstat { get; private set; }

    public void Init()
    {
        Owner = gameObject.GetComponent<Player>();

        HPstat = HP ? GetStat(HP) : null;
        APstat = AP ? GetStat(AP) : null;

    }

    public Stat GetStat(Stat stat)
    {
        return stats.FirstOrDefault(x => x.ID == stat.ID);
    }


    private void OnDestroy()
    {
        foreach (var stat in stats)
            Destroy(stat);
        stats = null;
    }

}
