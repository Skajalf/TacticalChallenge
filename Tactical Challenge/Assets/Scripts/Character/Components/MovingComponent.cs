using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovingComponent : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 3.0f;
    [SerializeField] private float jumpForce = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera Settings")]
    [SerializeField] private bool bUseCamera = true;
    [SerializeField] private string followTargetName = "Camera Root";
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 limitPitchAngle = new Vector2(80, 350);
    //[SerializeField] private float mouseRotationLerp = 0.25f;
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float zoomLerp = 25.0f;
    [SerializeField] private Vector2 zoomRange = new Vector2(1, 3);

    private Vector2 inputLook;
    private float zoomDistance;
    private Transform followTargetTransform;

    private Quaternion rotation;
    //public Quaternion Rotation { set => rotation = value; }

    [Header("Tech Settings")]
    [SerializeField] private float sensitivity = 100.0f;
    [SerializeField] private float deadZone = 0.01f;
    private Vector2 velocity;

    private Vector2 inputMove;
    private Vector2 currInputMove;
    private Vector3 verticalVelocity = Vector3.zero;

    private Cinemachine3rdPersonFollow tpsFollowCamera;
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

        InputAction lookAction = actionMap.FindAction("Look");
        lookAction.performed += Input_Look_Performed;
        lookAction.canceled += Input_Look_Canceled;

        InputAction zoomAction = actionMap.FindAction("Zoom");
        zoomAction.performed += Input_Zoom_Performed;

        InputAction cover = actionMap.FindAction("Cover");
        cover.started += startCover;
        cover.canceled += cancelCover;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        followTargetTransform = transform.FindChildByName(followTargetName);


        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        CinemachineVirtualCamera camera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        tpsFollowCamera = camera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (tpsFollowCamera != null)
            zoomDistance = tpsFollowCamera.CameraDistance;
    }

    private void Update()
    {
        
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

    private void Input_Look_Performed(InputAction.CallbackContext context)
    {
        inputLook = context.ReadValue<Vector2>();
    }

    private void Input_Look_Canceled(InputAction.CallbackContext context)
    {
        inputLook = Vector2.zero;
    }

    private void Input_Zoom_Performed(InputAction.CallbackContext context)
    {
        float value = -context.ReadValue<float>() * zoomSensitivity;

        zoomDistance += value;
        zoomDistance = Mathf.Clamp(zoomDistance, zoomRange.x, zoomRange.y);
    }

    private void startCover(InputAction.CallbackContext context)
    {
        bCover = true;
    }

    private void cancelCover(InputAction.CallbackContext context)
    {
        bCover = false;
    }

    private void Update_RotateCamera()
    {
        rotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x, Vector3.up);
        rotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        followTargetTransform.rotation = rotation;

        Vector3 angles = rotation.eulerAngles;
        angles.z = 0.0f;

        float xAngle = rotation.eulerAngles.x;

        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            angles.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            angles.x = limitPitchAngle.y;

        rotation.eulerAngles = angles;

        if (currInputMove.magnitude > deadZone)
        {
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
            followTargetTransform.localEulerAngles = new Vector3(angles.x, 0, 0);
        }

    }

    private void Update_ZoomCamera()
    {
        if (MathHelpers.IsNearlyEqual(tpsFollowCamera.CameraDistance, zoomDistance, 0.01f))
        {
            tpsFollowCamera.CameraDistance = zoomDistance;

            return;
        }

        tpsFollowCamera.CameraDistance = Mathf.SmoothStep(tpsFollowCamera.CameraDistance, zoomDistance, zoomLerp * Time.deltaTime);
    }

    public void Movement()
    {
        if (!bCanMove) return;

        currInputMove = Vector2.SmoothDamp(currInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        Vector3 direction = Vector3.zero;
        float speed = bRun ? runSpeed : walkSpeed;
        animator.SetBool("IsRun", bRun);

        if (currInputMove.magnitude > deadZone)
        {
            Vector3 forward = followTargetTransform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = followTargetTransform.right;
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
}