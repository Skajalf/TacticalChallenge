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

    [Header("Ground Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, 0.05f, 0);
    [SerializeField] private Vector3 groundCheckSize = new Vector3(0.5f, 0.1f, 0.5f);

    private Vector2 velocity;
    private Vector2 inputMove;
    private Vector2 currentInputMove;
    private Vector3 verticalVelocity = Vector3.zero;
    private Quaternion cameraRotation;

    private float coverTimer = 0f;

    private Rigidbody rigidbodyController;
    private Animator animator;
    private CameraComponent cameraComponent;  // CameraComponent 참조 추가
    private Transform cameraRootTransform;

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
        
        rigidbodyController = GetComponent<Rigidbody>();
        rigidbodyController.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponent<Animator>();

        cameraRootTransform = transform.FindChildByName("CameraRoot").transform;

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
        CheckGrounded();
        
        Cover();
        Gravity();
    }

    public void FixedUpdate()
    {
        Movement();
        Jump();
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

        // 4) 실제 이동
        Vector3 move = characterDirection * Time.fixedDeltaTime + verticalVelocity * Time.fixedDeltaTime;
        rigidbodyController.MovePosition(rigidbodyController.position + move);
    }

    public void Rotation()
    {
        if (!bCanMove || bAlt) return;

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
        if (bCover)
        {
            bCover = false;
            animator.SetBool("Cover", false);
        }

        if (bJump && bGrounded)
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
    private void OnDrawGizmosSelected()
    {
        // Draw ground check box
        Gizmos.color = Color.green;
        Vector3 center = transform.position + groundCheckOffset;
        Gizmos.DrawWireCube(center, groundCheckSize);
        // Draw cover radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, coverRadius);
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
