using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class MovingComponent : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 3.0f;
    [SerializeField] private float jumpForce = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Tech Settings")]
    [SerializeField] private float sensitivity = 100.0f;
    [SerializeField] private float deadZone = 0.01f;

    private Vector2 velocity;
    private Vector2 inputMove;
    private Vector2 currentInputMove;
    private Vector3 verticalVelocity = Vector3.zero;

    [Header("Cover Settings")]
    [SerializeField] private float coverRadius = 0.5f;
    [SerializeField] private LayerMask coverLayer;
    [SerializeField] private float coverDetectionTime = 0.5f;

    private float coverTimer = 0f;

    private Quaternion cameraRotation;

    private CharacterController controller;
    private Animator animator;
    private CameraComponent cameraComponent;  // CameraComponent 참조 추가

    public bool bCanMove { get; private set; } = true;
    public bool bRun { get; private set; }
    public bool bAlt { get; private set; }
    public bool bJump { get; private set; }
    public bool bCover { get; private set; }

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        cameraComponent = FindObjectOfType<CameraComponent>();  // CameraComponent 찾기
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        PlayerInput input = GetComponent<PlayerInput>();

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
    }

    public void Update()
    {
        Movement();
        Jump();
        Gravity();
        Cover();
    }

    public void LateUpdate()
    {
        LateUpdate_Rotate();
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

    #endregion

    public void Movement()
    {
        if (!bCanMove) return; // 움직일 수 있는 상태가 아니면 종료

        // 입력 받은 값으로 현재 이동값을 부드럽게 계산 (마우스 이동에 따른 값)
        currentInputMove = Vector2.SmoothDamp(currentInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        Vector3 characterDirection = Vector3.zero; // 캐릭터의 이동 방향
        float speed = bRun ? runSpeed : walkSpeed; // 달리기와 걷기 속도 설정
        animator.SetBool("IsRun", bRun); // 애니메이션 설정

        // 카메라의 회전 정보를 가져옴
        cameraRotation = cameraComponent.GetCameraRotation();

        if (currentInputMove.magnitude > deadZone) // deadZone 이상으로 입력값이 있으면 이동 처리
        {
            //// 카메라 방향을 기준으로 전후, 좌우 방향을 계산
            //Vector3 cameraForward = cameraRotation * Vector3.forward;
            //Vector3 cameraRight = cameraRotation * Vector3.right;

            //cameraForward.y = 0; // 수평 이동만 고려
            //cameraRight.y = 0;

            //cameraForward.Normalize();
            //cameraRight.Normalize();

            //// 입력된 이동 방향에 따라 실제 이동 벡터 계산
            //characterDirection = (cameraRight * currentInputMove.x) + (cameraForward * currentInputMove.y);
            //characterDirection = characterDirection.normalized * speed;

            //// Alt 키가 눌리지 않은 경우에만 회전 적용
            //if (!bAlt)
            //{
            //    Vector3 targetDirection = characterDirection;
            //    if (targetDirection != Vector3.zero)
            //    {
            //        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f); // 부드러운 회전 적용
            //    }
            //}

            animator.SetBool("IsMove", true);
        }
        else
        {
            //// Alt 키가 눌리지 않은 경우 카메라 방향을 캐릭터 회전에 반영
            //if (!bAlt)
            //{
            //    transform.rotation = Quaternion.Euler(0, cameraRotation.eulerAngles.y, 0);
            //}

            animator.SetBool("IsMove", false);
        }

        // 캐릭터의 움직임 적용
        Vector3 move = characterDirection * Time.deltaTime; // 이동 벡터에 Time.deltaTime 곱하기
        move.y = verticalVelocity.y * Time.deltaTime; // 중력 및 점프 값 적용

        // 여기서 캐릭터가 실제로 이동할 수 있도록 Move 호출
        controller.Move(move); // CharacterController를 통해 실제로 움직임 적용
    }


    private void LateUpdate_Rotate()
    {
        if (cameraComponent == null)
            return;

        float speed = bRun ? runSpeed : walkSpeed; // 달리기와 걷기 속도 설정
        if (currentInputMove.magnitude > deadZone) // deadZone 이상으로 입력값이 있으면 이동 처리
        {
            Vector3 characterDirection = Vector3.zero;

            // 카메라 방향을 기준으로 전후, 좌우 방향을 계산
            Vector3 cameraForward = cameraRotation * Vector3.forward;
            Vector3 cameraRight = cameraRotation * Vector3.right;

            cameraForward.y = 0; // 수평 이동만 고려
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            // 입력된 이동 방향에 따라 실제 이동 벡터 계산
            characterDirection = (cameraRight * currentInputMove.x) + (cameraForward * currentInputMove.y);
            characterDirection = characterDirection.normalized * speed;

            // Alt 키가 눌리지 않은 경우에만 회전 적용
            if (!bAlt)
            {
                Vector3 targetDirection = characterDirection;
                if (targetDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f); // 부드러운 회전 적용
                }
            }

        }
        else
        {
            // Alt 키가 눌리지 않은 경우 카메라 방향을 캐릭터 회전에 반영
            if (!bAlt)
            {
                Quaternion cameraRotation = cameraComponent.GetCameraRotation();
                transform.rotation = Quaternion.Euler(0, cameraRotation.eulerAngles.y, 0);

            }
        }

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
        if (bCover)
        {
            bCover = false;
            animator.SetBool("Cover", false);
        }

        if (bJump && controller.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetTrigger("IsJump");
            bJump = false;
        }
    }

    public void Gravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
    }

    private void Cover()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, coverRadius, coverLayer);

        if (hitColliders.Length > 0)
        {
            coverTimer += Time.deltaTime;

            if (coverTimer >= coverDetectionTime)
            {
                bCover = true;
                animator.SetBool("Cover", true);
            }
        }
        else
        {
            coverTimer = 0f;
            bCover = false;
            animator.SetBool("Cover", false);
        }
    }

#if UNITY_EDITOR
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
