using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillComponent : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GameObject weapon; // Bip001_Weapon
    [SerializeField] private SkillObject EXSkillObject; // EX��ų
    [SerializeField] private SkillObject NormalSkillObject; // �븻��ų
    [SerializeField] private float ExSkillDelay;
    [SerializeField] private float NormalSkillDelay;
    [SerializeField] private LayerMask TargetingLayerMask; // Ÿ���� �����ϱ� ���� ���̾� ����ũ

    public bool IsEXSkill { private set; get; }
    public bool IsNormalSkill { private set; get; }
    public bool IsMeleeAttack { private set; get; }

    PlayerInput input;
    InputActionMap actionMap;
    InputAction Skill;
    InputAction EXSkill;
    InputAction MeleeAttack;
    private InputAction Attack;

    StateComponent state;
    private StatComponent playerStat;
    private CameraComponent cameraComponent; // �߰��� ī�޶� ������Ʈ

    private bool isTargetingMode = false; // Ÿ���� ��� ����
    private bool isSkillReady = false; // ��ų �غ� �Ϸ� ����
    private SkillObject currentSkillObject; // ���� ��ų ��ü

    private float lastSkillUseTime = 0f; // ���������� ��ų�� ����� �ð��� ����
    private float lastEXSkillUseTime = 0f; // ���������� EX ��ų�� ����� �ð��� ����

    private float originalZoomDistance; // ī�޶��� ���� zoomRange ���� ����


    public void Awake()
    {
        weapon = transform.FindChildByName("Bip001_Weapon")?.gameObject;

        if (weapon == null)
        {
            Debug.LogError("Weapon not found! Make sure the weapon is named 'Bip001_Weapon'.");
        }

        if (!(input = GetComponent<PlayerInput>()))
            this.AddComponent<PlayerInput>();

        Init();
    }

    private void Init()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        playerStat = GetComponent<StatComponent>();
        cameraComponent = GetComponent<CameraComponent>();

        actionMap = input.actions.FindActionMap("Player");

        Skill = actionMap.FindAction("Skill");
        Skill.started += context => StartSkill(NormalSkillObject, ref lastSkillUseTime, NormalSkillObject.SkillCoolTime);

        EXSkill = actionMap.FindAction("EXSkill");
        EXSkill.started += context => StartSkill(EXSkillObject, ref lastEXSkillUseTime, EXSkillObject.SkillCoolTime);

        MeleeAttack = actionMap.FindAction("MeleeAttack");
        MeleeAttack.started += StartMeleeAttack;

        Attack = actionMap.FindAction("Attack");
        Attack.started += OnMouseClick;

        originalZoomDistance = cameraComponent.currentZoomDistance;
    }

    private void StartSkill(SkillObject skillObject, ref float lastUseTime, float coolTime)
    {
        float currentTime = Time.time; // ���� �ð�

        // ��Ÿ�� Ȯ��
        if (currentTime - lastUseTime < coolTime)
        {
            Debug.Log("��ų ��Ÿ���� ������ �ʾҽ��ϴ�.");
            return;
        }

        if (!state.CanDoSomething() || !CheckStat(skillObject))
            return;

        // ��ų ��� �ð� ����
        lastUseTime = currentTime;

        // ��ų ��ü ����
        currentSkillObject = skillObject;

        // ī�޶� ���� ��ų�� �°� ����
        cameraComponent.currentZoomDistance = 4f; // ������ �� �� ����

        state.SetSkillMode();

        if (skillObject.InstantCast)
        {
            Fire();
        }
        else
        {
            Confirm();
        }
    }

    // Fire �޼��忡�� SkillManager�� ����� ����
    private void Fire()
    {
        Vector3 mousePosition = GetMouseWorldPosition();

        switch (currentSkillObject.skillType)
        {
            case SkillType.Targeting:
                TargetingSkill(mousePosition);
                break;

            case SkillType.AreaAttack:
                AreaAttackSkill(mousePosition);
                break;

            case SkillType.SelfCast:
                // SelfCast�� �ڽ��� Ÿ������ �����Ͽ� ó��
                TargetingSkill(playerStat.transform.position);
                break;
        }

        OnSkill();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            return hitInfo.point;
        }
        return Vector3.zero;
    }

    private void TargetingSkill(Vector3 targetPosition)
    {
        List<StatComponent> targets = currentSkillObject.GetTargets(targetPosition);

        if (targets.Count > 0)
        {
            StatComponent closestTarget = GetClosestTarget(targets, targetPosition);
            ApplySkillToTarget(closestTarget);
        }
        else
        {
            Debug.Log("No valid target found.");
        }
    }

    private void AreaAttackSkill(Vector3 targetPosition)
    {
        List<StatComponent> targets = currentSkillObject.GetTargets(targetPosition);
        foreach (StatComponent target in targets)
        {
            ApplySkillToTarget(target);
        }
    }

    private StatComponent GetClosestTarget(List<StatComponent> targets, Vector3 position)
    {
        StatComponent closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (StatComponent target in targets)
        {
            float distance = Vector3.Distance(position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target;
            }
        }

        return closest;
    }

    private void ApplySkillToTarget(StatComponent target)
    {
        if (!isSkillReady || target == null)
            return;

        if (playerStat.CurrentAP >= currentSkillObject.APCost)
        {
            target.Damage(currentSkillObject.SkillDamage);
            playerStat.APUse(currentSkillObject.APCost);
        }
        else
        {
            Debug.Log("Not enough AP to use this skill.");
        }
    }

    private IEnumerator EndSkill()
    {
        // ī�޶� ���� ���� ������ ����
        cameraComponent.currentZoomDistance = originalZoomDistance;
        state.SetIdleMode();

        yield return null;
    }

    private bool CheckStat(SkillObject skillObject)
    {
        if (playerStat.CurrentAP < skillObject.APCost)
        {
            Debug.Log("Not enough AP!");
            return false;
        }

        return true;
    }

    private void Confirm()
    {
        StartCoroutine(WaitForInputOrConditions());
    }

    private IEnumerator WaitForInputOrConditions()
    {
        // ��ų �غ� �Ϸ�� ������ ���� ���
        while (isSkillReady)
        {
            // ���콺 ��Ŭ���� �߻��� �� Fire �޼��� ȣ��
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Fire();
                break;
            }

            // ��ų ��尡 �ƴ� �� ����
            if (!state.SkillMode)
            {
                EndSkill();
                break;
            }

            yield return null;
        }
    }

    private void OnSkill()
    {
        if (isSkillReady)
        {
            StartCoroutine(EndSkill());
        }

        isSkillReady = false;
    }

    private void StartMeleeAttack(InputAction.CallbackContext context)
    {
        if (!state.CanDoSomething())
            return;

        IsMeleeAttack = true;

        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("MeleeAttack");

        StartCoroutine(EndMeleeAttack());
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        if (isSkillReady)
        {
            Fire(); // ��ų �غ� �Ϸ�Ǿ����� Fire �޼��带 ȣ��
        }
    }

    private IEnumerator EndMeleeAttack()
    {
        yield return new WaitForSeconds(1.0f);

        if (weapon != null)
        {
            weapon.SetActive(true);
        }

        IsMeleeAttack = false;
    }
}