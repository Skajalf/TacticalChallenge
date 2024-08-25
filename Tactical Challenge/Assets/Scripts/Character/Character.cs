using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(CharacterController))]
[RequireComponent(typeof(StateComponent))]
//[RequireComponent(typeof(HealthPointComponent))]
[RequireComponent(typeof(WeaponComponent))]
[RequireComponent(typeof(SkillComponent))]
[RequireComponent(typeof(ActionComponent))]
public abstract class Character : MonoBehaviour, IStoppable
{
    protected Animator animator;
    protected CharacterController controller;
    protected StateComponent state;
    //protected HealthPointComponent healthPoint;
    protected WeaponComponent weapon;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        state = GetComponent<StateComponent>();
        //healthPoint = GetComponent<HealthPointComponent>();
        weapon = GetComponent<WeaponComponent>();
    }

    protected virtual void Start()
    {
        Regist_MovableStopper();
    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void OnDestroy()
    {
        Remove_MovableStopper();
    }

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

    protected virtual void End_Damaged()
    {

    }

    private void OnAnimatorMove()
    {
        transform.position += animator.deltaPosition;
    }
}