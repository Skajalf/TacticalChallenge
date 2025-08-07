using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RootMotion.FinalIK;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class MovingComponent : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 3.0f;
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Tech Settings")]
    [SerializeField] private float sensitivity = 100.0f;
    [SerializeField] private float deadZone = 0.01f;
    [SerializeField] private float rotationSmooth = 20f;

    [Header("Cover Settings")]
    [SerializeField] private LayerMask coverLayer;
    [SerializeField] private float coverRadius = 0.5f;
    [SerializeField] private float coverDetectionTime = 0.5f;
    private Obstacles nearbyCoverObstacle = null;

    [Header("Pakour Settings")]
    [SerializeField] private float parkourDistance = 1.0f;
    [SerializeField] private float parkourHeight = 0.3f;
    [SerializeField] private float parkourDuration = 0.5f;

    private float currentParkourDistance;
    private float currentParkourHeight;

    [Header("Ground Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, 0.05f, 0);
    [SerializeField] private Vector3 groundCheckSize = new Vector3(0.5f, 0.1f, 0.5f);

    private Vector2 velocity;
    private Vector2 inputMove;
    private Vector2 currentInputMove;
    private Vector3 verticalVelocity = Vector3.zero;
    private Quaternion cameraRotation;

    private Collider currentCoverCollider;
    private Vector3 coverNormal;

    private float coverTimer = 0f;

    private CharacterController characterController;
    private Animator animator;
    private CameraComponent cameraComponent;  // CameraComponent 참조 추가
    private Transform cameraRootTransform;
    private AimIK aimIK;

    public bool bCanMove { get; private set; } = true;
    public bool bRun { get; private set; }
    public bool bAlt { get; private set; }
    public bool bJump { get; private set; }
    public bool bGrounded { get; private set; }
    public bool bCover { get; private set; }

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        cameraComponent = FindObjectOfType<CameraComponent>();  // CameraComponent 찾기

        characterController = GetComponent<CharacterController>();

        animator = GetComponent<Animator>();

        cameraRootTransform = transform.FindChildByName("CameraRoot").transform;

        PlayerInput input = GetComponent<PlayerInput>();
        aimIK = GetComponent<AimIK>();

        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction move = actionMap.FindAction("Move");
        move.performed += performMove;
        move.canceled += cancelMove;

        InputAction run = actionMap.FindAction("Run");
        run.started += startRun;
        run.canceled += cancelRun;

        InputAction jump = actionMap.FindAction("Jump");
        jump.started += startJump;

        InputAction alt = actionMap.FindAction("Alt");
        alt.started += startAlt;
        alt.canceled += cancelAlt;

        InputAction cover = actionMap.FindAction("Cover");
        cover.started += Togglecover;
    }

    private void OnEnable()
    {
        Obstacles.OnCoverAvailable += HandleCoverAvailable;
        Obstacles.OnCoverUnavailable += HandleCoverUnavailable;
    }

    private void OnDisable()
    {
        Obstacles.OnCoverAvailable -= HandleCoverAvailable;
        Obstacles.OnCoverUnavailable -= HandleCoverUnavailable;
    }

    private void HandleCoverAvailable(Obstacles obstacle, Transform player)
    {
        if (player != transform) return;
        nearbyCoverObstacle = obstacle;
        Debug.Log("[커버 진입 가능 지점 감지됨]");
    }

    private void HandleCoverUnavailable(Obstacles obstacle, Transform player)
    {
        if (player != transform) return;
        if (nearbyCoverObstacle == obstacle)
        {
            nearbyCoverObstacle = null;
            Debug.Log("[커버 진입 지점 이탈]");
        }
    }

    public void Update()
    {
        CheckGrounded();
    }

    public void FixedUpdate()
    {
        if (bCover)
        {
            verticalVelocity = Vector3.zero;

            Jump();

            return;
        }

        Jump();
        Gravity();
        Movement();
    }

    public void LateUpdate()
    {
        Rotation();
    }

    #region actionMethods
    private void performMove(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
    }

    private void cancelMove(InputAction.CallbackContext context)
    {
        inputMove = Vector2.zero;
    }

    private void startRun(InputAction.CallbackContext context)
    {
        bRun = true;
    }

    private void cancelRun(InputAction.CallbackContext context)
    {
        bRun = false;
    }

    private void startJump(InputAction.CallbackContext context)
    {
        bJump = true;
    }

    private void startAlt(InputAction.CallbackContext context)
    {
        bAlt = true;
    }

    private void cancelAlt(InputAction.CallbackContext context)
    {
        bAlt = false;
    }

    private void Togglecover(InputAction.CallbackContext context)
    {
        if (bCover)
        {
            ExitCover();
        }
        else if (nearbyCoverObstacle != null)
        {
            CanCover();
        }
    }

    #endregion

    public void Movement()
    {
        if (!bCanMove) return;

        // 1) 입력 부드럽게
        currentInputMove = Vector2.SmoothDamp(currentInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        // 2) 카메라 기준 방향
        Vector3 cameraForward = cameraRootTransform.forward; cameraForward.y = 0; cameraForward.Normalize();
        Vector3 cameraRight = cameraRootTransform.right; cameraRight.y = 0; cameraRight.Normalize();

        // 3) 수평 이동 벡터 계산
        Vector3 characterDirection = Vector3.zero;
        float speed = bRun ? runSpeed : walkSpeed;
        if (currentInputMove.magnitude > deadZone)
        {
            characterDirection = (cameraRight * currentInputMove.x + cameraForward * currentInputMove.y).normalized * speed;
            animator.SetBool("IsMove", true);
        }
        else
        {
            animator.SetBool("IsMove", false);
        }
        animator.SetBool("IsRun", bRun);

        // 4) 중력 포함한 실제 이동
        Vector3 move = characterDirection + verticalVelocity;

        // 5) CharacterController로 이동 적용 (deltaTime 반영)
        characterController.Move(move * Time.deltaTime);
    }


    public void Rotation()
    {
        if (bAlt) return;

        // 1) 목표 회전(카메라 Yaw)
        float targetYaw = cameraRootTransform.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, targetYaw, 0f);

        // 2) 부드럽게 보간해서 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSmooth);
    }

    public void Move()
    {
        bCanMove = true;
    }

    public void Stop()
    {
        bCanMove = false;
    }

    public void Jump()
    {
        if (!bJump) return;

        // 커버 중이거나 커버 근처에 있을 때는 파쿠르 시도
        if (bCover || nearbyCoverObstacle != null)
        {
            Parkour();
            bJump = false;
            return;
        }

        if (bGrounded)
        {
            verticalVelocity.y = jumpForce;
            animator.SetTrigger("IsJump");
        }

        bJump = false;
    }

    public void Gravity()
    {
        if (bGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
    }

    private void CheckGrounded()
    {
        Vector3 center = transform.position + groundCheckOffset;
        Vector3 halfExtents = groundCheckSize * 0.5f;
        bGrounded = Physics.CheckBox(
            center,
            halfExtents,
            Quaternion.identity,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void CanCover()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = transform.forward;
        dir.y = 0f;
        dir.Normalize();

        if (Physics.Raycast(origin, dir, out hit, coverRadius, coverLayer))
        {
            // 기존 세팅
            bCover = true;
            bCanMove = false;
            animator.SetBool("Cover", true);
            aimIK.enabled = false;

            currentCoverCollider = hit.collider;
            coverNormal = hit.normal;

            float obsHeight = hit.collider.bounds.max.y - transform.position.y;
            float obsDepth = Vector3.Dot(hit.collider.bounds.size, coverNormal);

            currentParkourDistance = Mathf.Max(parkourDistance, obsDepth + 0.1f);
            currentParkourHeight = Mathf.Max(parkourHeight, obsHeight + 0.1f);
        }
    }

    private void ExitCover()
    {
        // 커버 해제
        bCover = false;
        bCanMove = true;
        animator.SetBool("Cover", false);

        aimIK.enabled = true;
        //cameraComponent?.SetCoverMode(false);
    }

    private void Parkour()
    {
        // 엄폐 상태인 경우 → 이미 currentCoverCollider 있음
        if (bCover)
        {
            if (currentCoverCollider == null) { ExitCover(); return; }
        }
        else if (nearbyCoverObstacle != null)
        {
            // 엄폐 상태는 아니지만 엄폐물이 근처에 있을 경우 → Raycast로 확인
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 0.5f;

            // 카메라 XZ 기반이 아니라 캐릭터 기준 전방
            Vector3 dir = transform.forward;
            dir.y = 0f; // 완전히 수평 방향
            dir.Normalize();

            if (Physics.Raycast(origin, dir, out hit, coverRadius, coverLayer))
            {
                currentCoverCollider = hit.collider;
                coverNormal = hit.normal;

                float obsHeight = hit.collider.bounds.max.y - transform.position.y;
                float obsDepth = Vector3.Dot(hit.collider.bounds.size, coverNormal);

                currentParkourDistance = Mathf.Max(parkourDistance, obsDepth + 0.1f);
                currentParkourHeight = Mathf.Max(parkourHeight, obsHeight + 0.1f);
            }
            else
            {
                return; // 엄폐물 근처지만 앞에 아무것도 없음
            }
        }
        else
        {
            return; // 엄폐 상태도 아니고, nearbyCoverObstacle도 없음
        }

        // 파쿠르 실행
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - coverNormal * currentParkourDistance;

        animator.SetTrigger("IsJump");
        StartCoroutine(ParkourRoutine(startPos, endPos, parkourDuration));
    }


    private IEnumerator ParkourRoutine(Vector3 from, Vector3 to, float duration)
    {
        // 1) 파쿠르 시작 전
        characterController.enabled = false;
        bCover = false;
        bCanMove = false;
        animator.SetBool("Cover", false);
        aimIK.enabled = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 horizontal = Vector3.Lerp(from, to, t);
            float arc = Mathf.Sin(Mathf.PI * t) * currentParkourHeight;
            horizontal.y = Mathf.Lerp(from.y, to.y, t) + arc;
            transform.position = horizontal;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2) 최종 위치 & 복원
        transform.position = to;
        characterController.enabled = true;

        bCanMove = true;
        currentCoverCollider = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw ground check box
        Gizmos.color = Color.green;
        Vector3 center = transform.position + groundCheckOffset;
        Gizmos.DrawWireCube(center, groundCheckSize);
        // Draw cover radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, coverRadius);

        if (currentCoverCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + coverNormal);
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 120, 300, 100));
        var textStyle = new GUIStyle();
        textStyle.normal.textColor = Color.yellow;
        GUILayout.Label(cameraComponent.GetCameraRotation().eulerAngles.ToString(), textStyle);
        GUILayout.Label(transform.rotation.eulerAngles.ToString(), textStyle);
        GUILayout.EndArea();
    }

#endif
}