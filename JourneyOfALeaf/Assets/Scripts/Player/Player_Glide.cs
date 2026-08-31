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

    [Header("Boost Settings")]
    [SerializeField] private float boostDrag = 2f; // how quickly the boost fades (higher = faster decay)

    private float highestPoint;
    private bool wasGrounded;
    private bool isGliding;
    private float glideVerticalVelocity;

    // BOOST STATE
    private Vector3 boostVelocity = Vector3.zero;

    public bool IsGliding => isGliding;

    private void Update()
    {
        bool grounded = controller.isGrounded;

        if (wasGrounded && !grounded)
        {
            highestPoint = transform.position.y;
            glideVerticalVelocity = 0f;
        }

        if (!grounded && !isGliding)
        {
            float fallDistance = highestPoint - transform.position.y;
            if (fallDistance >= minimumFallHeight)
            {
                StartGlide();
            }
        }

        if (isGliding)
        {
            UpdateGlide();
        }

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
        boostVelocity = Vector3.zero; // clear boost on landing
        playerMovement.SetGliding(false);
        leaf.StopGliding();
        Debug.Log("GLIDE ENDED");
    }

    private void UpdateGlide()
    {
        glideVerticalVelocity += glideGravity * Time.deltaTime;
        glideVerticalVelocity = Mathf.Max(glideVerticalVelocity, -maxGlideFallSpeed);

        float steering = 0f;
        if (leaf != null)
        {
            steering = leaf.GetGlideDirection();
        }

        Vector3 horizontalMovement = transform.right * steering * glideHorizontalSpeed;

        // ADD BOOST ON TOP OF NORMAL STEERING
        horizontalMovement += boostVelocity;

        // DECAY THE BOOST OVER TIME
        boostVelocity = Vector3.Lerp(boostVelocity, Vector3.zero, boostDrag * Time.deltaTime);

        playerMovement.SetVerticalVelocity(glideVerticalVelocity);

        controller.Move(horizontalMovement * Time.deltaTime);
    }

    /// <summary>
    /// Called externally (e.g. by a boost ring) to give the player a burst of speed while gliding.
    /// </summary>
    public void ApplyBoost(Vector3 direction, float speed)
    {
        boostVelocity = direction.normalized * speed;
    }
}