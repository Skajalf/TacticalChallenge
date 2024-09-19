using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillComponent : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GameObject weapon; // Bip001_Weapon
    [SerializeField] private SkillObject EXSkillObject; // EX스킬
    [SerializeField] private SkillObject NormalSkillObject; // 노말스킬

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

        // 스킬과 EX스킬을 동일한 방식으로 처리
        Skill = actionMap.FindAction("Skill");
        EXSkill = actionMap.FindAction("EXSkill");

        MeleeAttack = actionMap.FindAction("MeleeAttack");

        // InputAction에 메서드 연결
        Skill.started += context => UsingSkill(NormalSkillObject, ref lastSkillUseTime);
        EXSkill.started += context => UsingSkill(EXSkillObject, ref lastEXSkillUseTime);
        MeleeAttack.started += StartMeleeAttack;

        originalZoomDistance = cameraComponent.currentZoomDistance;
    }

    // 스킬 사용 가능 여부 확인 후 스킬 시전
    private void UsingSkill(SkillObject skillObject, ref float lastUseTime)
    {
        if (CheckStat(skillObject) && Time.time - lastUseTime >= skillObject.SkillCoolTime)
        {
            Debug.Log($"UsingSkill 호출 : 현재 상태 {state.CurrentState}");
            lastUseTime = Time.time;
            state.SetSkillMode();

            // SkillObject에서 스킬 실행
            skillObject.ExecuteSkill(gameObject);
        }
        else
        {
            Debug.Log("스킬을 사용할 수 없습니다.");
        }

        state.SetIdleMode();
    }

    // AP와 같은 상태 체크 (이전 조건들과 통합)
    private bool CheckStat(SkillObject skillObject)
    {
        if (playerStat.CurrentAP < skillObject.APCost)
        {
            Debug.Log("AP가 부족합니다!");
            return false;
        }

        if (!state.CanDoSomething())
        {
            Debug.Log($"현재 상태로는 스킬을 사용할 수 없습니다. 현재 상태 : {state.CurrentState}");
            return false;
        }

        return true;
    }

    // 근접 공격 시작
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

    // 근접 공격 종료
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
