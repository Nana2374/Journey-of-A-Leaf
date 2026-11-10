using UnityEngine;

public class Player_Animator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Player_Movement playerMovement;
    [SerializeField] private Player_Input playerInput;
    [SerializeField] private CharacterController controller;

    // Parameter hashes — faster than strings
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsGlidingHash = Animator.StringToHash("IsGliding");
    private static readonly int IsInWaterHash = Animator.StringToHash("IsInWater");

    [Header("Settings")]
    [SerializeField] private float animationSmoothTime = 0.1f;

    private float currentSpeed;

    void Update()
    {
        UpdateMovementAnimation();
        UpdateJumpAnimation();
        UpdateGlideAnimation();
        UpdateWaterAnimation();
    }

    void UpdateMovementAnimation()
    {
        // Only show walk animation when grounded and not in special states
        if (playerMovement.IsGliding || playerMovement.IsInWater)
        {
            animator.SetFloat(SpeedHash, 0f);
            return;
        }

        Vector3 horizontalVelocity = new Vector3(
            controller.velocity.x, 0f, controller.velocity.z);
        float targetSpeed = horizontalVelocity.magnitude;

        currentSpeed = Mathf.Lerp(
            currentSpeed, targetSpeed, Time.deltaTime / animationSmoothTime);

        animator.SetFloat(SpeedHash, currentSpeed);
    }

    void UpdateJumpAnimation()
    {
        bool isGrounded = controller.isGrounded;
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(IsJumpingHash, !isGrounded &&
            playerMovement.GetVerticalVelocity() > 0f &&
            !playerMovement.IsGliding &&
            !playerMovement.IsInWater);
    }

    void UpdateGlideAnimation()
    {
        // Stubbed — wire up when glide animation is ready
        animator.SetBool(IsGlidingHash, playerMovement.IsGliding);
    }

    void UpdateWaterAnimation()
    {
        // Stubbed — wire up when water/boat animation is ready
        animator.SetBool(IsInWaterHash, playerMovement.IsInWater);
    }
}