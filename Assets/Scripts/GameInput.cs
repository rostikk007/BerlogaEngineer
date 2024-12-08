using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameImput : MonoBehaviour
{
    public static GameImput Instance { get; private set; }
    private PlayerInputActions _playerInputActions;
    public event EventHandler OnPlayerAttack;
    private void Awake()
    {
        Instance = this;
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();
        _playerInputActions.Combat.Attack.started += PlayerAttack_started;
    }

    private void PlayerAttack_started(InputAction.CallbackContext obj)
    {
        if (OnPlayerAttack != null)
        {
            OnPlayerAttack.Invoke(this, EventArgs.Empty);
        }
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }
    public Vector3 GetMousePosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        return mousePos;
    }
    public void DisableMovement()
    {
        _playerInputActions.Disable();
    }
}
