using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NPCController))]
public class NPCGiveItemPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject promptRoot;   // parent of the icon+background+button (world-space)
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Image backgroundImage;   // the part that turns green/red
    [SerializeField] private Button giveButton;

    [Header("Settings")]
    [SerializeField] private float showRange = 3f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Color canGiveColor = Color.green;
    [SerializeField] private Color cannotGiveColor = new Color(1f, 0.3f, 0.3f);

    private NPCController npcController;
    private Transform player;
    private Camera mainCamera;

    private void Awake()
    {
        npcController = GetComponent<NPCController>();
        giveButton.onClick.AddListener(OnGiveButtonClicked);
    }

    private void Start()
    {
        mainCamera = Camera.main;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning(name + ": no GameObject tagged '" + playerTag + "' found for proximity checks.");

        promptRoot.SetActive(false);
    }

    private void Update()
    {
        if (player == null)
        {
            promptRoot.SetActive(false);
            return;
        }

        ItemData requiredItem = npcController.GetCurrentRequiredItem();

        // Only show once the quest has actually been accepted, and there IS a current request
        if (!npcController.CurrentStepOffered || requiredItem == null)
        {
            promptRoot.SetActive(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > showRange)
        {
            promptRoot.SetActive(false);
            return;
        }

        promptRoot.SetActive(true);

        // Billboard - always face the camera
        if (mainCamera != null)
            promptRoot.transform.forward = mainCamera.transform.forward;

        itemIconImage.sprite = requiredItem.icon;

        bool hasItem = FindPlayerItem(requiredItem) != null;
        backgroundImage.color = hasItem ? canGiveColor : cannotGiveColor;
        giveButton.interactable = hasItem;
    }

    private void OnGiveButtonClicked()
    {
        ItemData requiredItem = npcController.GetCurrentRequiredItem();
        if (requiredItem == null) return;

        LeafItem item = FindPlayerItem(requiredItem);
        if (item == null) return; // shouldn't happen since the button is only clickable when found

        if (npcController.CanAccept(item))
        {
            npcController.ReceiveItem(item);
        }
    }

    // Searches all leaf items in the scene for one matching the required data that's currently on the leaf.
    // NOTE: fine for a small number of items/NPCs; if your scene grows large, consider having
    // LeafController expose its placed items directly instead of scanning the whole scene.
    private LeafItem FindPlayerItem(ItemData data)
    {
        LeafItem[] allItems = FindObjectsOfType<LeafItem>();
        foreach (LeafItem item in allItems)
        {
            if (item.IsOnLeaf && item.Data == data)
                return item;
        }
        return null;
    }
}