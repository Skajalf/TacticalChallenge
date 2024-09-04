using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovingComponent : MonoBehaviour
{
    CinemachineVirtualCamera vcam;
    Vector3 camDirection;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 3.0f;
    [SerializeField] private float jumpForce = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    //public Quaternion Rotation { set => rotation = value; }

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

    private CharacterController controller;
    private Animator animator;

    public bool bCanMove { get; private set; } = true;
    public bool bRun { get; private set; }
    public bool bAlt { get; private set; }
    public bool bJump { get; private set; }
    public bool bCover { get; private set; }

    public void Awake()
    {
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
        Cover(); // Cover 상태를 확인하는 메서드 호출
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
        Transform cameraRootTransform = transform.FindChildByName("CameraRoot").transform;

        if (!bCanMove) return;

        currentInputMove = Vector2.SmoothDamp(currentInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        Vector3 characterDirection = Vector3.zero;
        float speed = bRun ? runSpeed : walkSpeed;
        animator.SetBool("IsRun", bRun);

        // 카메라의 Y축 회전을 캐릭터의 회전에 반영
        Quaternion cameraRotation = cameraRootTransform.rotation;
        Quaternion characterRotation = Quaternion.Euler(0, cameraRotation.eulerAngles.y, 0);


        if (currentInputMove.magnitude > deadZone )
        {
            Vector3 cameraForward = cameraRootTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraRootTransform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();

            characterDirection = (cameraRight * currentInputMove.x) + (cameraForward * currentInputMove.y);
            characterDirection = characterDirection.normalized * speed;

            if (!bAlt)
            {
                transform.rotation = characterRotation;
            }

            animator.SetBool("IsMove", true);
        }
        else
        {
            // 캐릭터가 이동하지 않더라도 Alt 키가 눌리지 않은 경우에만 카메라의 Y축 회전을 따라 회전
            if (!bAlt)
            {
                transform.rotation = characterRotation;
            }

            animator.SetBool("IsMove", false);
        }

        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = 0f;
        }

        Vector3 move = characterDirection * Time.deltaTime;
        move.y = verticalVelocity.y * Time.deltaTime;

        controller.Move(move);
    }

    public void Move()
    {
        bCanMove = true;
    }

    public void Stop()
    {
        bCanMove = false;
    }

    private void Jump()
    {
        // 엄폐 상태 해제
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

    private void Gravity()
    {
        if (!controller.isGrounded)
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        else if (verticalVelocity.y < 0)
        {
            verticalVelocity.y = 0f;
        }
    }

    private void Cover()
    {
        // 플레이어 위치에서 coverRadius 반경 내의 Collider들을 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, coverRadius, coverLayer);

        // Cover 태그가 붙은 오브젝트가 감지되면
        if (hitColliders.Length > 0)
        {
            // 일정 시간 동안 Cover 상태를 유지하도록 타이머를 증가
            coverTimer += Time.deltaTime;

            if (coverTimer >= coverDetectionTime)
            {
                bCover = true;
                animator.SetBool("Cover", true);
            }
        }
        else
        {
            // Cover 상태에서 벗어나면 타이머 리셋하고 상태 해제
            coverTimer = 0f;
            bCover = false;
            animator.SetBool("Cover", false);
        }
    }
}