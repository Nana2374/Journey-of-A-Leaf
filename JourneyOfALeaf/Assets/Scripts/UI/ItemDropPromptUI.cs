using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDropPromptUI : MonoBehaviour
{
    public static ItemDropPromptUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform promptRoot; // the "Drop" button's container
    [SerializeField] private Button dropButton;

    [Header("Settings")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f); // hover above the item

    private LeafItem currentItem;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mainCamera = Camera.main;
        promptRoot.gameObject.SetActive(false);

        dropButton.onClick.AddListener(OnDropClicked);
    }

    private void Update()
    {
        if (currentItem == null) return;

        // Keep the button hovering above the selected item on screen
        Vector3 screenPos = mainCamera.WorldToScreenPoint(currentItem.transform.position + worldOffset);
        promptRoot.position = screenPos;
    }

    public void Show(LeafItem item)
    {
        currentItem = item;
        promptRoot.gameObject.SetActive(true);
    }

    public void Hide()
    {
        currentItem = null;
        promptRoot.gameObject.SetActive(false);
    }

    public bool IsShowingFor(LeafItem item) => currentItem == item;

    private void OnDropClicked()
    {
        if (currentItem == null) return;

        currentItem.RemoveFromLeaf();
        Hide();
    }
}