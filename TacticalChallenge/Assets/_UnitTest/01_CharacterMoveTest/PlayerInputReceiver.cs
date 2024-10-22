using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputReceiver : MonoBehaviour
{
    /// <summary>
    /// ¿ì¼± WASD¸¸
    /// </summary>


    public Vector2 InputMove { get; private set; }

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        PlayerInput input = GetComponent<PlayerInput>();

        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction move = actionMap.FindAction("Move");
        move.performed += MovePerform;
        move.canceled += MoveCanceled;
    }

    private void MovePerform(InputAction.CallbackContext context)
    {
        InputMove = context.ReadValue<Vector2>().normalized;
    }
    
    private void MoveCanceled(InputAction.CallbackContext context)
    {
        InputMove = Vector2.zero;
    }
}
