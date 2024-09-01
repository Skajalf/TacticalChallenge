using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraComponent : MonoBehaviour
{
    // 플레이어는 카메라를 기준으로 방향을 정하게 됨
    [Header("Camera Settings")]
    [SerializeField] private bool bUseCamera = true; // 카메라 사용 여부 -> 옵션 창
    [SerializeField] private Vector2 zoomRange = new Vector2(1, 3);
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float zoomLerp = 25.0f;

    [Header("Mouse Settings")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.5f, 0.5f); // 마우스 민감도
    
    private Vector2 limitPitchAngle = new Vector2(80, 350); // Pitch 한계
    //[SerializeField] private float mouseRotationLerp = 0.25f;

    private Vector2 inputLook; // 현재 마우스 입력
    private float zoomDistance; // 카메라와 캐릭터 거리

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Cinemachine3rdPersonFollow tpsFollowCamera;

    private InputActionMap actionMap;
    private Quaternion cameraRotation; // Quaternion 값
    private Transform targetTransform; // 목표 대상 (GameObject)의 transform

    InputAction lookAction;
    InputAction zoomAction;

    private void Awake()
    {
        Init();
    }

    private void Init() // 최초설정과정
    {
        Cursor.visible = false; // 커서 비가시
        Cursor.lockState = CursorLockMode.Locked; // 커서 고정

        targetTransform = transform.FindChildByName("CameraRoot").transform; // 캐릭터의 CameraRoot Transform을 가져옴.

        cinemachineVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>(); // Virtual 카메라를 가져옴.
        tpsFollowCamera = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        cinemachineVirtualCamera.Follow = targetTransform; // Follow 설정.
        zoomDistance = tpsFollowCamera.CameraDistance;

        PlayerInput input = GetComponent<PlayerInput>();
        actionMap = input.actions.FindActionMap("Player");

        lookAction = actionMap.FindAction("Look");
        lookAction.performed += Input_Look_Performed;
        lookAction.canceled += Input_Look_Canceled;

        zoomAction = actionMap.FindAction("Zoom");
        zoomAction.performed += Input_Zoom_Performed;
    }

    private void FixedUpdate()
    {
        if (bUseCamera)
            UpdateCamera();
    }

    private void UpdateCamera()
    {
        Update_Rotation();
        Update_Zoom();
    }


    private void Update_Rotation()
    {
        cameraRotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x, Vector3.up);
        cameraRotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        targetTransform.rotation = cameraRotation; // Camera Root의 Transform.rotation 변경
        
        Vector3 currentAngle = cameraRotation.eulerAngles;
        currentAngle.z = 0.0f;

        float xAngle = currentAngle.x;

        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            currentAngle.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            currentAngle.x = limitPitchAngle.y;

        cameraRotation.eulerAngles = currentAngle;
    }

    // 원본 코드 그대로
    private void Update_Zoom()
    {
        if (MathHelpers.IsNearlyEqual(tpsFollowCamera.CameraDistance, zoomDistance, 0.01f))
        {
            tpsFollowCamera.CameraDistance = zoomDistance;

            return;
        }

        tpsFollowCamera.CameraDistance = Mathf.SmoothStep(tpsFollowCamera.CameraDistance, zoomDistance, zoomLerp * Time.deltaTime);
    }

    #region Input_Look Methods
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
    #endregion
}
