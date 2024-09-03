using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(CapsuleCollider))]

public class Enemy : Character
{
    private StatComponent statComponent;

    private void Awake()
    {
        statComponent = GetComponent<StatComponent>();
        Init();
    }

    public override void OnDamage(float damage)
    {
        statComponent.Damage(damage);

        if (statComponent.Dead == false)
        {
            state.SetDamagedMode();

            // Impact �ִϸ��̼� Ʈ���� �� �Ķ���� ����
            //animator.SetInteger("ImpactType", (int)causer.Type); // ���� Ÿ�Կ� ���� ImpactType ���� ���߿� ��������
            animator.SetTrigger("Impact");

            // ������ ���� //Rigidbody ����̶� ���߿� �����ؾ���
            //Rigidbody rb = GetComponent<Rigidbody>();
            //rb.isKinematic = false;

            //float launch = rb.drag * data.Distance * 10.0f;
            //rb.AddForce(-transform.forward * launch);

            //StartCoroutine(Change_IsKinemetics(30));
            End_Damaged();
            return;
        }

        state.SetDeadMode();

        Collider collider = GetComponent<Collider>();
        collider.enabled = false;

        animator.SetTrigger("Dead");

        Destroy(gameObject, 5);
    }

    //private IEnumerator Change_IsKinemetics(int frame)
    //{
    //    for (int i = 0; i < frame; i++)
    //        yield return new WaitForFixedUpdate();

    //    GetComponent<Rigidbody>().isKinematic = true;
    //}

    protected override void End_Damaged()
    {
        base.End_Damaged();

        state.SetIdleMode();
    }
}