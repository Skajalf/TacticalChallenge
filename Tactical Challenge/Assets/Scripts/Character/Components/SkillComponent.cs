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
    StateComponent state;
    private InputAction Attack;
    private StatComponent playerStat;
    private bool isTargetingMode = false; // Ÿ���� ��� ����
    private bool isSkillReady = false; // ��ų �غ� �Ϸ� ����
    private SkillObject currentSkillObject; // ���� ��ų ��ü

    public void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        playerStat = GetComponent<StatComponent>(); // �÷��̾��� StatComponent�� ����

        weapon = transform.FindChildByName("Bip001_Weapon")?.gameObject;

        if (weapon == null)
        {
            Debug.LogError("Weapon not found! Make sure the weapon is named 'Bip001_Weapon'.");
        }

        if (!(input = GetComponent<PlayerInput>()))
            this.AddComponent<PlayerInput>();

        Init();
    }

    private void Init() // SkillComponent Init();
    {
        actionMap = input.actions.FindActionMap("Player");

        Skill = actionMap.FindAction("Skill");
        Skill.started += startSkill;

        EXSkill = actionMap.FindAction("EXSkill");
        EXSkill.started += startEXSkill;

        MeleeAttack = actionMap.FindAction("MeleeAttack");
        MeleeAttack.started += startMeleeAttack;

        Attack = actionMap.FindAction("Attack"); // Attack �׼� ��������
        Attack.started += OnMouseClick;
    }

    private void Update() //Ÿ���� ��ų�� ���콺 Ŀ���� ��ġ�� ��� ��ȯ���ִ� �κ�
    {
        if (isTargetingMode && NormalSkillObject.skillType == SkillType.Targeting)
        {
            UpdateTargetingSkill();
        }
    }

    private void startSkill(InputAction.CallbackContext context)
    {
        if (state.CurrentState == StateType.Skill || state.CurrentState == StateType.Reload)
        {
            return;
        }

        // AP üũ
        if (playerStat.CurrentAP < NormalSkillObject.APCost)
        {
            Debug.Log("Not enough AP!");
            return;
        }

        state.SetSkillMode();
        if (NormalSkillObject.skillType == SkillType.Targeting)
        {
            // Ÿ���� ��� Ȱ��ȭ
            isTargetingMode = true;
            isSkillReady = true; // ��ų �غ� �Ϸ� ���·� ����
            currentSkillObject = NormalSkillObject; // ���� ��ų ��ü ����
        }


        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("Skill");
        StartCoroutine(EndSkill());
    }

    private void startEXSkill(InputAction.CallbackContext context)
    {
        if (state.CurrentState == StateType.Skill || state.CurrentState == StateType.Reload)
        {
            return;
        }

        // AP üũ
        if (playerStat.CurrentAP < EXSkillObject.APCost)
        {
            Debug.Log("Not enough AP!");
            return;
        }

        state.SetSkillMode();

        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("EXSkill");
        StartCoroutine(EndEXSkill());
    }

    private IEnumerator EndSkill()
    {
        yield return new WaitForSeconds(NormalSkillDelay);

        if (weapon != null)
        {
            weapon.SetActive(true);
        }

        state.SetIdleMode();
    }

    private IEnumerator EndEXSkill()
    {
        yield return new WaitForSeconds(ExSkillDelay);

        if (weapon != null)
        {
            weapon.SetActive(true);
        }

        state.SetIdleMode();
    }

    private void startMeleeAttack(InputAction.CallbackContext context)
    {
        IsMeleeAttack = true;

        if (weapon != null)
        {
            weapon.SetActive(false);
        }

        animator.SetTrigger("MeleeAttack");

        StartCoroutine(EndMeleeAttack());
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

    private void UpdateTargetingSkill()
    {
        if (!isTargetingMode || NormalSkillObject.skillType != SkillType.Targeting)
            return;

        // ���콺 Ŀ���� ���� ��ġ�� ��� ���� ���� ����
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, NormalSkillObject.AttackRange, TargetingLayerMask))
        {
            Transform hitTransform = hit.transform;
            Collider[] hitColliders = Physics.OverlapSphere(hit.point, NormalSkillObject.AttackRange, TargetingLayerMask);

            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Collider collider in hitColliders)
            {
                Transform targetTransform = collider.transform;
                float distance = Vector3.Distance(hit.point, targetTransform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = targetTransform;
                }
            }

            if (closestTarget != null)
            {
                Debug.Log($"���� ����� Ÿ�� �߰�: {closestTarget.name}");

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ApplySkillToTarget(closestTarget);
                    isTargetingMode = false;
                }
            }
        }
    }

    private void ApplySkillToTarget(Transform target) // Ÿ�ٿ��� ��ų�� �����ϴ� �޼���
    {
        if (!isSkillReady)
            return;

        Debug.Log("ApplySkillToTarget called");  // �α� �߰�

        // Ÿ���� StatComponent�� ������
        StatComponent targetStat = target.GetComponent<StatComponent>();
        
        if (targetStat != null)
        {
            // AP�� ������� Ȯ��
            if (playerStat.CurrentAP >= currentSkillObject.APCost)
            {
                Debug.Log("Player has enough AP");  // �α� �߰�
                // ��ų ������ ����
                targetStat.Damage(currentSkillObject.SkillDamage);

                // AP �Ҹ�
                playerStat.APUse(currentSkillObject.APCost);  // AP ����
                Debug.Log($"Player AP after skill: {playerStat.CurrentAP}");  // �α� �߰�
            }
            else
            {
                Debug.Log("Not enough AP to use this skill.");
                return;  // AP�� �����ϸ� ��ų ������� ����
            }
        }

        if (currentSkillObject == NormalSkillObject)
        {
            animator.SetTrigger("Skill");
            StartCoroutine(EndSkill());
        }

        isSkillReady = false;
    }

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        if (isTargetingMode)
        {
            // Ÿ���� ��忡���� �̹� Ÿ���� ������ Update���� ó���ǹǷ�, ���⼭�� ���Ḹ ó��
            isTargetingMode = false;
        }
    }
}