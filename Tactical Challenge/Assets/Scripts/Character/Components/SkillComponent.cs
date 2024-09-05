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
    StateComponent state;
    private InputAction Attack;
    private StatComponent playerStat;
    private bool isTargetingMode = false; // 타겟팅 모드 여부
    private bool isSkillReady = false; // 스킬 준비 완료 여부
    private SkillObject currentSkillObject; // 현재 스킬 객체

    public void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        playerStat = GetComponent<StatComponent>(); // 플레이어의 StatComponent를 참조

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

        Attack = actionMap.FindAction("Attack"); // Attack 액션 가져오기
        Attack.started += OnMouseClick;
    }

    private void Update() //타게팅 스킬의 마우스 커서의 위치를 계속 반환해주는 부분
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

        // AP 체크
        if (playerStat.CurrentAP < NormalSkillObject.APCost)
        {
            Debug.Log("Not enough AP!");
            return;
        }

        state.SetSkillMode();
        if (NormalSkillObject.skillType == SkillType.Targeting)
        {
            // 타겟팅 모드 활성화
            isTargetingMode = true;
            isSkillReady = true; // 스킬 준비 완료 상태로 설정
            currentSkillObject = NormalSkillObject; // 현재 스킬 객체 설정
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

        // AP 체크
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

        // 마우스 커서의 월드 위치를 얻기 위해 레이 생성
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
                Debug.Log($"가장 가까운 타겟 발견: {closestTarget.name}");

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ApplySkillToTarget(closestTarget);
                    isTargetingMode = false;
                }
            }
        }
    }

    private void ApplySkillToTarget(Transform target) // 타겟에게 스킬을 적용하는 메서드
    {
        if (!isSkillReady)
            return;

        Debug.Log("ApplySkillToTarget called");  // 로그 추가

        // 타겟의 StatComponent를 가져옴
        StatComponent targetStat = target.GetComponent<StatComponent>();
        
        if (targetStat != null)
        {
            // AP가 충분한지 확인
            if (playerStat.CurrentAP >= currentSkillObject.APCost)
            {
                Debug.Log("Player has enough AP");  // 로그 추가
                // 스킬 데미지 적용
                targetStat.Damage(currentSkillObject.SkillDamage);

                // AP 소모
                playerStat.APUse(currentSkillObject.APCost);  // AP 차감
                Debug.Log($"Player AP after skill: {playerStat.CurrentAP}");  // 로그 추가
            }
            else
            {
                Debug.Log("Not enough AP to use this skill.");
                return;  // AP가 부족하면 스킬 사용하지 않음
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
            // 타겟팅 모드에서는 이미 타겟팅 로직이 Update에서 처리되므로, 여기서는 종료만 처리
            isTargetingMode = false;
        }
    }
}