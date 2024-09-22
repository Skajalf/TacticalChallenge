using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class Character : MonoBehaviour
{
    protected Animator animator;
    protected StateComponent state;
    protected StatComponent stat;

    // protected AudioSource audioSource;

    public void Init()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        stat = GetComponent<StatComponent>();
    }
}
