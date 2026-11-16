using UnityEngine;

public class AntAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Player_Movement playerMovement;
    [SerializeField] private CharacterController controller;
    [SerializeField] LeafController leafController;
    [SerializeField] private Player_Input playerInput;   // drag from the Player

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");      // Trigger
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");


    private static readonly int IsInWaterHash = Animator.StringToHash("IsInWater");
    private static readonly int BoatStartHash = Animator.StringToHash("BoatStart");
    private static readonly int BoatEndHash = Animator.StringToHash("BoatEnd");
    private static readonly int BoatDirectionHash = Animator.StringToHash("BoatDirection");

    private static readonly int IsGlidingHash = Animator.StringToHash("IsGliding");
    private static readonly int GlideDirectionHash = Animator.StringToHash("GlideDirection");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");

    [Header("Settings")]
    [SerializeField] private float animationSmoothTime = 0.1f;

    private float currentSpeed;
    private bool wasGrounded;
    private bool wasInWater;

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

        float target = 0f;
        if (playerMovement.IsGliding)
        {
            // Rotation is frozen while gliding, so compare input to the player's facing
            Vector3 move = playerInput.MoveDirection;
            target = Mathf.Clamp(Vector3.Dot(move, playerMovement.transform.right), -1f, 1f);
        }

        // Damping smooths the lean so it doesn't snap
        //animator.SetFloat(GlideDirectionHash, target, 0.1f, Time.deltaTime);
    }

    void UpdateWater()
    {
        bool isInWater = playerMovement.IsInWater;

        animator.SetBool(IsInWaterHash, isInWater);

        // Detect entering water
        if (isInWater && !wasInWater)
        {
            animator.SetTrigger(BoatStartHash);
        }

        // Detect leaving water
        if (!isInWater && wasInWater)
        {
            animator.SetTrigger(BoatEndHash);
        }

        wasInWater = isInWater;

        // Read left/right D-pad input directly
        float targetDirection = 0f;

        if (isInWater)
        {
            targetDirection = playerInput.RawMoveInput.x;
        }

        animator.SetFloat(
            BoatDirectionHash,
            targetDirection,
            0.1f,
            Time.deltaTime
        );
    }
}