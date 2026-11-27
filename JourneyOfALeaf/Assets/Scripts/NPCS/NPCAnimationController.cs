using UnityEngine;

/// Owns the NPC's Animator.
/// Animator parameters: Wave (Trigger), Look (Trigger), PlayerClose (Bool), IsChatting (Bool), ChatIndex (Int)
public class NPCAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string playerTag = "Player";

    [Header("Greeting / facing")]
    [SerializeField] private float greetRadius = 4f;
    [SerializeField] private float exitBuffer = 0.75f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private bool returnToStartRotation = true;

    [Header("Idle behaviour")]
    [SerializeField] private float closeRadius = 1f;                        // inside this: continuous waving
    [SerializeField] private Vector2 lookInterval = new Vector2(10f, 20f);  // outside: rare looks
    [SerializeField, Range(0f, 1f)] private float lookChance = 0.5f;

    [Header("Chat animations")]
    [SerializeField] private int chatVariants = 3;
    [SerializeField] private float chatVariantInterval = 6f;

    private static readonly int WaveHash = Animator.StringToHash("Wave");
    private static readonly int LookHash = Animator.StringToHash("Look");
    private static readonly int PlayerCloseHash = Animator.StringToHash("PlayerClose");
    private static readonly int IsChattingHash = Animator.StringToHash("IsChatting");
    private static readonly int ChatIndexHash = Animator.StringToHash("ChatIndex");

    private Transform player;
    private Quaternion startRotation;
    private bool playerInRange, isChatting;
    private float nextVariantTime, nextLookTime;

    public bool IsChatting => isChatting;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        startRotation = transform.rotation;
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;
        else Debug.LogWarning(name + ": no GameObject tagged '" + playerTag + "' found.");
    }

    private void Update()
    {
        if (animator == null) return;

        Vector3 toPlayer = player != null ? player.position - transform.position : Vector3.zero;
        float distance = player != null ? toPlayer.magnitude : float.MaxValue;
        bool close = distance <= closeRadius && !isChatting;

        animator.SetBool(PlayerCloseHash, close);

        // Greeting wave when entering the greet radius (skipped if already in the wave loop)
        if (!playerInRange && distance <= greetRadius)
        {
            playerInRange = true;
            if (!isChatting && !close) animator.SetTrigger(WaveHash);
        }
        else if (playerInRange && distance > greetRadius + exitBuffer)
            playerInRange = false;

        // Facing
        toPlayer.y = 0f;
        bool face = (playerInRange || isChatting) && toPlayer.sqrMagnitude > 0.001f;
        if (face || returnToStartRotation)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                face ? Quaternion.LookRotation(toPlayer) : startRotation, turnSpeed * Time.deltaTime);

        // Occasional look, only when far and idle
        if (close) animator.ResetTrigger(LookHash);
        else if (!isChatting && Time.time >= nextLookTime)
        {
            nextLookTime = Time.time + Random.Range(lookInterval.x, lookInterval.y);
            if (Random.value < lookChance
                && animator.GetCurrentAnimatorStateInfo(0).IsName("Cici_Idle")
                && !animator.IsInTransition(0))
                animator.SetTrigger(LookHash);
        }

        if (isChatting && chatVariantInterval > 0f && Time.time >= nextVariantTime)
            PickChatVariant();
    }

    // ---------- Called by NPCController ----------

    public void StartChat()
    {
        if (isChatting || animator == null) return;
        isChatting = true;
        animator.ResetTrigger(WaveHash);
        animator.ResetTrigger(LookHash);
        animator.SetBool(IsChattingHash, true);
        PickChatVariant();
    }

    public void EndChat()
    {
        if (!isChatting || animator == null) return;
        isChatting = false;
        animator.SetBool(IsChattingHash, false);
        animator.SetTrigger(WaveHash); // goodbye wave
    }

    private void PickChatVariant()
    {
        if (chatVariants > 1) animator.SetInteger(ChatIndexHash, Random.Range(0, chatVariants));
        nextVariantTime = Time.time + chatVariantInterval;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, greetRadius);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, closeRadius);
    }
}