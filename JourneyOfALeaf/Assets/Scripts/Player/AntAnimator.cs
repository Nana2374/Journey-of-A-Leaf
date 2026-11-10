using UnityEngine;

public class AntAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Player_Movement playerMovement;
    [SerializeField] private CharacterController controller;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");      // Trigger
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsGlidingHash = Animator.StringToHash("IsGliding");
    private static readonly int IsInWaterHash = Animator.StringToHash("IsInWater");

    [Header("Settings")]
    [SerializeField] private float animationSmoothTime = 0.1f;

    private float currentSpeed;
    private bool wasGrounded;

    void Update()
    {
        UpdateMovement();
        UpdateJump();
        UpdateGlide();
        UpdateWater();
    }

    void UpdateMovement()
    {
        if (playerMovement.IsGliding || playerMovement.IsInWater)
        {
            animator.SetFloat(SpeedHash, 0f);
            return;
        }

        Vector3 horizontalVelocity = new Vector3(
            controller.velocity.x, 0f, controller.velocity.z);
        float targetSpeed = horizontalVelocity.magnitude;

        currentSpeed = Mathf.Lerp(
        currentSpeed, targetSpeed,
        1f - Mathf.Exp(-Time.deltaTime / animationSmoothTime));

        animator.SetFloat(SpeedHash, currentSpeed);
    }

    void UpdateJump()
    {
        bool isGrounded = controller.isGrounded;
        animator.SetBool(IsGroundedHash, isGrounded);

        // Detect the moment the player leaves the ground
        // This fires the trigger exactly once per jump, reliably
        if (wasGrounded && !isGrounded &&
            !playerMovement.IsGliding &&
            !playerMovement.IsInWater)
        {
            animator.SetTrigger(JumpHash);
        }

        wasGrounded = isGrounded;
    }

    void UpdateGlide()
    {
        animator.SetBool(IsGlidingHash, playerMovement.IsGliding);
    }

    void UpdateWater()
    {
        animator.SetBool(IsInWaterHash, playerMovement.IsInWater);
    }
}