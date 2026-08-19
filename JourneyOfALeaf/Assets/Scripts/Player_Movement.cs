using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTransform;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    private Vector3 velocity;

    private Vector3 lockedForward;
    private Vector3 lockedRight;

    private bool movementLocked;

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();

        moveAction.action.started += OnMovementStarted;
        moveAction.action.canceled += OnMovementCanceled;
    }

    private void OnDisable()
    {
        moveAction.action.started -= OnMovementStarted;
        moveAction.action.canceled -= OnMovementCanceled;

        moveAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void OnMovementStarted(InputAction.CallbackContext context)
    {
        // Capture camera direction when joystick starts
        lockedForward = cameraTransform.forward;
        lockedRight = cameraTransform.right;

        // Keep movement horizontal
        lockedForward.y = 0f;
        lockedRight.y = 0f;

        lockedForward.Normalize();
        lockedRight.Normalize();

        movementLocked = true;
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        movementLocked = false;
    }

    private void Update()
    {
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 move = Vector3.zero;

        if (movementLocked)
        {
            move =
                lockedForward * input.y +
                lockedRight * input.x;

            move = Vector3.ClampMagnitude(move, 1f);

            // Rotate Ant toward movement
            if (move.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(move);

                transform.rotation =
                    Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
            }
        }

        // Jump
        if (grounded && jumpAction.action.WasPressedThisFrame())
        {
            velocity.y =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = move * playerSpeed;

        finalMovement.y = velocity.y;

        controller.Move(
            finalMovement * Time.deltaTime
        );
    }
}
