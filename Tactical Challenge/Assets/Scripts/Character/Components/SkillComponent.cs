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

    public bool IsEXSkill { private set; get; }
    public bool IsNormalSkill { private set; get; }
    public bool IsMeleeAttack { private set; get; }

    PlayerInput input;
    InputActionMap actionMap;
    InputAction Skill;
    InputAction EXSkill;
    InputAction MeleeAttack;

    StateComponent state;
    private StatComponent playerStat;
    private CameraComponent cameraComponent;

    private SkillObject currentSkillObject;
    private float lastSkillUseTime = 0f;
    private float lastEXSkillUseTime = 0f;

    private float originalZoomDistance;

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

        // ��ų�� EX��ų�� ������ ������� ó��
        Skill = actionMap.FindAction("Skill");
        EXSkill = actionMap.FindAction("EXSkill");

        MeleeAttack = actionMap.FindAction("MeleeAttack");

        // InputAction�� �޼��� ����
        Skill.started += context => UsingSkill(NormalSkillObject, ref lastSkillUseTime);
        EXSkill.started += context => UsingSkill(EXSkillObject, ref lastEXSkillUseTime);
        MeleeAttack.started += StartMeleeAttack;

        originalZoomDistance = cameraComponent.currentZoomDistance;
    }

    // ��ų ��� ���� ���� Ȯ�� �� ��ų ����
    private void UsingSkill(SkillObject skillObject, ref float lastUseTime)
    {
        if (CheckStat(skillObject) && Time.time - lastUseTime >= skillObject.SkillCoolTime)
        {
            Debug.Log($"UsingSkill ȣ�� : ���� ���� {state.CurrentState}");
            lastUseTime = Time.time;
            state.SetSkillMode();

            // SkillObject���� ��ų ����
            skillObject.ExecuteSkill(gameObject);
        }
        else
        {
            Debug.Log("��ų�� ����� �� �����ϴ�.");
        }

        state.SetIdleMode();
    }

    // AP�� ���� ���� üũ (���� ���ǵ�� ����)
    private bool CheckStat(SkillObject skillObject)
    {
        if (playerStat.CurrentAP < skillObject.APCost)
        {
            Debug.Log("AP�� �����մϴ�!");
            return false;
        }

        if (!state.CanDoSomething())
        {
            Debug.Log($"���� ���·δ� ��ų�� ����� �� �����ϴ�. ���� ���� : {state.CurrentState}");
            return false;
        }

        return true;
    }

    // ���� ���� ����
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

    // ���� ���� ����
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
