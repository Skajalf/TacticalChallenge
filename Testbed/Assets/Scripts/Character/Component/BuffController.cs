using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    List<float> floats = new List<float>();
    public void Init()
    {
        gameObject.GetComponent<StatComponent>();
    }

    public void StatAdjust(StatComponent stat, Buff buff)
    {
        
    }
}
