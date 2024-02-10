using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 2.0f;
    [SerializeField] private float _jumpHeight = 1.0f;

    [SerializeField] private Transform _cameraTransform;

    private CharacterController controller;

    private Vector3 playerVelocity;

    private bool groundedPlayer;
    private float gravityValue = -9.81f;

    private InputManager inputManager;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 movement = inputManager.GetPlayerMovement();
        Vector3 move = new Vector3(movement.x, 0f, movement.y);
        move = _cameraTransform.forward * move.z + _cameraTransform.right * move.x;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * _playerSpeed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        if (inputManager.PlayerJumped() && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}