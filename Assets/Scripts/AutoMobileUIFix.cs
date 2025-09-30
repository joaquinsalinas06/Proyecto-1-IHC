using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically fixes common mobile UI scaling issues at runtime
/// Add this component to your main Canvas or any GameObject in the scene
/// </summary>
public class AutoMobileUIFix : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    [Tooltip("Automatically fix Canvas Scaler on Start")]
    public bool fixCanvasScalerOnStart = true;
    
    [Tooltip("Automatically fix button positions on Start")]
    public bool fixButtonPositionsOnStart = true;
    
    [Tooltip("Automatically fix slider bounds on Start")]
    public bool fixSliderBoundsOnStart = true;
    
    [Header("Canvas Scaler Override")]
    [Tooltip("Use custom reference resolution instead of default")]
    public bool useCustomResolution = true;
    public Vector2 customReferenceResolution = new Vector2(1920, 1080);
    
    [Range(0f, 1f)]
    [Tooltip("0 = Match Width, 1 = Match Height, 0.5 = Balanced")]
    public float matchWidthOrHeight = 0.5f;

    void Start()
    {
        // Apply fixes on start
        if (fixCanvasScalerOnStart)
        {
            FixCanvasScaler();
        }

        if (fixButtonPositionsOnStart)
        {
            FixButtonPositions();
        }

        if (fixSliderBoundsOnStart)
        {
            FixSliderBounds();
        }
    }

    [ContextMenu("Apply All Mobile Fixes")]
    public void ApplyAllMobileFixes()
    {
        FixCanvasScaler();
        FixButtonPositions();
        FixSliderBounds();
        Debug.Log("All mobile UI fixes applied!");
    }

    void FixCanvasScaler()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("AutoMobileUIFix: No Canvas found in scene");
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            Debug.LogWarning("AutoMobileUIFix: No CanvasScaler found on Canvas");
            return;
        }

        // Configure for mobile compatibility
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
        if (useCustomResolution)
        {
            scaler.referenceResolution = customReferenceResolution;
        }
        else
        {
            // Default mobile-friendly resolution
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = matchWidthOrHeight;

        Debug.Log($"Canvas Scaler fixed: Resolution {scaler.referenceResolution}, Match: {matchWidthOrHeight}");
    }

    void FixButtonPositions()
    {
        Button[] buttons = FindObjectsOfType<Button>();
        
        foreach (Button button in buttons)
        {
            // Check if this is a control button (iniciar, reiniciar, etc.)
            string buttonName = button.name.ToLower();
            if (buttonName.Contains("iniciar") || 
                buttonName.Contains("reiniciar") ||
                buttonName.Contains("play") || 
                buttonName.Contains("pause") ||
                buttonName.Contains("reset"))
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // Check for overlap with timer elements
                    CheckAndFixButtonOverlap(buttonRect, button.name);
                }
            }
        }
        
        Debug.Log($"Button positions checked and fixed for {buttons.Length} buttons");
    }

    void CheckAndFixButtonOverlap(RectTransform buttonRect, string buttonName)
    {
        // Find potential timer UI elements
        GameObject[] timerObjects = {
            GameObject.Find("TimerPanel"),
            GameObject.Find("Timer Panel"),
            GameObject.Find("Panel"),
            GameObject.Find("TimerText"),
            GameObject.Find("Timer")
        };

        foreach (GameObject timerObj in timerObjects)
        {
            if (timerObj != null)
            {
                RectTransform timerRect = timerObj.GetComponent<RectTransform>();
                if (timerRect != null && CheckOverlap(buttonRect, timerRect))
                {
                    // Move button to avoid overlap
                    Vector2 newPosition = CalculateNonOverlappingPosition(buttonRect, timerRect);
                    buttonRect.anchoredPosition = newPosition;
                    Debug.Log($"Moved {buttonName} to avoid overlap with {timerObj.name}");
                    break;
                }
            }
        }
    }

    bool CheckOverlap(RectTransform rect1, RectTransform rect2)
    {
        // Convert to world space for accurate overlap detection
        Vector3[] corners1 = new Vector3[4];
        Vector3[] corners2 = new Vector3[4];
        
        rect1.GetWorldCorners(corners1);
        rect2.GetWorldCorners(corners2);
        
        // Check if rectangles overlap
        return !(corners1[2].x < corners2[0].x || corners1[0].x > corners2[2].x ||
                corners1[2].y < corners2[0].y || corners1[0].y > corners2[2].y);
    }

    Vector2 CalculateNonOverlappingPosition(RectTransform buttonRect, RectTransform timerRect)
    {
        Vector2 currentPos = buttonRect.anchoredPosition;
        Vector2 timerPos = timerRect.anchoredPosition;
        Vector2 timerSize = timerRect.sizeDelta;
        Vector2 buttonSize = buttonRect.sizeDelta;
        
        // Try to place button below timer with some padding
        float newY = timerPos.y - (timerSize.y / 2) - (buttonSize.y / 2) - 20; // 20px padding
        
        return new Vector2(currentPos.x, newY);
    }

    void FixSliderBounds()
    {
        Slider[] sliders = FindObjectsOfType<Slider>();
        
        foreach (Slider slider in sliders)
        {
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            if (sliderRect != null)
            {
                // Find parent panel
                Transform parentPanel = FindParentPanel(sliderRect.transform);
                if (parentPanel != null)
                {
                    RectTransform panelRect = parentPanel.GetComponent<RectTransform>();
                    if (panelRect != null)
                    {
                        EnsureSliderFitsInPanel(sliderRect, panelRect, slider.name);
                    }
                }
            }
        }
        
        Debug.Log($"Slider bounds checked and fixed for {sliders.Length} sliders");
    }

    Transform FindParentPanel(Transform child)
    {
        Transform current = child.parent;
        
        while (current != null)
        {
            string name = current.name.ToLower();
            if (name.Contains("panel") || current.GetComponent<Image>() != null)
            {
                return current;
            }
            current = current.parent;
        }
        
        return null;
    }

    void EnsureSliderFitsInPanel(RectTransform sliderRect, RectTransform panelRect, string sliderName)
    {
        // Get local bounds
        Vector3[] sliderCorners = new Vector3[4];
        sliderRect.GetWorldCorners(sliderCorners);
        
        // Convert to panel's local space
        Vector2 sliderLocalMin = panelRect.InverseTransformPoint(sliderCorners[0]);
        Vector2 sliderLocalMax = panelRect.InverseTransformPoint(sliderCorners[2]);
        
        Vector2 panelSize = panelRect.sizeDelta;
        Vector2 currentPos = sliderRect.anchoredPosition;
        bool needsAdjustment = false;
        
        // Check horizontal bounds
        float margin = 20f; // pixels
        if (sliderLocalMax.x > panelSize.x / 2 - margin)
        {
            currentPos.x -= (sliderLocalMax.x - (panelSize.x / 2 - margin));
            needsAdjustment = true;
        }
        
        if (sliderLocalMin.x < -(panelSize.x / 2 - margin))
        {
            currentPos.x += (-(panelSize.x / 2 - margin) - sliderLocalMin.x);
            needsAdjustment = true;
        }
        
        // Check vertical bounds
        if (sliderLocalMax.y > panelSize.y / 2 - margin)
        {
            currentPos.y -= (sliderLocalMax.y - (panelSize.y / 2 - margin));
            needsAdjustment = true;
        }
        
        if (sliderLocalMin.y < -(panelSize.y / 2 - margin))
        {
            currentPos.y += (-(panelSize.y / 2 - margin) - sliderLocalMin.y);
            needsAdjustment = true;
        }
        
        if (needsAdjustment)
        {
            sliderRect.anchoredPosition = currentPos;
            Debug.Log($"Adjusted {sliderName} position to fit within panel bounds");
        }
    }

    // For testing in the editor
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyAllMobileFixes();
        }
    }
}