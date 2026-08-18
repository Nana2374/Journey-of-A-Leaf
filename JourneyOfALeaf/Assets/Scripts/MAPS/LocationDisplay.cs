using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocationDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float holdTime = 1.5f;
    [SerializeField] private float fadeOutTime = 1f;

    [Tooltip("The name shown to the player for THIS scene")]
    [SerializeField] private string locationName = "Location";

    void Start()
    {
        ShowLocationText();
    }

    public void ShowLocationText()
    {
        locationText.text = locationName;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // Fade in
        yield return Fade(0f, 1f, fadeInTime);

        // Hold
        yield return new WaitForSeconds(holdTime);

        // Fade out
        yield return Fade(1f, 0f, fadeOutTime);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}