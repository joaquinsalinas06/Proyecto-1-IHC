using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UICharacterClickHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Click Detection")]
    [Tooltip("Reference to the SessionManager to notify when clicked")]
    public SessionManager sessionManager;
    
    [Header("Visual Feedback")]
    [Tooltip("Scale multiplier when character is pressed")]
    public float pressedScale = 0.9f;
    
    [Tooltip("Duration of the press animation")]
    public float pressAnimationDuration = 0.1f;
    
    private Vector3 originalScale;
    private bool isPressed = false;
    
    void Start()
    {
        // Store the original scale
        originalScale = transform.localScale;
        
        // Auto-find SessionManager if not assigned
        if (sessionManager == null)
        {
            sessionManager = FindObjectOfType<SessionManager>();
        }
        
        Debug.Log($"UICharacterClickHandler initialized for UI element: {gameObject.name}");
        Debug.Log($"SessionManager found: {sessionManager != null}");
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"UI Character {gameObject.name} was clicked!");
        OnCharacterClicked();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"UI Character {gameObject.name} pointer down!");
        OnCharacterPressed();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"UI Character {gameObject.name} pointer up!");
        OnCharacterReleased();
    }
    
    void OnCharacterPressed()
    {
        if (!isPressed)
        {
            isPressed = true;
            // Animate scale down for visual feedback
            StartCoroutine(AnimateScale(originalScale * pressedScale));
        }
    }
    
    void OnCharacterReleased()
    {
        if (isPressed)
        {
            isPressed = false;
            // Animate scale back to original
            StartCoroutine(AnimateScale(originalScale));
        }
    }
    
    void OnCharacterClicked()
    {
        if (sessionManager != null)
        {
            Debug.Log($"Calling SessionManager.OnCharacterClicked() from {gameObject.name}");
            sessionManager.OnCharacterClicked();
        }
        else
        {
            Debug.LogWarning($"SessionManager not found for {gameObject.name}");
        }
    }
    
    System.Collections.IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < pressAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / pressAnimationDuration;
            
            // Smooth animation curve
            progress = Mathf.SmoothStep(0f, 1f, progress);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
}