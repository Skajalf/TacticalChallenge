using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StatComponent))]
[RequireComponent(typeof(StateComponent))]
public class Character : MonoBehaviour, IStoppable
{
    protected Animator animator; 
    protected StateComponent state;
    protected WeaponComponent weapon;
    protected StatComponent stat;

    public virtual void Init()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        weapon = GetComponent<WeaponComponent>();
        stat = GetComponent<StatComponent>();
    }

    public virtual void OnDamage(float damage)
    {

    }

    protected virtual void End_Damaged()
    {

    }

    #region IStoppable
    public void Regist_MovableStopper()
    {
        MovableStopper.Instance.Regist(this);
    }

    public void Remove_MovableStopper()
    {
        MovableStopper.Instance.Remove(this);
    }

    public IEnumerator Start_FrameDelay(int frame)
    {
        animator.speed = 0.0f;

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        animator.speed = 1.0f;
    }
    #endregion
}
