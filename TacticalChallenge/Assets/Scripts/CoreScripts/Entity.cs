using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private string ID;
    private string instanceID;

    public string GetID() => ID;
    public string SetInstanceID(string name) => instanceID = name;

}
