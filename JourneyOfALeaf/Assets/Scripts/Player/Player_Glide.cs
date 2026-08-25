using UnityEngine;

public class Player_Glide : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Player_Movement playerMovement;
    [SerializeField] private LeafController leaf;

    [Header("Glide Activation")]
    [SerializeField] private float minimumFallHeight = 2f;

    [Header("Glide Movement")]
    [SerializeField] private float glideGravity = -0.5f;
    [SerializeField] private float maxGlideFallSpeed = 1f;
    [SerializeField] private float glideHorizontalSpeed = 4f;

    private float highestPoint;

    private bool wasGrounded;
    private bool isGliding;

    private float glideVerticalVelocity;

    private void Update()
    {
        bool grounded = controller.isGrounded;

        // -----------------------------------------
        // RECORD HEIGHT WHEN LEAVING GROUND
        // -----------------------------------------

        if (wasGrounded && !grounded)
        {
            highestPoint = transform.position.y;

            glideVerticalVelocity = 0f;
        }

        // -----------------------------------------
        // CHECK WHETHER TO START GLIDING
        // -----------------------------------------

        if (!grounded && !isGliding)
        {
            float fallDistance =
                highestPoint - transform.position.y;

            if (fallDistance >= minimumFallHeight)
            {
                StartGlide();
            }
        }

        // -----------------------------------------
        // GLIDE
        // -----------------------------------------

        if (isGliding)
        {
            UpdateGlide();
        }

        // -----------------------------------------
        // LANDING
        // -----------------------------------------

        if (grounded && isGliding)
        {
            StopGlide();
        }

        wasGrounded = grounded;
    }

    private void StartGlide()
    {
        if (leaf == null || playerMovement == null)
            return;

        isGliding = true;

        glideVerticalVelocity = 0f;

        playerMovement.SetGliding(true);

        leaf.StartGliding();

        Debug.Log("GLIDE STARTED");
    }

    private void StopGlide()
    {
        isGliding = false;

        glideVerticalVelocity = 0f;

        playerMovement.SetGliding(false);

        leaf.StopGliding();

        Debug.Log("GLIDE ENDED");
    }

    private void UpdateGlide()
    {
        // -----------------------------------------
        // GLIDE GRAVITY
        // -----------------------------------------

        glideVerticalVelocity +=
            glideGravity * Time.deltaTime;

        // Limit maximum downward speed
        glideVerticalVelocity = Mathf.Max(
            glideVerticalVelocity,
            -maxGlideFallSpeed
        );

        // -----------------------------------------
        // LEAF STEERING
        // -----------------------------------------

        float steering = 0f;

        if (leaf != null)
        {
            steering = leaf.GetGlideDirection();
        }

        Vector3 horizontalMovement =
            transform.right *
            steering *
            glideHorizontalSpeed;

        // -----------------------------------------
        // SEND VERTICAL MOVEMENT
        // -----------------------------------------

        playerMovement.SetVerticalVelocity(
            glideVerticalVelocity
        );

        // -----------------------------------------
        // HORIZONTAL MOVEMENT
        // -----------------------------------------

        controller.Move(
            horizontalMovement * Time.deltaTime
        );
    }
}