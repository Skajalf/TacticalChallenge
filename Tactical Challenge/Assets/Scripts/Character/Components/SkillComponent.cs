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
    [SerializeField] private float ExSkillDelay;
    [SerializeField] private float NormalSkillDelay;
    [SerializeField] private LayerMask TargetingLayerMask; // 타겟팅 감지하기 위한 레이어 마스크

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
    private CameraComponent cameraComponent; // 추가된 카메라 컴포넌트

    private bool isTargetingMode = false; // 타겟팅 모드 여부
    private bool isSkillReady = false; // 스킬 준비 완료 여부
    private SkillObject currentSkillObject; // 현재 스킬 객체

    private float lastSkillUseTime = 0f; // 마지막으로 스킬을 사용한 시간을 저장
    private float lastEXSkillUseTime = 0f; // 마지막으로 EX 스킬을 사용한 시간을 저장

    private float originalZoomDistance; // 카메라의 원래 zoomRange 값을 저장


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
        float currentTime = Time.time; // 현재 시간

        // 쿨타임 확인
        if (currentTime - lastUseTime < coolTime)
        {
            Debug.Log("스킬 쿨타임이 끝나지 않았습니다.");
            return;
        }

        if (!state.CanDoSomething() || !CheckStat(skillObject))
            return;

        // 스킬 사용 시간 갱신
        lastUseTime = currentTime;

        // 스킬 객체 설정
        currentSkillObject = skillObject;

        // 카메라 줌을 스킬에 맞게 조정
        cameraComponent.currentZoomDistance = 4f; // 고정된 줌 값 설정

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

    // Fire 메서드에서 SkillManager의 기능을 통합
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
                // SelfCast는 자신을 타겟으로 설정하여 처리
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
        // 카메라 줌을 원래 값으로 복구
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
        // 스킬 준비가 완료된 상태일 때만 대기
        while (isSkillReady)
        {
            // 마우스 좌클릭이 발생할 때 Fire 메서드 호출
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Fire();
                break;
            }

            // 스킬 모드가 아닐 때 종료
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
            Fire(); // 스킬 준비가 완료되었으면 Fire 메서드를 호출
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