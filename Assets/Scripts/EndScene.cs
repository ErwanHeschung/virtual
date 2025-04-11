using UnityEngine;
using System.Collections;
using TMPro; // Make sure to import TextMesh Pro

public class FinalSceneSlideshowTMP : MonoBehaviour
{
    [Header("Slides Settings")]
    // Assign each slide’s CanvasGroup component (should be set to full screen in the RectTransform)
    public CanvasGroup[] slideCanvasGroups;

    [Header("Thank You Settings")]
    // The final TextMesh Pro component for the "Thank You" message.

    [Header("Timing Settings")]
    public float fadeDuration = 1f;   // Time for fade in/out transitions
    public float displayTime = 2f;    // Time that each slide remains fully visible

    void Start()
    {
        // Ensure all slides and the thank you text start hidden.
        foreach (CanvasGroup group in slideCanvasGroups)
        {
            group.alpha = 0f;
            group.gameObject.SetActive(false);
        }

        StartCoroutine(PlaySlideshow());
    }

    IEnumerator PlaySlideshow()
    {
        // Loop through each slide CanvasGroup
        foreach (CanvasGroup group in slideCanvasGroups)
        {
            // Activate the slide and fade it in
            group.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(group, 0f, 1f, fadeDuration));

            // Wait for the slide to be displayed
            yield return new WaitForSeconds(displayTime);

            // Fade out the slide
            yield return StartCoroutine(FadeCanvasGroup(group, 1f, 0f, fadeDuration));

            // Disable the slide to avoid overlapping with the next one
            group.gameObject.SetActive(false);
        }

    }

    // Fades a CanvasGroup's alpha value from startAlpha to endAlpha over the duration.
    IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            group.alpha = alpha;
            yield return null;
        }
        group.alpha = endAlpha;
    }

    // Fades a TextMesh Pro text's alpha value from startAlpha to endAlpha over the duration.

}
