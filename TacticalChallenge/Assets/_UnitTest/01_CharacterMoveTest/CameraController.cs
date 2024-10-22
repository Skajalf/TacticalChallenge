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
    [SerializeField] private Transform lookAtTransform;   // ī�޶� �ٶ� ��� (LookAt)
    [SerializeField] private float cameraArmLength = 5f;  // LookAt���κ��� ī�޶� ������ �Ÿ�
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);  // LookAt ���� ������
    [SerializeField, Range(0.5f, 20f)] private float verticalSensitivity = 10f;   // ���� �ΰ���
    [SerializeField, Range(0.5f, 20f)] private float horizontalSensitivity = 10f; // ���� �ΰ���
    [SerializeField] private Vector2 rollRange = new Vector2(-45f, 10f);   // ���� ���� ����
    [SerializeField] private Vector2 pitchRange = new Vector2(-45f, 45f);  // ���� ���� ����

    private Vector3 cameraTempPosition;
    private Vector3 cameraTempRotation;
    float roll;
    float pitch;

    /// <summary>
    /// �Է� ���� �޾Ƽ� ȸ�� ������ ������Ʈ.
    /// roll�� pitch�� �Է¿� ���� ȸ���ϴ� ������ ��Ÿ��.
    /// </summary>
    private void GetInput()
    {
        roll += input.Roll * Time.deltaTime * verticalSensitivity;
        roll = Mathf.Clamp(roll, rollRange.x, rollRange.y);  // roll (���� ���� ����)

        pitch += input.Pitch * Time.deltaTime * horizontalSensitivity;
        pitch = Mathf.Clamp(pitch, pitchRange.x, pitchRange.y);  // pitch (���� ���� ����)
    }

    /// <summary>
    /// LookAtTransform�� �������� ī�޶� �̵���Ŵ.
    /// - LookAtTransform���� ������ offset�� arm ���̸�ŭ ������ ��ġ�� ī�޶� �̵�.
    /// </summary>
    private void MoveCamera()
    {
        // LookAtTransform ��ġ���� offset�� ������ ī�޶� ��ġ ���
        cameraTempPosition = lookAtTransform.position + lookAtTransform.rotation * offset;

        // LookAtTransform���� ī�޶� ��ġ�ؾ� �� ���� ��� (cameraArmLength�� �ݿ�)
        Vector3 cameraArmVector = -lookAtTransform.forward * cameraArmLength;

        // ī�޶� ȸ�� ���� (roll: ����, pitch: ����)
        cameraArmVector = Quaternion.AngleAxis(-roll, lookAtTransform.right) * cameraArmVector;  // ���� ȸ��
        cameraArmVector = Quaternion.AngleAxis(pitch, lookAtTransform.up) * cameraArmVector;    // ���� ȸ��

        // ī�޶� ��ġ ������Ʈ
        cameraTempPosition += cameraArmVector;

        // ī�޶� ���� ��ġ�� �̵�
        mainCamera.transform.position = cameraTempPosition;
    }

    /// <summary>
    /// ī�޶� LookAtTransform�� ���ϵ��� ȸ����Ŵ.
    /// - LookAtTransform���� offset ��ġ�� �������� ī�޶� ȸ��.
    /// </summary>
    private void RotateCamera()
    {
        // LookAtTransform���� offset ������ ������ ���� ī�޶� ȸ��
        cameraTempRotation = lookAtTransform.position + lookAtTransform.rotation * offset;

        // ī�޶� LookAtTransform�� �ٶ󺸴� ���� ���
        Vector3 cameraVector = (cameraTempRotation - mainCamera.transform.position).normalized;

        Debug.DrawLine(cameraTempRotation, mainCamera.transform.position, Color.green);  // ����׿�

        if (cameraVector == Vector3.zero)
            return;

        // ī�޶� LookAtTransform�� ���ϰ� ȸ��
        Quaternion cameraRotation = Quaternion.LookRotation(cameraVector);

        // ī�޶� ȸ�� ����
        mainCamera.transform.rotation = cameraRotation;
    }
}
