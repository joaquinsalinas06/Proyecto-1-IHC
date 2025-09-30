using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Comprehensive mobile UI compatibility script
/// Handles common scaling and positioning issues when building to mobile platforms
/// </summary>
[System.Serializable]
public class MobileUIManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private bool applyFixesInEditor = false;
    
    [Header("Canvas Settings")]
    [SerializeField] private Vector2 mobileReferenceResolution = new Vector2(1920, 1080);
    [SerializeField] private float mobileMatchWidthOrHeight = 0.5f;
    
    [Header("Safe Area")]
    [SerializeField] private bool enableSafeArea = true;
    [SerializeField] private RectTransform safeAreaPanel;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Canvas mainCanvas;
    private CanvasScaler canvasScaler;
    private bool isMobilePlatform;

    void Start()
    {
        DetectPlatformAndApplyFixes();
    }

    void DetectPlatformAndApplyFixes()
    {
        // Check if we're on a mobile platform
        isMobilePlatform = Application.platform == RuntimePlatform.Android || 
                          Application.platform == RuntimePlatform.IPhonePlayer;

        if (showDebugLogs)
        {
            Debug.Log($"Platform detected: {Application.platform}, Is Mobile: {isMobilePlatform}");
        }

        // Apply fixes if on mobile or if editor testing is enabled
        if (isMobilePlatform || (applyFixesInEditor && Application.isEditor))
        {
            ApplyMobileFixes();
        }
    }

    [ContextMenu("Apply Mobile Fixes")]
    public void ApplyMobileFixes()
    {
        if (showDebugLogs)
        {
            Debug.Log("Applying mobile UI fixes...");
        }

        // Fix Canvas Scaler
        FixCanvasScaling();
        
        // Apply safe area
        if (enableSafeArea)
        {
            SetupSafeArea();
        }
        
        // Fix UI element positions
        FixUIElementPositions();
        
        if (showDebugLogs)
        {
            Debug.Log("Mobile UI fixes applied successfully!");
        }
    }

    void FixCanvasScaling()
    {
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("MobileUIManager: No Canvas found!");
            return;
        }

        canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            Debug.LogError("MobileUIManager: No CanvasScaler found on Canvas!");
            return;
        }

        // Configure Canvas Scaler for mobile
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = mobileReferenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = mobileMatchWidthOrHeight;
        
        if (showDebugLogs)
        {
            Debug.Log($"Canvas Scaler configured: {mobileReferenceResolution}, Match: {mobileMatchWidthOrHeight}");
        }
    }

    void SetupSafeArea()
    {
        if (safeAreaPanel == null)
        {
            // Create safe area panel if it doesn't exist
            GameObject safeAreaGO = new GameObject("SafeAreaPanel");
            safeAreaGO.transform.SetParent(mainCanvas.transform, false);
            safeAreaPanel = safeAreaGO.AddComponent<RectTransform>();
        }

        // Get safe area rect
        Rect safeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // Calculate anchor values
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= screenSize.x;
        anchorMin.y /= screenSize.y;
        anchorMax.x /= screenSize.x;
        anchorMax.y /= screenSize.y;

        // Apply to safe area panel
        safeAreaPanel.anchorMin = anchorMin;
        safeAreaPanel.anchorMax = anchorMax;
        safeAreaPanel.offsetMin = Vector2.zero;
        safeAreaPanel.offsetMax = Vector2.zero;

        if (showDebugLogs)
        {
            Debug.Log($"Safe area applied: {safeArea} (Screen: {screenSize})");
        }
    }

    void FixUIElementPositions()
    {
        FixButtonOverlaps();
        FixSliderBounds();
        FixTextOverflows();
    }

    void FixButtonOverlaps()
    {
        Button[] buttons = FindObjectsOfType<Button>();
        
        foreach (Button button in buttons)
        {
            if (IsControlButton(button))
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    FixButtonPosition(buttonRect, button.name);
                }
            }
        }
    }

    bool IsControlButton(Button button)
    {
        string name = button.name.ToLower();
        return name.Contains("iniciar") || name.Contains("reiniciar") || 
               name.Contains("play") || name.Contains("pause") || 
               name.Contains("reset") || name.Contains("start") || 
               name.Contains("stop");
    }

    void FixButtonPosition(RectTransform buttonRect, string buttonName)
    {
        // Find timer or overlapping UI elements
        GameObject[] potentialOverlaps = {
            GameObject.Find("TimerPanel"),
            GameObject.Find("Timer Panel"), 
            GameObject.Find("Timer"),
            GameObject.Find("TimerText")
        };

        foreach (GameObject obj in potentialOverlaps)
        {
            if (obj != null)
            {
                RectTransform overlapRect = obj.GetComponent<RectTransform>();
                if (overlapRect != null && CheckUIOverlap(buttonRect, overlapRect))
                {
                    // Move button to non-overlapping position
                    Vector2 safePosition = CalculateSafePosition(buttonRect, overlapRect);
                    buttonRect.anchoredPosition = safePosition;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"Moved button '{buttonName}' to avoid overlap with '{obj.name}'");
                    }
                    break;
                }
            }
        }
    }

    bool CheckUIOverlap(RectTransform rect1, RectTransform rect2)
    {
        Vector3[] corners1 = new Vector3[4];
        Vector3[] corners2 = new Vector3[4];
        
        rect1.GetWorldCorners(corners1);
        rect2.GetWorldCorners(corners2);

        // Simple AABB overlap check
        return !(corners1[2].x < corners2[0].x || corners1[0].x > corners2[2].x ||
                corners1[2].y < corners2[0].y || corners1[0].y > corners2[2].y);
    }

    Vector2 CalculateSafePosition(RectTransform element, RectTransform obstacle)
    {
        Vector2 currentPos = element.anchoredPosition;
        Vector2 obstaclePos = obstacle.anchoredPosition;
        Vector2 obstacleSize = obstacle.sizeDelta;
        Vector2 elementSize = element.sizeDelta;

        // Calculate position below the obstacle
        float safeY = obstaclePos.y - (obstacleSize.y / 2) - (elementSize.y / 2) - 30; // 30px margin
        
        return new Vector2(currentPos.x, safeY);
    }

    void FixSliderBounds()
    {
        Slider[] sliders = FindObjectsOfType<Slider>();
        
        foreach (Slider slider in sliders)
        {
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            if (sliderRect != null)
            {
                ConstrainSliderToParent(sliderRect, slider.name);
            }
        }
    }

    void ConstrainSliderToParent(RectTransform sliderRect, string sliderName)
    {
        // Find immediate parent with bounds (panel, etc.)
        Transform parent = sliderRect.parent;
        RectTransform parentRect = null;

        while (parent != null)
        {
            RectTransform rect = parent.GetComponent<RectTransform>();
            if (rect != null && (parent.name.ToLower().Contains("panel") || parent.GetComponent<Image>()))
            {
                parentRect = rect;
                break;
            }
            parent = parent.parent;
        }

        if (parentRect != null)
        {
            // Check and constrain bounds
            Vector2 parentSize = parentRect.sizeDelta;
            Vector2 sliderSize = sliderRect.sizeDelta;
            Vector2 currentPos = sliderRect.anchoredPosition;
            
            float margin = 20f;
            float halfParentWidth = (parentSize.x / 2) - margin;
            float halfSliderWidth = sliderSize.x / 2;
            
            // Constrain X position
            if (currentPos.x + halfSliderWidth > halfParentWidth)
            {
                currentPos.x = halfParentWidth - halfSliderWidth;
            }
            else if (currentPos.x - halfSliderWidth < -halfParentWidth)
            {
                currentPos.x = -halfParentWidth + halfSliderWidth;
            }
            
            sliderRect.anchoredPosition = currentPos;
            
            if (showDebugLogs && currentPos != sliderRect.anchoredPosition)
            {
                Debug.Log($"Constrained slider '{sliderName}' within parent bounds");
            }
        }
    }

    void FixTextOverflows()
    {
        // Fix TextMeshPro components that might overflow on mobile
        TMPro.TextMeshProUGUI[] texts = FindObjectsOfType<TMPro.TextMeshProUGUI>();
        
        foreach (var text in texts)
        {
            // Enable auto-sizing for mobile compatibility
            if (text.enableAutoSizing == false)
            {
                text.enableAutoSizing = true;
                text.fontSizeMin = text.fontSize * 0.5f;
                text.fontSizeMax = text.fontSize;
                
                if (showDebugLogs)
                {
                    Debug.Log($"Enabled auto-sizing for text '{text.name}'");
                }
            }
        }
    }

    // Public methods for manual fixes
    [ContextMenu("Fix Canvas Only")]
    public void FixCanvasOnly()
    {
        FixCanvasScaling();
    }

    [ContextMenu("Fix Buttons Only")]
    public void FixButtonsOnly()
    {
        FixButtonOverlaps();
    }

    [ContextMenu("Fix Sliders Only")]
    public void FixSlidersOnly()
    {
        FixSliderBounds();
    }

    // Update method to handle runtime changes
    void Update()
    {
        // Check for orientation or resolution changes
        if (enableSafeArea && safeAreaPanel != null)
        {
            SetupSafeArea();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (applyFixesInEditor && Application.isPlaying)
        {
            ApplyMobileFixes();
        }
    }
#endif
}