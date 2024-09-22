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
