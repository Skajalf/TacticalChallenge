using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public partial class CameraInputReceiver : MonoBehaviour
{
    private PlayerInput playerInput;
    private Vector2 input;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        InputActionMap actionMap = playerInput.actions.FindActionMap("Camera");
        Debug.Assert(actionMap != null, "[CameraInputReceiver] : ActionMap Not Found");

        actionMap.FindAction("LookAround", false).performed += InputLookPerformed;

        actionMap.FindAction("LookAround", true).canceled += InputLookCanceled;
    }
}

#region Rotation Calculate
public partial class CameraInputReceiver
{
    public float Roll { get; private set; }
    public float Pitch { get; private set; }

    private void InputLookPerformed(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
        Roll = input.y;
        Pitch = input.x;
    }

    private void InputLookCanceled(InputAction.CallbackContext context)
    {
        Roll = 0;
        Pitch = 0;
    }
}
#endregion