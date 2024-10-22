using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReceiver))]

public partial class MovementController : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;
    private PlayerInputReceiver input;
    private Transform cameraTransform;
    private Vector3 InputVector;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputReceiver>();
    }
    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        GetInput();
        RotatePlayerByCamera();
        MovePlayerByCamera();
    }
}

public partial class MovementController
{
    private Vector2 movementInput;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float rotateSpeed = 10.0f;

    private void GetInput()
    {
        movementInput = input.InputMove;
        InputVector = new Vector3(movementInput.x, 0f, movementInput.y);
    }

    /// <summary>
    /// cameraTransform의 방향으로 캐릭터 회전
    /// </summary>
    private void RotatePlayerByCamera()
    {
        if (movementInput == Vector2.zero)
        {
            animator.SetBool("IsMove", false);
            return;
        }

        Vector3 towardVector = cameraTransform.forward;
        towardVector.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(towardVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed) ;
        animator.SetBool("IsMove", true);
    }

    /// <summary>
    /// 전 단계에서 회전된 방향을 기준으로 전후좌우 움직임
    /// </summary>
    private void MovePlayerByCamera()
    {
        characterController.Move(transform.rotation * InputVector * Time.deltaTime);
    }

}
