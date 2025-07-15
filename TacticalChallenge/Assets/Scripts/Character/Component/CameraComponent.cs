using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraComponent : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool bUseCamera = true;  // 카메라 사용 여부
    [SerializeField] private Vector2 zoomRange = new Vector2(2.0f, 3.0f);
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float zoomLerp = 40.0f;

    [Header("Mouse Settings")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.1f, 0.1f);  // 마우스 민감도 x는 좌우, y는 수직

    [Header("Camera Pitch Limits")]
    [SerializeField] private Vector2 limitPitchAngle = new Vector2(-20f, 75f);

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.3f;

    [Header("Obstruction Settings")]
    [SerializeField] private LayerMask obstructionMask;      // 장애물 레이어
    [SerializeField] private Material transparentMaterial;    // 페이드용 투명 머티리얼
    [SerializeField] private float minAlpha = 0.1f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float holdDuration = 1.5f;

    private enum FadeState { FadingOut, Hold, FadingIn }
    private class ObstructionInfo
    {
        public Renderer rend;
        public Material[] originalMats;
        public Material fadeMat;
        public FadeState state;
        public float timer;
    }
    private Dictionary<Renderer, ObstructionInfo> obstructions = new Dictionary<Renderer, ObstructionInfo>();


    [HideInInspector] public Vector2 inputLook;  // 현재 마우스 입력
    [HideInInspector] public float currentZoomDistance;  // 카메라와 캐릭터 거리
    private float prevZoomDistance;  // 이전 거리
    private float collisionZoomDistance;

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

        tpsFollowCamera.CameraDistance = zoomRange.y;
        currentZoomDistance = zoomRange.y;
        collisionZoomDistance = currentZoomDistance;

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
        if (!bUseCamera) return;

        UpdateCamera();
        HandleObstructions();
        ClampZoomDistance();
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
        if (MathHelpers.IsNearlyEqual(tpsFollowCamera.CameraDistance, collisionZoomDistance, 0.01f))
        {
            tpsFollowCamera.CameraDistance = collisionZoomDistance;
            return;
        }

        tpsFollowCamera.CameraDistance = Mathf.Lerp(tpsFollowCamera.CameraDistance, collisionZoomDistance, zoomLerp * Time.deltaTime);
    }

    private void ClampZoomDistance()
    {
        collisionZoomDistance = currentZoomDistance;

        Vector3 dir = (cinemachineVirtualCamera.transform.position - targetTransform.position).normalized;

        if (Physics.SphereCast(
                targetTransform.position,
                collisionRadius,
                dir,
                out RaycastHit hit,
                currentZoomDistance,
                collisionMask))
        {
            float minAllowed = Mathf.Max(hit.distance - collisionRadius, zoomRange.x);
            collisionZoomDistance = minAllowed;
        }
    }

    private void HandleObstructions()
    {
        Vector3 camPos = cinemachineVirtualCamera.transform.position;
        Vector3 dir = (targetTransform.position - camPos).normalized;
        float dist = Vector3.Distance(camPos, targetTransform.position);

        var hits = Physics.RaycastAll(camPos, dir, dist, obstructionMask)
            .Select(h => h.collider.GetComponent<Renderer>())
            .Where(r => r != null)
            .Distinct();

        foreach (var rend in hits)
        {
            if (obstructions.ContainsKey(rend)) continue;
            var info = new ObstructionInfo
            {
                rend = rend,
                originalMats = rend.sharedMaterials,
                fadeMat = new Material(transparentMaterial),
                state = FadeState.FadingOut,
                timer = 0f
            };
            obstructions[rend] = info;
            rend.materials = Enumerable.Repeat(info.fadeMat, info.originalMats.Length).ToArray();
        }

        foreach (var kv in obstructions)
        {
            var info = kv.Value;
            bool currentlyHit = hits.Contains(info.rend);
            if (!currentlyHit && info.state == FadeState.Hold)
            {
                info.state = FadeState.FadingIn;
                info.timer = 0f;
            }
            else if (!currentlyHit && info.state == FadeState.FadingOut)
            {
                info.state = FadeState.FadingIn;
                info.timer = fadeDuration * (info.timer / fadeDuration);
            }
        }

        UpdateObstructions();
    }

    private void UpdateObstructions()
    {
        float dt = Time.deltaTime;
        foreach (var kv in obstructions.ToList())
        {
            var info = kv.Value;
            switch (info.state)
            {
                case FadeState.FadingOut:
                    info.timer += dt;
                    float tOut = Mathf.Clamp01(info.timer / fadeDuration);
                    SetAlpha(info.fadeMat, Mathf.Lerp(1f, minAlpha, tOut));
                    if (tOut >= 1f) { info.state = FadeState.Hold; info.timer = 0f; }
                    break;
                case FadeState.Hold:
                    if (Physics.Raycast(
                        cinemachineVirtualCamera.transform.position,
                        (info.rend.bounds.center - cinemachineVirtualCamera.transform.position).normalized,
                        out var hit,
                        Mathf.Infinity,
                        obstructionMask) && hit.collider.GetComponent<Renderer>() == info.rend)
                    {
                        info.timer = 0f;
                    }
                    else info.timer += dt;

                    if (info.timer >= holdDuration)
                    {
                        info.state = FadeState.FadingIn;
                        info.timer = 0f;
                    }
                    break;
                case FadeState.FadingIn:
                    info.timer += dt;
                    float tIn = Mathf.Clamp01(info.timer / fadeDuration);
                    SetAlpha(info.fadeMat, Mathf.Lerp(minAlpha, 1f, tIn));
                    if (tIn >= 1f)
                    {
                        info.rend.materials = info.originalMats;
                        obstructions.Remove(info.rend);
                    }
                    break;
            }
        }
    }

    private void SetAlpha(Material mat, float a)
    {
        var c = mat.color;
        c.a = a;
        mat.color = c;
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
