using UnityEngine;
using UnityEngine.InputSystem;

// Reads raw movement/jump input and converts it into camera-relative
// direction data that Player_Movement can consume. This is the one place
// to look when debugging "what direction is the dpad/camera producing,"
// separate from the physics/jump/glide/water logic in Player_Movement.
public class Player_Input : MonoBehaviour
{
    [Header("Camera")]
    // Assign your FreeLook camera (or its rig/pivot transform). Movement
    // input is interpreted relative to this transform's facing direction,
    // projected onto the ground plane.
    public Transform cameraTransform;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    // Camera-relative, magnitude-clamped movement direction for this frame.
    // (0,0,0) length means no input.
    public Vector3 MoveDirection { get; private set; }

    // True on the exact frame the jump action was pressed via the
    // configured input action (gamepad/keyboard). Does NOT include the
    // UI jump button — that's handled separately in Player_Movement via
    // TriggerJump(), since it comes from a different source (OnClick).
    public bool JumpPressedThisFrame { get; private set; }

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
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 move;
        if (cameraTransform != null)
        {
            // Flatten the camera's forward/right onto the XZ (ground) plane
            // so pitch (looking up/down) doesn't affect movement speed/direction.
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            move = camForward * input.y + camRight * input.x;
        }
        else
        {
            // Fallback: behave as world-space if no camera is assigned
            move = new Vector3(input.x, 0f, input.y);
        }

        MoveDirection = Vector3.ClampMagnitude(move, 1f);
        JumpPressedThisFrame = jumpAction.action.WasPressedThisFrame();


    }
}