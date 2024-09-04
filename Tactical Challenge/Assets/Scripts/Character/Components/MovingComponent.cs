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
        Cover(); // Cover ���¸� Ȯ���ϴ� �޼��� ȣ��
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

        // ī�޶��� Y�� ȸ���� ĳ������ ȸ���� �ݿ�
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
            // ĳ���Ͱ� �̵����� �ʴ��� Alt Ű�� ������ ���� ��쿡�� ī�޶��� Y�� ȸ���� ���� ȸ��
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
        // ���� ���� ����
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
        // �÷��̾� ��ġ���� coverRadius �ݰ� ���� Collider���� ����
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, coverRadius, coverLayer);

        // Cover �±װ� ���� ������Ʈ�� �����Ǹ�
        if (hitColliders.Length > 0)
        {
            // ���� �ð� ���� Cover ���¸� �����ϵ��� Ÿ�̸Ӹ� ����
            coverTimer += Time.deltaTime;

            if (coverTimer >= coverDetectionTime)
            {
                bCover = true;
                animator.SetBool("Cover", true);
            }
        }
        else
        {
            // Cover ���¿��� ����� Ÿ�̸� �����ϰ� ���� ����
            coverTimer = 0f;
            bCover = false;
            animator.SetBool("Cover", false);
        }
    }
}