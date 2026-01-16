using System.Collections;
using UnityEngine;
using TMPro;

public class NotificationUI : MonoBehaviour
{
    [Header("UI Element References")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float popDuration = 0.5f;
    [SerializeField] private float displayDuration = 3.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private Coroutine _animationCoroutine;

    private void Start()
    {
        // Ensure initial state is hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        // Subscribe to event
        CampfireManager.OnFortressSecureEvent += OnFortressSecure;
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        CampfireManager.OnFortressSecureEvent -= OnFortressSecure;
    }

    private void OnFortressSecure()
    {
        ShowNotification("FORTRESS SECURE! \n ENEMIES APPROACHING");
    }

    public void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
        }

        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimateNotification());
    }

    private IEnumerator AnimateNotification()
    {
        // Setup initial state
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true; // Block clicks while visible (optional, per requirements)
            canvasGroup.alpha = 0f;
        }
        
        transform.localScale = Vector3.one * 0.5f;

        // IN: Pop and Fade (0 to 1)
        float timer = 0f;
        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float t = timer / popDuration;
            
            // Optional: Add basic easing for "pop" effect (SmoothStep)
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothT);
            transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, smoothT);

            yield return null;
        }

        // Ensure final values
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;

        // WAIT
        yield return new WaitForSeconds(displayDuration);

        // OUT: Fade (1 to 0)
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutDuration;

            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        // Finalize
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false; // Disable blocking when hidden
        }
    }
}
