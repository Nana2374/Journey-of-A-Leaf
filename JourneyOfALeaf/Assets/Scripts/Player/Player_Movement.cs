using UnityEngine;
public class Player_Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravityValue = -9.81f;
    [Header("References")]
    public CharacterController controller;
    // All raw input reading and camera-relative direction math now lives
    // in Player_Input. This script just consumes the result.
    private Player_Input playerInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    // Set true by the UI Jump Button's OnClick(), consumed next Update
    private bool jumpButtonPressed = false;
    // Controlled by Player_Glide
    public bool IsGliding { get; private set; }
    // Controlled by PlayerWaterFloater
    public bool IsInWater { get; private set; }
    private void OnEnable()
    {
        playerInput = GetComponent<Player_Input>();
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
        // MOVEMENT INPUT (camera-relative, computed in Player_Input)
        // -----------------------------------------
        Vector3 move = playerInput.MoveDirection;
        // -----------------------------------------
        // ROTATION
        // -----------------------------------------
        if (move != Vector3.zero && !IsGliding)
        {
            // Instant snap. This used to feed a rotation feedback loop when
            // the FreeLook camera was parented under the player (camera's
            // rotation inherited the player's rotation, which fed back into
            // this calculation). Now that the camera is unparented, this is
            // safe again.
            transform.rotation = Quaternion.LookRotation(move, Vector3.up);
        }
        // -----------------------------------------
        // JUMP
        // -----------------------------------------
        bool jumpTriggered =
            playerInput.JumpPressedThisFrame ||
            jumpButtonPressed;
        if (groundedPlayer &&
            jumpTriggered &&
            !IsGliding &&
            !IsInWater)
        {
            playerVelocity.y =
                Mathf.Sqrt(
                    jumpHeight * -2f * gravityValue
                );
        }
        // Reset the UI button flag every frame after it's been checked
        jumpButtonPressed = false;
        // -----------------------------------------
        // GRAVITY
        // -----------------------------------------
        if (!IsGliding && !IsInWater)
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
    // Call this from the UI Jump Button's OnClick() event
    public void TriggerJump()
    {
        jumpButtonPressed = true;
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
    // Called by PlayerWaterFloater when the player enters/exits the water volume
    public void SetInWater(bool inWater)
    {
        IsInWater = inWater;
        if (inWater)
        {
            // Remove normal falling/jumping velocity so gravity doesn't
            // keep accumulating underneath the float correction
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