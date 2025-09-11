using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UI_CrossHair : MonoBehaviour
{
    [Header("크로스헤어의 각 파트들")]
    public RectTransform topLeft;
    public RectTransform topRight;
    public RectTransform bottomLeft;
    public RectTransform bottomRight;

    [Header("조종하는 캐릭터의 루트와 카메라 컴포넌트")]
    public Transform playerRoot;
    public CameraComponent cameraComponent;

    [Header("분산 정도")]
    public float idleSpread = 30f;
    public float walkSpread = 50f;
    public float runSpread = 70f;
    public float aimSpread = 25f;
    public float lookSpread = 50f;
    public float maxSpread = 120f;

    [Header("분산 속도")]
    public float spreadLerpSpeed = 9f;

    [Header("Look (Mouse)")]
    public float lookInputScale = 10f;

    private MovingComponent movingComponent;
    private float currentSpread;
    private float targetSpread;

    void Awake()
    {
        if (playerRoot != null)
        {
            movingComponent = playerRoot.GetComponent<MovingComponent>();
        }
        targetSpread = idleSpread;
    }

    void Update()
    {
        if (cameraComponent == null) return;

        bool isAiming = Mathf.Abs(cameraComponent.currentZoomDistance - cameraComponent.GetZoomRange().x) < 0.01f;

        if (isAiming)
        {
            targetSpread = aimSpread;
        }
        else
        {
            switch (movingComponent.currentState)
            {
                case MovingComponent.MoveState.Idle:
                    targetSpread = idleSpread;
                    break;
                case MovingComponent.MoveState.Move:
                    targetSpread = walkSpread;
                    break;
                case MovingComponent.MoveState.Run:
                    targetSpread = runSpread;
                    break;
                default:
                    targetSpread = idleSpread;
                    break;
            }
        }

        float lookMag = cameraComponent.inputLook.magnitude;
        float lookEffect = Mathf.Clamp(lookMag * lookInputScale, 0f, lookSpread);
        float finalTargetSpread = targetSpread + lookEffect;

        finalTargetSpread = Mathf.Clamp(finalTargetSpread, 0f, maxSpread);
        currentSpread = Mathf.Lerp(currentSpread, finalTargetSpread, 1f - Mathf.Exp(-spreadLerpSpeed * Time.deltaTime));

        ApplyPositions();
    }

    void ApplyPositions()
    {
        Vector2 tl = new Vector2(-currentSpread, currentSpread);
        Vector2 tr = new Vector2(currentSpread, currentSpread);
        Vector2 bl = new Vector2(-currentSpread, -currentSpread);
        Vector2 br = new Vector2(currentSpread, -currentSpread);

        if (topLeft) topLeft.anchoredPosition = tl;
        if (topRight) topRight.anchoredPosition = tr;
        if (bottomLeft) bottomLeft.anchoredPosition = bl;
        if (bottomRight) bottomRight.anchoredPosition = br;
    }
}