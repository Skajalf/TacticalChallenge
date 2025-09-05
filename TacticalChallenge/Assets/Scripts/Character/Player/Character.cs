using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public abstract class Character : MonoBehaviour
{
    protected Animator animator;
    protected StateComponent state;

    // protected AudioSource audioSource;

    public void Init()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
    }

    

}
