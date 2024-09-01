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
    private Vector2 currInputMove;
    private Vector3 verticalVelocity = Vector3.zero;


    private CharacterController controller;
    private Animator animator;

    private bool bCanMove = true;
    private bool bRun;
    private bool bJump;
    private bool bCover;

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

    public void FixedUpdate()
    {
        Movement();
        Jump();
        Gravity();
    }

    private void LateUpdate()
    {
        if (bUseCamera)
        {
            Update_RotateCamera();
            Update_ZoomCamera();
        }
    }

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

    public void Movement()
    {
        Transform testTransfom = transform.FindChildByName("CameraRoot").transform;

        if (!bCanMove) return;

        currInputMove = Vector2.SmoothDamp(currInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        Vector3 direction = Vector3.zero;
        float speed = bRun ? runSpeed : walkSpeed;
        animator.SetBool("IsRun", bRun);

        if (currInputMove.magnitude > deadZone)
        {
            Vector3 forward = testTransfom.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = testTransfom.right;
            right.y = 0;
            right.Normalize();

            direction = (right * currInputMove.x) + (forward * currInputMove.y);
            direction = direction.normalized * speed;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * sensitivity);

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

        Vector3 move = direction * Time.deltaTime;
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
    private void FindVcamDirection()
    {
        vcam = FindAnyObjectByType<CinemachineVirtualCamera>();
        Transform cameraTransform = vcam.transform;
        camDirection = cameraTransform.forward;
    }
}