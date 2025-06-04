using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraComponent : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool bUseCamera = true;  // ī�޶� ��� ����
    [SerializeField] private Vector2 zoomRange = new Vector2(1.5f, 3.0f);
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float zoomLerp = 40.0f;

    [Header("Mouse Settings")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.1f, 0.1f);  // ���콺 �ΰ��� x�� �¿�, y�� ����

    [Header("Camera Pitch Limits")]
    [SerializeField] private Vector2 limitPitchAngle = new Vector2(-45f, 75f);

    [HideInInspector] public Vector2 inputLook;  // ���� ���콺 �Է�
    [HideInInspector] public float currentZoomDistance;  // ī�޶�� ĳ���� �Ÿ�
    private float prevZoomDistance;  // ���� �Ÿ�

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Cinemachine3rdPersonFollow tpsFollowCamera;
    private Quaternion cameraRotation;  // ī�޶� ȸ�� ��
    private Transform targetTransform;  // ī�޶� ������ ��� Transform

    InputAction lookAction;
    InputAction zoomAction;
    InputAction aimAction;

    private void Awake()
    {
        Init();
    }

    private void Init()  // ���� ���� ����
    {
        Cursor.visible = false;  // Ŀ�� �񰡽�
        Cursor.lockState = CursorLockMode.Locked;  // Ŀ�� ����

        targetTransform = transform.FindChildByName("CameraRoot").transform;  // ĳ������ CameraRoot Transform�� ������

        cinemachineVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();  // Virtual ī�޶� ������
        tpsFollowCamera = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        cinemachineVirtualCamera.Follow = targetTransform;  // ī�޶��� ���� ��� ����

        tpsFollowCamera.CameraDistance = zoomRange.y;
        currentZoomDistance = zoomRange.y;

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
        // Yaw (����) ȸ���� �׻� ����
        cameraRotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x, Vector3.up);

        // Pitch (����) ȸ���� ���ѿ� �������� ���� ��쿡�� ����
        Vector3 currentEuler = cameraRotation.eulerAngles;
        float pitch = currentEuler.x;

        if (pitch > 180f) pitch -= 360f;  // 0~360�� ������ -180~180���� ��ȯ

        // ��ġ�� �����ϴ� ���� ����
        if (inputLook.y < 0 && pitch < limitPitchAngle.y)  // �Ʒ��� ������ �Է�, �Ѱ��� �̵���
        {
            cameraRotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        }
        else if (inputLook.y > 0 && pitch > limitPitchAngle.x)  // ���� �ø��� �Է�, �Ѱ��� �̵���
        {
            cameraRotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        }

        // ������ ����
        Vector3 newEuler = cameraRotation.eulerAngles;
        newEuler.z = 0.0f;  // Z�� ȸ�� ����
        cameraRotation.eulerAngles = newEuler;

        // ī�޶��� ȸ������ ��ǥ Transform�� �ݿ�
        targetTransform.rotation = cameraRotation;
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
        return cameraRotation;  // CameraComponent�� ȸ�� ������ �ܺο��� ���� ����
    }

    public float GetCameraYaw()  // ī�޶��� Yaw �� (���� ȸ�� ����) ��ȯ
    {
        return cameraRotation.eulerAngles.y;
    }

    #region Input Methods
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

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        var textStyle = new GUIStyle();
        textStyle.normal.textColor = Color.green;
        GUILayout.Label(cameraRotation.eulerAngles.ToString(), textStyle);
        GUILayout.EndArea();
    }
#endif
}
