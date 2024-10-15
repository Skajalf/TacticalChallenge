using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraComponent : MonoBehaviour
{
    // �÷��̾�� ī�޶� �������� ������ ���ϰ� ��
    [Header("Camera Settings")]
    [SerializeField] private bool bUseCamera = true; // ī�޶� ��� ���� -> �ɼ� â
    [SerializeField] private Vector2 zoomRange = new Vector2(1, 3);
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float zoomLerp = 25.0f;

    [Header("Mouse Settings")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.5f, 0.5f); // ���콺 �ΰ���

    private Vector2 limitPitchAngle = new Vector2(45, 340); // Pitch �Ѱ�
    //[SerializeField] private float mouseRotationLerp = 0.25f;

    private Vector2 inputLook; // ���� ���콺 �Է�
    public float currentZoomDistance; // ī�޶�� ĳ���� �Ÿ�
    private float prevZoomDistance; // ���� �Ÿ�

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Cinemachine3rdPersonFollow tpsFollowCamera;
    private Quaternion cameraRotation; // Quaternion ��
    private Transform targetTransform; // ��ǥ ��� (GameObject)�� transform

    InputAction lookAction;
    InputAction zoomAction;
    InputAction aimAction;

    private void Awake()
    {
        Init();
    }

    private void Init() // ���ʼ�������
    {
        Cursor.visible = false; // Ŀ�� �񰡽�
        Cursor.lockState = CursorLockMode.Locked; // Ŀ�� ����

        targetTransform = transform.FindChildByName("CameraRoot").transform; // ĳ������ CameraRoot Transform�� ������.

        cinemachineVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>(); // Virtual ī�޶� ������.
        tpsFollowCamera = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        cinemachineVirtualCamera.Follow = targetTransform; // Follow ����.
        currentZoomDistance = tpsFollowCamera.CameraDistance;

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        lookAction = actionMap.FindAction("Look");
        lookAction.performed += Input_Look_Performed;
        lookAction.canceled += Input_Look_Canceled;

        zoomAction = actionMap.FindAction("Zoom");
        zoomAction.performed += Input_Zoom_Performed;

        aimAction = actionMap.FindAction("Aim");
        aimAction.performed += Input_Aim_Performed;
        aimAction.canceled += Input_Aim_Canceled;
    }

    private void Update()
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
        targetTransform.rotation = cameraRotation; // Camera Root�� Transform.rotation ����

        Vector3 currentAngle = cameraRotation.eulerAngles;
        currentAngle.z = 0.0f;

        float xAngle = currentAngle.x;

        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            currentAngle.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            currentAngle.x = limitPitchAngle.y;

        cameraRotation.eulerAngles = currentAngle;
    }

    private void Update_Zoom()
    {
        if (MathHelpers.IsNearlyEqual(tpsFollowCamera.CameraDistance, currentZoomDistance, 0.01f))
        {
            tpsFollowCamera.CameraDistance = currentZoomDistance;

            return;
        }

        tpsFollowCamera.CameraDistance = Mathf.SmoothStep(tpsFollowCamera.CameraDistance, currentZoomDistance, zoomLerp * Time.deltaTime);
    }

    public Quaternion GetCameraRotation()
    {
        return cameraRotation;  // CameraComponent�� ȸ�� ������ �ܺο��� ������ �� �ֵ��� ��
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

        currentZoomDistance += value;
        currentZoomDistance = Mathf.Clamp(currentZoomDistance, zoomRange.x, zoomRange.y);
    }

    private void Input_Aim_Performed(InputAction.CallbackContext context)
    {
        prevZoomDistance = currentZoomDistance;
        currentZoomDistance = zoomRange.x;
    }

    private void Input_Aim_Canceled(InputAction.CallbackContext context)
    {
        currentZoomDistance = prevZoomDistance;
    }
    #endregion
}
