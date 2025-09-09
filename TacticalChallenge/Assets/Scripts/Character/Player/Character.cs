using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public abstract class Character : MonoBehaviour
{
    protected Animator animator;
    protected StateComponent state;
    protected StatComponent stat;
    // protected AudioSource audioSource;

    [SerializeField] protected float APRegen = 0.5f;

    public StatComponent Stat => stat;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        stat = GetComponent<StatComponent>();
    }

    protected virtual void Update()
    {
        if (stat != null && stat.CurrentAP < stat.MaxAP && APRegen > 0f)
        {
            stat.RecoverAP(APRegen * Time.deltaTime);
        }
    }

    public virtual bool GetDamage(float amount, GameObject giver = null)
    {
        if (stat == null)
            return false;

        return stat.TakeDamage(amount, giver);
    }

    public virtual bool UseAP(float cost)
    {
        if (stat == null)
            return false;

        return stat.UseAP(cost);
    }

    public virtual void RecoverAP(float amount)
    {
        if (stat == null)
            return;

        stat.RecoverAP(amount);
    }
}