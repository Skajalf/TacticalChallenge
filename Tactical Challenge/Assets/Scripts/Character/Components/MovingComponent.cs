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

    private CharacterController controller;
    private Animator animator;

    public bool bCanMove { get; private set; } = true;
    public bool bRun { get; private set; }
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
    }

    public void Update()
    {
        Movement();
        Jump();
        Gravity();
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

    #endregion

    public void Movement()
    {
        Transform cameraRootTransform = transform.FindChildByName("CameraRoot").transform;

        if (!bCanMove) return;

        currentInputMove = Vector2.SmoothDamp(currentInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        Vector3 characterDirection = Vector3.zero;
        float speed = bRun ? runSpeed : walkSpeed;
        animator.SetBool("IsRun", bRun);

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

            if(speed != 0) // 카메라 고정을 위한 것
            {
                transform.rotation = Quaternion.LookRotation(cameraForward);
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(characterDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * sensitivity);
            }
            animator.SetBool("IsMove", true);
        }
        else
        {
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
}