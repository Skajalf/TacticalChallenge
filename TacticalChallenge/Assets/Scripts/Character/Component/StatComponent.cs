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
    [SerializeField] private StatOverride[] statOverrides;

    private Stat[] stats;

    public Character Owner { get; private set; }

    public void Init()
    {
        Owner = gameObject.GetComponent<Character>();

        stats = statOverrides.Select(x => x.CreateStat()).ToArray();
        
    }

    public Stat GetStat(Stat stat) => stats.FirstOrDefault(x => x.ID == stat.ID);
    public Stat GetStat(string ID) => stats.FirstOrDefault(x => x.ID == ID);
    public float GetValue(Stat stat) => GetStat(stat).Value;
    public bool HasStat(Stat stat) => stats.Any(x => x.ID == stat.ID);


    


    private void OnDestroy()
    {
        foreach (var stat in stats)
            Destroy(stat);
        stats = null;
    }

}
