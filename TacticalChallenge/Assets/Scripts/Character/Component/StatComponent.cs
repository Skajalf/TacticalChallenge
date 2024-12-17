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
    // stat component�� �䱸����
    // 1. �� Object�� ���� �ִ� Stat�� �迭�� ������ ���� ��.
    // 2. Stat�� �����ϴ� �������̽��� ������ ��.

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
