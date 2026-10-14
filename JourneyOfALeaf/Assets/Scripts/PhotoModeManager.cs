using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class PhotoModeManager : MonoBehaviour
{
    [Header("References")]
    public GameObject[] uiToHide;     // drag all UI canvases to hide here
    public GameObject photoModeUI;    // the UI that shows during photo mode (just the capture button)
    public Button photoModeButton;    // the button that toggles photo mode
    public Button captureButton;      // the button that takes the photo

    [Header("Flash Effect")]
    public Image flashImage;          // a white full-screen Image for flash effect
    public float flashDuration = 0.3f;

    [Header("SFX")]
    public AudioClip shutterClip;

    private bool isPhotoMode = false;


    void Start()
    {
        photoModeUI.SetActive(false);
        photoModeButton.onClick.AddListener(TogglePhotoMode);
        captureButton.onClick.AddListener(TakePhoto);

        if (flashImage != null)
            flashImage.gameObject.SetActive(false);
    }

    void TogglePhotoMode()
    {
        if (isPhotoMode)
            ExitPhotoMode();
        else
            EnterPhotoMode();
    }

    void EnterPhotoMode()
    {
        isPhotoMode = true;

        // Hide all game UI
        foreach (var ui in uiToHide)
            ui.SetActive(false);

        // Show only photo mode UI
        photoModeUI.SetActive(true);
    }

    void ExitPhotoMode()
    {
        isPhotoMode = false;

        // Restore all game UI
        foreach (var ui in uiToHide)
            ui.SetActive(true);

        // Hide photo mode UI
        photoModeUI.SetActive(false);
    }

    void TakePhoto()
    {
        StartCoroutine(CapturePhoto());
    }

    IEnumerator CapturePhoto()
    {
        Debug.Log("CapturePhoto started");

        photoModeUI.SetActive(false);
        photoModeButton.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Flash effect
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);
            flashImage.color = new Color(1f, 1f, 1f, 1f);
            StartCoroutine(FadeFlash());
        }

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] bytes = screenshot.EncodeToPNG();
        Destroy(screenshot);

        string fileName = $"Photo_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

#if UNITY_EDITOR || UNITY_STANDALONE
        // Save to Downloads on laptop/PC
        string downloadsPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            "Downloads",
            fileName
        );
        File.WriteAllBytes(downloadsPath, bytes);
        Debug.Log($"Photo saved to: {downloadsPath}");

#elif UNITY_ANDROID || UNITY_IOS
    // Save to camera roll on mobile (requires NativeGallery plugin)
    string tempPath = Path.Combine(Application.temporaryCachePath, fileName);
    File.WriteAllBytes(tempPath, bytes);

    NativeGallery.SaveImageToGallery(
        tempPath,
        "YourGameName",  // album name, change this to your game's name
        fileName,
        (success, path) => Debug.Log($"Photo saved: {success}, path: {path}")
    );
#endif

        if (shutterClip != null)
            SFXManager.Instance?.PlaySFX(shutterClip);

        yield return new WaitForSeconds(0.5f);

        photoModeUI.SetActive(true);
        photoModeButton.gameObject.SetActive(true);
    }

    IEnumerator FadeFlash()
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        flashImage.gameObject.SetActive(false);
    }

}