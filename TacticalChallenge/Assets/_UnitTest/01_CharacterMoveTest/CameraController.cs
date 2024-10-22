using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CameraInputReceiver))]
public partial class CameraController : MonoBehaviour
{
    private CameraInputReceiver input;
    private Camera mainCamera;

    private void Awake()
    {
        input = GetComponent<CameraInputReceiver>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        GetInput();
        MoveCamera();
        RotateCamera();
    }
}


public partial class CameraController
{
    [Header("Camera Setting")]
    [SerializeField] private Transform lookAtTransform;   // 카메라가 바라볼 대상 (LookAt)
    [SerializeField] private float cameraArmLength = 5f;  // LookAt으로부터 카메라가 떨어질 거리
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);  // LookAt 기준 오프셋
    [SerializeField, Range(0.5f, 20f)] private float verticalSensitivity = 10f;   // 수직 민감도
    [SerializeField, Range(0.5f, 20f)] private float horizontalSensitivity = 10f; // 수평 민감도
    [SerializeField] private Vector2 rollRange = new Vector2(-45f, 10f);   // 수직 각도 제한
    [SerializeField] private Vector2 pitchRange = new Vector2(-45f, 45f);  // 수평 각도 제한

    private Vector3 cameraTempPosition;
    private Vector3 cameraTempRotation;
    float roll;
    float pitch;

    /// <summary>
    /// 입력 값을 받아서 회전 각도를 업데이트.
    /// roll과 pitch는 입력에 따라 회전하는 각도를 나타냄.
    /// </summary>
    private void GetInput()
    {
        roll += input.Roll * Time.deltaTime * verticalSensitivity;
        roll = Mathf.Clamp(roll, rollRange.x, rollRange.y);  // roll (수직 각도 제한)

        pitch += input.Pitch * Time.deltaTime * horizontalSensitivity;
        pitch = Mathf.Clamp(pitch, pitchRange.x, pitchRange.y);  // pitch (수평 각도 제한)
    }

    /// <summary>
    /// LookAtTransform을 기준으로 카메라를 이동시킴.
    /// - LookAtTransform에서 지정된 offset과 arm 길이만큼 떨어진 위치로 카메라 이동.
    /// </summary>
    private void MoveCamera()
    {
        // LookAtTransform 위치에서 offset을 적용한 카메라 위치 계산
        cameraTempPosition = lookAtTransform.position + lookAtTransform.rotation * offset;

        // LookAtTransform에서 카메라가 위치해야 할 벡터 계산 (cameraArmLength를 반영)
        Vector3 cameraArmVector = -lookAtTransform.forward * cameraArmLength;

        // 카메라 회전 적용 (roll: 수직, pitch: 수평)
        cameraArmVector = Quaternion.AngleAxis(-roll, lookAtTransform.right) * cameraArmVector;  // 수직 회전
        cameraArmVector = Quaternion.AngleAxis(pitch, lookAtTransform.up) * cameraArmVector;    // 수평 회전

        // 카메라 위치 업데이트
        cameraTempPosition += cameraArmVector;

        // 카메라를 최종 위치로 이동
        mainCamera.transform.position = cameraTempPosition;
    }

    /// <summary>
    /// 카메라가 LookAtTransform을 향하도록 회전시킴.
    /// - LookAtTransform에서 offset 위치를 기준으로 카메라를 회전.
    /// </summary>
    private void RotateCamera()
    {
        // LookAtTransform에서 offset 적용한 지점을 향해 카메라 회전
        cameraTempRotation = lookAtTransform.position + lookAtTransform.rotation * offset;

        // 카메라가 LookAtTransform을 바라보는 벡터 계산
        Vector3 cameraVector = (cameraTempRotation - mainCamera.transform.position).normalized;

        Debug.DrawLine(cameraTempRotation, mainCamera.transform.position, Color.green);  // 디버그용

        if (cameraVector == Vector3.zero)
            return;

        // 카메라가 LookAtTransform을 향하게 회전
        Quaternion cameraRotation = Quaternion.LookRotation(cameraVector);

        // 카메라 회전 적용
        mainCamera.transform.rotation = cameraRotation;
    }
}
