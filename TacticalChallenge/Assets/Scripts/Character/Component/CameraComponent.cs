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
    [SerializeField] private bool bUseCamera = true;  // ī�޶� ��� ����
    [SerializeField] private Vector2 zoomRange = new Vector2(2.0f, 3.0f);
    [SerializeField] private float zoomSensitivity = 0.1f;
    [SerializeField] private float zoomLerp = 40.0f;

    [Header("Mouse Settings")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.1f, 0.1f);  // ���콺 �ΰ��� x�� �¿�, y�� ����

    [Header("Camera Pitch Limits")]
    [SerializeField] private Vector2 limitPitchAngle = new Vector2(-20f, 75f);

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.3f;

    [Header("Obstruction Settings")]
    [SerializeField] private LayerMask obstructionMask;      // ��ֹ� ���̾�
    [SerializeField] private Material transparentMaterial;    // ���̵�� ���� ��Ƽ����
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


    [HideInInspector] public Vector2 inputLook;  // ���� ���콺 �Է�
    [HideInInspector] public float currentZoomDistance;  // ī�޶�� ĳ���� �Ÿ�
    private float prevZoomDistance;  // ���� �Ÿ�
    private float collisionZoomDistance;

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
