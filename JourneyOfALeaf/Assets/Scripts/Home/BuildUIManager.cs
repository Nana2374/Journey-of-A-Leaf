using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildUIManager : MonoBehaviour
{
    [Header("Cameras")]
    public InputActionReference lookAroundAction;
    public CinemachineVirtualCamera topDownCamera;
    public int activePriority = 20;
    public int inactivePriority = 0;

    [SerializeField] private GameObject gridVisualization;

    [Header("Player UI")]
    public GameObject movementCanvas; // drag CanvasUI_Movement here

    [Header("References")]
    public PlacementSystem placementSystem;
    public FurnitureSelector furnitureSelector;
    public InputManager inputManager;

    [Header("Main Button")]
    public Button mainBuildButton;

    [Header("Build Panel")]
    public RectTransform buildPanel;
    public float slideDistance = 300f;
    public float slideDuration = 0.3f;

    [Header("UI Text")]
    public GameObject buildModeText;

    [Header("Action Bar")]
    public ActionBarFollower actionBarFollower;

    [Header("Action Buttons")]
    public Button placeButton;
    public Button rotateButton;
    public Button storeButton;   // was removeButton, now stores furniture to inventory

    [Header("Furniture Buttons")]
    public Button[] furnitureButtons;
    public int[] furnitureIDs;

    private bool isBuildModeOpen = false;
    private Vector2 panelHiddenPos;
    private Vector2 panelShownPos;

    private bool isMoving = false; // true while waiting for free placement after move

    void Start()
    {
        panelShownPos = buildPanel.anchoredPosition;
        panelHiddenPos = panelShownPos + new Vector2(-slideDistance, 0f);

        buildPanel.anchoredPosition = panelHiddenPos;
        buildPanel.gameObject.SetActive(false);

        actionBarFollower.Hide();

        // Make sure all action buttons start hidden
        placeButton.gameObject.SetActive(false);
        rotateButton.gameObject.SetActive(false);
        storeButton.gameObject.SetActive(false);

        mainBuildButton.onClick.AddListener(ToggleBuildMode);

        for (int i = 0; i < furnitureButtons.Length; i++)
        {
            int id = furnitureIDs[i];
            furnitureButtons[i].onClick.AddListener(() => OnFurnitureSelected(id));
        }
    }

    void ToggleBuildMode()
    {
        if (isBuildModeOpen) CloseBuildMode();
        else OpenBuildMode();
    }

    void OpenBuildMode()
    {
        isBuildModeOpen = true;
        inputManager.SetBuildPanelOpen(true);

        buildModeText.SetActive(true);
        gridVisualization.SetActive(true);
        lookAroundAction.action.Disable();
        topDownCamera.Priority = activePriority;
        buildPanel.gameObject.SetActive(true);
        furnitureSelector.EnterSelectionMode();
        movementCanvas.SetActive(false);  // hide movement UI
        StopAllCoroutines();
        StartCoroutine(SlidePanel(panelShownPos));
        actionBarFollower.Hide();
        RefreshFurnitureButtons();
    }

    void CloseBuildMode()
    {
        isBuildModeOpen = false;
        inputManager.SetBuildPanelOpen(true);

        buildModeText.SetActive(false);
        gridVisualization.SetActive(false);
        placementSystem.ForceStop();
        furnitureSelector.ExitSelectionMode();
        actionBarFollower.Hide();
        lookAroundAction.action.Enable();
        topDownCamera.Priority = inactivePriority;
        movementCanvas.SetActive(true);   // restore movement UI
        StopAllCoroutines();
        StartCoroutine(SlidePanel(panelHiddenPos, () =>
        {
            buildPanel.gameObject.SetActive(false);
        }));
    }

    void OnFurnitureSelected(int id)
    {
        furnitureSelector.Deselect();
        actionBarFollower.StopTracking();
        placementSystem.StartPlacement(id);

        // Restore place button label
        var placeText = placeButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (placeText != null) placeText.text = "Place";

        placeButton.gameObject.SetActive(true);
        rotateButton.gameObject.SetActive(true);
        storeButton.gameObject.SetActive(true);

        StartCoroutine(ShowActionBarNextFrame());
    }

    IEnumerator ShowActionBarNextFrame()
    {
        yield return null;
        actionBarFollower.Show();
    }

    // Called after placing to go back to selection mode
    public void OnItemPlaced()
    {
        isMoving = false;
        actionBarFollower.Hide();
        placeButton.gameObject.SetActive(false);
        rotateButton.gameObject.SetActive(false);
        storeButton.gameObject.SetActive(false);
        furnitureSelector.EnterSelectionMode();
        RefreshFurnitureButtons();
    }

    // Called when tapping an already-placed furniture piece
    public void ShowActionBarOnFurniture(GameObject furniture)
    {
        actionBarFollower.TrackWorldObject(furniture);
        actionBarFollower.Show();
        placeButton.gameObject.SetActive(true);   // show as Move button
        rotateButton.gameObject.SetActive(true);
        storeButton.gameObject.SetActive(true);

        // Change place button label to Move
        var placeText = placeButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (placeText != null) placeText.text = "Move";
    }
    public void HideActionBar()
    {
        actionBarFollower.Hide();
    }

    public void StartCoroutine_ShowActionBarNextFrame()
    {
        StartCoroutine(ShowActionBarNextFrame());
    }

    public void OnPlacePressed()
    {
        if (furnitureSelector.SelectedFurniture != null && !isMoving)
        {
            // Selected furniture — enter move mode
            isMoving = true;
            furnitureSelector.MoveSelected();

            var placeText = placeButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (placeText != null) placeText.text = "Place";
            StartCoroutine(ShowActionBarNextFrame());
            return;
        }

        // Place the preview down
        if (placementSystem.IsPlacing())
        {
            isMoving = false;
            placementSystem.PlaceCurrentItem();
        }
    }

    public void OnRotatePressed()
    {
        // Rotate preview if placing, rotate selected if selecting
        if (furnitureSelector.SelectedFurniture != null)
            furnitureSelector.RotateSelected();
        else
            placementSystem.RotateCurrentItem();
    }

    public void OnStorePressed()
    {
        if (furnitureSelector.SelectedFurniture != null)
        {
            isMoving = false;
            furnitureSelector.StoreSelected();
            RefreshFurnitureButtons();
        }
        else if (placementSystem.IsPlacing())
        {
            isMoving = false;
            placementSystem.CancelPlacement();
            actionBarFollower.Hide();
            placeButton.gameObject.SetActive(false);
            rotateButton.gameObject.SetActive(false);
            storeButton.gameObject.SetActive(false);
            furnitureSelector.EnterSelectionMode();
            RefreshFurnitureButtons();
        }
    }

    public void ContinuePlacement(int id)
    {
        isMoving = false;
        furnitureSelector.Deselect();
        actionBarFollower.StopTracking();
        placementSystem.StartPlacement(id);

        var placeText = placeButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (placeText != null) placeText.text = "Place";

        placeButton.gameObject.SetActive(true);
        rotateButton.gameObject.SetActive(true);
        storeButton.gameObject.SetActive(true);

        StartCoroutine(ShowActionBarNextFrame());
    }

    public void RefreshFurnitureButtons()
    {
        for (int i = 0; i < furnitureButtons.Length; i++)
        {
            int id = furnitureIDs[i];
            int qty = FurnitureInventory.Instance.GetQuantity(id);
            Debug.Log($"Button {i}: ID={id}, Quantity={qty}");

            var text = furnitureButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
                text.text = $"{qty}x";

            furnitureButtons[i].interactable = qty > 0;
        }
    }

    IEnumerator SlidePanel(Vector2 targetPos, System.Action onComplete = null)
    {
        Vector2 startPos = buildPanel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            buildPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        buildPanel.anchoredPosition = targetPos;
        onComplete?.Invoke();
    }
}