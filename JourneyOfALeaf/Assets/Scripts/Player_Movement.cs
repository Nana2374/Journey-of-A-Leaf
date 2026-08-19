using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("References")]
    public CharacterController controller;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    private Vector3 playerVelocity;
    private bool groundedPlayer;

    // Controlled by Player_Glide
    public bool IsGliding { get; private set; }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;

        // -----------------------------------------
        // GROUNDING
        // -----------------------------------------

        if (groundedPlayer && playerVelocity.y < 0f)
        {
            playerVelocity.y = -2f;
        }

        // -----------------------------------------
        // MOVEMENT INPUT
        // -----------------------------------------

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 move = new Vector3(
            input.x,
            0f,
            input.y
        );

        move = Vector3.ClampMagnitude(move, 1f);

        // -----------------------------------------
        // ROTATION
        // -----------------------------------------

        if (move != Vector3.zero && !IsGliding)
        {
            transform.forward = move;
        }

        // -----------------------------------------
        // JUMP
        // -----------------------------------------

        if (groundedPlayer &&
            jumpAction.action.WasPressedThisFrame() &&
            !IsGliding)
        {
            playerVelocity.y =
                Mathf.Sqrt(
                    jumpHeight * -2f * gravityValue
                );
        }

        // -----------------------------------------
        // GRAVITY
        // -----------------------------------------

        if (!IsGliding)
        {
            playerVelocity.y +=
                gravityValue * Time.deltaTime;
        }

        // -----------------------------------------
        // MOVEMENT
        // -----------------------------------------

        Vector3 finalMove =
            move * playerSpeed;

        finalMove.y = playerVelocity.y;

        controller.Move(
            finalMove * Time.deltaTime
        );
    }

    // Called by Player_Glide
    public void SetGliding(bool gliding)
    {
        IsGliding = gliding;

        if (gliding)
        {
            // Remove normal falling velocity
            playerVelocity.y = 0f;
        }
    }

    // Allows Player_Glide to control vertical movement
    public void SetVerticalVelocity(float velocity)
    {
        playerVelocity.y = velocity;
    }

    public float GetVerticalVelocity()
    {
        return playerVelocity.y;
    }
}