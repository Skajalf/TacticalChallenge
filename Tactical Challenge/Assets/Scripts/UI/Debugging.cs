using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Debugging : MonoBehaviour
{
    ActionComponent component;
    GameObject obj;
    private bool isAimTrack;

    public void Awake()
    {
        obj = GetComponent<GameObject>();
        component = GameObject.Find("CH0137").GetComponent<ActionComponent>();
    }

    public void Update()
    {

    }
}
