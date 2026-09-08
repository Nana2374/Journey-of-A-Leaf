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

    [Header("References")]
    public PlacementSystem placementSystem;
    public FurnitureSelector furnitureSelector;

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
        buildModeText.SetActive(true);
        gridVisualization.SetActive(true);
        lookAroundAction.action.Disable();
        topDownCamera.Priority = activePriority;
        buildPanel.gameObject.SetActive(true);
        furnitureSelector.EnterSelectionMode();
        StopAllCoroutines();
        StartCoroutine(SlidePanel(panelShownPos));
        actionBarFollower.Hide();
        RefreshFurnitureButtons(); // add this line
    }

    void CloseBuildMode()
    {
        isBuildModeOpen = false;
        buildModeText.SetActive(false);
        gridVisualization.SetActive(false);
        placementSystem.ForceStop();
        furnitureSelector.ExitSelectionMode();
        actionBarFollower.Hide();
        lookAroundAction.action.Enable();
        topDownCamera.Priority = inactivePriority;
        StopAllCoroutines();
        StartCoroutine(SlidePanel(panelHiddenPos, () =>
        {
            buildPanel.gameObject.SetActive(false);
        }));
    }

    void OnFurnitureSelected(int id)
    {
        Debug.Log($"OnFurnitureSelected called with ID={id}");
        furnitureSelector.Deselect();
        placementSystem.StartPlacement(id);
        placeButton.gameObject.SetActive(true);
        rotateButton.gameObject.SetActive(true);
        storeButton.gameObject.SetActive(false);
        actionBarFollower.Show();
    }

    // Called after placing to go back to selection mode
    public void OnItemPlaced()
    {
        actionBarFollower.Hide();
        furnitureSelector.EnterSelectionMode();
        RefreshFurnitureButtons();
    }

    // Called when tapping an already-placed furniture piece
    public void ShowActionBarOnFurniture(GameObject furniture)
    {
        actionBarFollower.TrackWorldObject(furniture);
        actionBarFollower.Show();
        placeButton.gameObject.SetActive(false);  // no place when selecting
        rotateButton.gameObject.SetActive(true);
        storeButton.gameObject.SetActive(true);   // store back to inventory
    }

    public void HideActionBar()
    {
        actionBarFollower.Hide();
    }

    public void OnPlacePressed()
    {
        placementSystem.PlaceCurrentItem();
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
        furnitureSelector.StoreSelected();
        RefreshFurnitureButtons();
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