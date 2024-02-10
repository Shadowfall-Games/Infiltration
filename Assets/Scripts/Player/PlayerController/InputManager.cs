using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;

    public static InputManager Instance {
        get {
            return instance;
        }
    }

    private PlayerControlls playerControlls;

    private void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
        }
        playerControlls = new PlayerControlls();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerControlls.Player.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return playerControlls.Player.CameraLook.ReadValue<Vector2>();
    }

    public bool PlayerJumped()
    {
        return playerControlls.Player.Jump.triggered;
    }

    private void OnEnable() => playerControlls.Enable();

    private void OnDisable() => playerControlls.Disable();
}