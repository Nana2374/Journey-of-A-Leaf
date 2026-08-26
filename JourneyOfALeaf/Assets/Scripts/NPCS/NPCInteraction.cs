using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCController))]
public class NPCInteraction : MonoBehaviour
{
    [Header("Interaction Range")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private string playerTag = "Player";

    private NPCController npcController;
    private Transform player;

    private void Awake()
    {
        npcController = GetComponent<NPCController>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning(name + ": no GameObject tagged '" + playerTag + "' found for proximity checks.");
    }

    private void Update()
    {
        if (player == null || InteractPromptUI.Instance == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactRange)
        {
            InteractPromptUI.Instance.Show(OnInteractPressed);
        }
        else
        {
            InteractPromptUI.Instance.HideIfOwnedBy(OnInteractPressed);
        }
    }

    // Called when the player taps the "Click to interact" button while in range
    private void OnInteractPressed()
    {
        InteractPromptUI.Instance.Hide();
        npcController.Interact();
    }
}