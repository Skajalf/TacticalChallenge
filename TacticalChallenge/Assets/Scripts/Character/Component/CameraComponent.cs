using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraComponent : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool bUseCamera = true;  // 카메라 사용 여부
    [SerializeField] private Vector2 zoomRange = new Vector2(1, 3);
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float zoomLerp = 25.0f;

    [Header("Mouse Settings")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.5f, 0.5f);  // 마우스 민감도

    [Header("Camera Pitch Limits")]
    [SerializeField] private Vector2 limitPitchAngle = new Vector2(-45f, 45f);

    public Vector2 inputLook;  // 현재 마우스 입력
    public float currentZoomDistance;  // 카메라와 캐릭터 거리
    private float prevZoomDistance;  // 이전 거리

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Cinemachine3rdPersonFollow tpsFollowCamera;
    private Quaternion cameraRotation;  // 카메라 회전 값
    private Transform targetTransform;  // 카메라가 추적할 대상 Transform

    InputAction lookAction;
    InputAction zoomAction;
    InputAction aimAction;

    private void Awake()
    {
        Init();
    }

    private void Init()  // 최초 설정 과정
    {
        Cursor.visible = false;  // 커서 비가시
        Cursor.lockState = CursorLockMode.Locked;  // 커서 고정

        targetTransform = transform.FindChildByName("CameraRoot").transform;  // 캐릭터의 CameraRoot Transform을 가져옴

        cinemachineVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();  // Virtual 카메라 가져옴
        tpsFollowCamera = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        cinemachineVirtualCamera.Follow = targetTransform;  // 카메라의 추적 대상 설정
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
        // Yaw (수평) 회전은 항상 적용
        cameraRotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x, Vector3.up);

        // Pitch (수직) 회전은 제한에 도달하지 않은 경우에만 적용
        Vector3 currentEuler = cameraRotation.eulerAngles;
        float pitch = currentEuler.x;

        if (pitch > 180f) pitch -= 360f;  // 0~360도 범위를 -180~180도로 변환

        // 피치를 제한하는 조건 수정
        if (inputLook.y < 0 && pitch < limitPitchAngle.y)  // 아래로 내리는 입력, 한계점 미도달
        {
            cameraRotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        }
        else if (inputLook.y > 0 && pitch > limitPitchAngle.x)  // 위로 올리는 입력, 한계점 미도달
        {
            cameraRotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        }

        // 짐벌락 방지
        Vector3 newEuler = cameraRotation.eulerAngles;
        newEuler.z = 0.0f;  // Z축 회전 고정
        cameraRotation.eulerAngles = newEuler;

        // 카메라의 회전값을 목표 Transform에 반영
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
        return cameraRotation;  // CameraComponent의 회전 정보를 외부에서 참조 가능
    }

    public float GetCameraYaw()  // 카메라의 Yaw 값 (수평 회전 각도) 반환
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
