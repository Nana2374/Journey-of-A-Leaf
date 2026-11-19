using UnityEngine;

/// <summary>
/// Owns the NPC's Animator. Nothing else should touch it.
/// - Player enters greet radius -> NPC turns to face them and waves
/// - Otherwise the Animator alternates between its two idle states
/// - StartChat() -> chat animations; EndChat() -> wave goodbye, then back to idle
///
/// Animator parameters required (exact names):
///   Wave (Trigger), IsChatting (Bool), ChatIndex (Int)
/// </summary>
public class NPCAnimationController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to auto-find an Animator on this object or its children.")]
    [SerializeField] private Animator animator;
    [SerializeField] private string playerTag = "Player";

    [Header("Greeting")]
    [SerializeField] private float greetRadius = 4f;
    [Tooltip("Extra distance the player must move away before the NPC can greet again (prevents flicker at the edge).")]
    [SerializeField] private float exitBuffer = 0.75f;

    [Header("Facing")]
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private bool returnToStartRotation = true;

    [Header("Chat animations")]
    [Tooltip("How many chat variants exist in the Animator (ChatIndex 0..n-1).")]
    [SerializeField] private int chatVariants = 3;
    [Tooltip("Seconds between switching chat variants. 0 = pick once per chat.")]
    [SerializeField] private float chatVariantInterval = 6f;

    private static readonly int WaveHash = Animator.StringToHash("Wave");
    private static readonly int IsChattingHash = Animator.StringToHash("IsChatting");
    private static readonly int ChatIndexHash = Animator.StringToHash("ChatIndex");

    private Transform player;
    private Quaternion startRotation;
    private bool playerInRange;
    private bool isChatting;
    private float nextVariantTime;

    public bool IsChatting => isChatting;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        startRotation = transform.rotation;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning(name + ": no GameObject tagged '" + playerTag + "' found.");
    }

    private void Update()
    {
        if (animator == null) return;

        UpdateProximity();
        UpdateFacing();

        if (isChatting && chatVariantInterval > 0f && Time.time >= nextVariantTime)
            PickChatVariant();
    }

    // ---------- Proximity / greeting ----------

    private void UpdateProximity()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!playerInRange && distance <= greetRadius)
        {
            playerInRange = true;
            if (!isChatting) animator.SetTrigger(WaveHash); // greeting wave
        }
        else if (playerInRange && distance > greetRadius + exitBuffer)
        {
            playerInRange = false;
        }
    }

    private void UpdateFacing()
    {
        if (player != null && (playerInRange || isChatting))
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0f; // stay upright
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
            }
        }
        else if (returnToStartRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, startRotation, turnSpeed * Time.deltaTime);
        }
    }

    // ---------- Called by NPCController ----------

    public void StartChat()
    {
        if (isChatting || animator == null) return;

        isChatting = true;
        animator.ResetTrigger(WaveHash); // don't let a pending greeting wave interrupt the chat
        animator.SetBool(IsChattingHash, true);
        PickChatVariant();
    }

    public void EndChat()
    {
        if (!isChatting || animator == null) return;

        isChatting = false;
        animator.SetBool(IsChattingHash, false);
        animator.SetTrigger(WaveHash); // goodbye wave; Animator then returns to idle
    }

    private void PickChatVariant()
    {
        if (chatVariants > 1)
            animator.SetInteger(ChatIndexHash, Random.Range(0, chatVariants));
        nextVariantTime = Time.time + chatVariantInterval;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, greetRadius);
    }
}
