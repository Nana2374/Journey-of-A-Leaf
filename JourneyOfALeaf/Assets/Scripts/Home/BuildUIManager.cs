using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;



public class BuildUIManager : MonoBehaviour
{
    [Header("Cameras")]
    public InputActionReference lookAroundAction;  // drag LookAround action here
    public CinemachineVirtualCamera topDownCamera;
    public int activePriority = 20;
    public int inactivePriority = 0;

    [SerializeField]
    private GameObject gridVisualization;

    [Header("References")]
    public PlacementSystem placementSystem;

    [Header("Main Button")]
    public Button mainBuildButton;

    [Header("Build Panel")]
    public RectTransform buildPanel;
    public float slideDistance = 300f;
    public float slideDuration = 0.3f;

    [Header("UI Text")]
    public GameObject buildModeText; // drag your Text GameObject here

    [Header("Action Bar")]
    public ActionBarFollower actionBarFollower;

    [Header("Action Buttons")]
    public Button placeButton;
    public Button rotateButton;
    public Button removeButton;

    [Header("Furniture Buttons")]
    public Button[] furnitureButtons;
    public int[] furnitureIDs;

    private bool isBuildModeOpen = false;
    private bool isRemoving = false;
    private Vector2 panelHiddenPos;
    private Vector2 panelShownPos;

    void Start()
    {
        panelShownPos = buildPanel.anchoredPosition;
        panelHiddenPos = panelShownPos + new Vector2(-slideDistance, 0f);

        buildPanel.anchoredPosition = panelHiddenPos;
        buildPanel.gameObject.SetActive(false);

        actionBarFollower.Hide();

        mainBuildButton.onClick.AddListener(ToggleBuildMode);

        //placeButton.onClick.AddListener(OnPlacePressed);
        //rotateButton.onClick.AddListener(OnRotatePressed);
        //removeButton.onClick.AddListener(OnRemovePressed);

        for (int i = 0; i < furnitureButtons.Length; i++)
        {
            int id = furnitureIDs[i];
            furnitureButtons[i].onClick.AddListener(() => OnFurnitureSelected(id));
        }

        actionBarFollower.Hide();
    }

    void ToggleBuildMode()
    {
        if (isBuildModeOpen)
            CloseBuildMode();
        else
            OpenBuildMode();
    }

    void OpenBuildMode()
    {
        isBuildModeOpen = true;
        buildModeText.SetActive(true);
        gridVisualization.SetActive(true);

        isRemoving = false;
        lookAroundAction.action.Disable();       // disable camera swipe
        topDownCamera.Priority = activePriority;
        buildPanel.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(SlidePanel(panelShownPos));
        actionBarFollower.Hide();
    }

    void CloseBuildMode()
    {
        isBuildModeOpen = false;
        buildModeText.SetActive(false);
        gridVisualization.SetActive(false);

        isRemoving = false;
        placementSystem.ForceStop();
        actionBarFollower.Hide();
        lookAroundAction.action.Enable();         // re-enable camera swipe
        topDownCamera.Priority = inactivePriority;
        StopAllCoroutines();
        StartCoroutine(SlidePanel(panelHiddenPos, () =>
        {
            buildPanel.gameObject.SetActive(false);
        }));
    }

    void OnFurnitureSelected(int id)
    {
        isRemoving = false;
        placementSystem.StartPlacement(id);
        actionBarFollower.Show();
        HighlightRemoveButton(false);
        ShowPlaceRotate(true);
    }

    public void OnPlacePressed()
    {
        Debug.Log("PLACE pressed");
        placementSystem.PlaceCurrentItem();

    }

    public void OnRotatePressed()
    {
        Debug.Log("ROTATE pressed");
        placementSystem.RotateCurrentItem();

    }

    public void OnRemovePressed()
    {
        Debug.Log("REMOVE pressed");

        isRemoving = !isRemoving;
        if (isRemoving)
        {
            placementSystem.StartRemoving();
            HighlightRemoveButton(true);
            ShowPlaceRotate(false);
        }
        else
        {
            placementSystem.ForceStop();
            HighlightRemoveButton(false);
            ShowPlaceRotate(false);
        }
        actionBarFollower.Show();
    }

    void ShowPlaceRotate(bool visible)
    {
        placeButton.gameObject.SetActive(visible);
        rotateButton.gameObject.SetActive(visible);
    }

    void HighlightRemoveButton(bool active)
    {
        var colors = removeButton.colors;
        colors.normalColor = active ? Color.red : Color.white;
        removeButton.colors = colors;
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