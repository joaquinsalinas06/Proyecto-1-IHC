using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MobileUIFixer : MonoBehaviour
{
    [Header("Canvas Scaler Settings")]
    [Tooltip("Reference resolution for consistent UI scaling across devices")]
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    
    [Tooltip("How to scale the canvas when the screen size doesn't match the reference resolution")]
    public CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    
    [Tooltip("Match width (0) or height (1) or a blend (0.5)")]
    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0.5f;

    [Header("UI Safe Area")]
    [Tooltip("Apply safe area adjustments for notched devices")]
    public bool applySafeArea = true;
    
    [Header("Button Fixes")]
    [Tooltip("Automatically fix button scaling and positioning")]
    public bool fixButtonPositions = true;
    
    [Header("Slider Fixes")]
    [Tooltip("Automatically fix slider scaling and ensure they stay within panels")]
    public bool fixSliderPositions = true;

    private Canvas mainCanvas;
    private CanvasScaler canvasScaler;
    private RectTransform safeAreaRect;

    void Start()
    {
        FixMobileUI();
    }

    [ContextMenu("Fix Mobile UI")]
    public void FixMobileUI()
    {
        // Find the main canvas
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("MobileUIFixer: No Canvas found in scene!");
            return;
        }

        // Fix Canvas Scaler
        FixCanvasScaler();

        // Apply safe area if needed
        if (applySafeArea)
        {
            ApplySafeArea();
        }

        // Fix UI element positions
        if (fixButtonPositions)
        {
            FixButtonPositions();
        }

        if (fixSliderPositions)
        {
            FixSliderPositions();
        }

        Debug.Log("Mobile UI fixes applied successfully!");
    }

    void FixCanvasScaler()
    {
        canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            Debug.LogError("MobileUIFixer: No CanvasScaler found on Canvas!");
            return;
        }

        // Configure Canvas Scaler for mobile compatibility
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        canvasScaler.screenMatchMode = screenMatchMode;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        
        Debug.Log($"Canvas Scaler configured: Resolution {referenceResolution}, Match: {matchWidthOrHeight}");
    }

    void ApplySafeArea()
    {
        // Create or find safe area rect
        GameObject safeAreaGO = GameObject.Find("SafeArea");
        if (safeAreaGO == null)
        {
            safeAreaGO = new GameObject("SafeArea");
            safeAreaGO.transform.SetParent(mainCanvas.transform, false);
        }

        safeAreaRect = safeAreaGO.GetComponent<RectTransform>();
        if (safeAreaRect == null)
        {
            safeAreaRect = safeAreaGO.AddComponent<RectTransform>();
        }

        // Configure safe area
        var safeArea = Screen.safeArea;
        var screenSize = new Vector2(Screen.width, Screen.height);
        
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= screenSize.x;
        anchorMin.y /= screenSize.y;
        anchorMax.x /= screenSize.x;
        anchorMax.y /= screenSize.y;

        safeAreaRect.anchorMin = anchorMin;
        safeAreaRect.anchorMax = anchorMax;
        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;

        Debug.Log($"Safe area applied: {safeArea}");
    }

    void FixButtonPositions()
    {
        Button[] buttons = FindObjectsOfType<Button>();
        
        foreach (Button button in buttons)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            
            // Ensure buttons are properly anchored
            if (buttonRect != null)
            {
                // Check if button is likely a timer control button based on name or parent
                if (button.name.ToLower().Contains("iniciar") || 
                    button.name.ToLower().Contains("reiniciar") ||
                    button.name.ToLower().Contains("play") || 
                    button.name.ToLower().Contains("reset") ||
                    button.name.ToLower().Contains("pause"))
                {
                    // Ensure these buttons don't overlap with timer panel
                    AdjustButtonPosition(buttonRect, button.name);
                }
            }
        }
        
        Debug.Log($"Fixed positions for {buttons.Length} buttons");
    }

    void AdjustButtonPosition(RectTransform buttonRect, string buttonName)
    {
        // Get current position
        Vector2 currentPos = buttonRect.anchoredPosition;
        
        // Find timer panel to avoid overlap
        GameObject timerPanel = GameObject.Find("TimerPanel") ?? 
                              GameObject.Find("Timer Panel") ?? 
                              GameObject.Find("Panel");
        
        if (timerPanel != null)
        {
            RectTransform timerRect = timerPanel.GetComponent<RectTransform>();
            if (timerRect != null)
            {
                // Calculate safe positions to avoid overlap
                float timerBottom = timerRect.anchoredPosition.y - (timerRect.sizeDelta.y / 2);
                
                // If button overlaps with timer, move it below
                if (currentPos.y > timerBottom - 100) // 100 pixel buffer
                {
                    buttonRect.anchoredPosition = new Vector2(currentPos.x, timerBottom - 120);
                    Debug.Log($"Adjusted {buttonName} position to avoid timer overlap");
                }
            }
        }
    }

    void FixSliderPositions()
    {
        Slider[] sliders = FindObjectsOfType<Slider>();
        
        foreach (Slider slider in sliders)
        {
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            
            if (sliderRect != null)
            {
                // Find parent panel
                Transform parentPanel = FindParentPanel(sliderRect);
                
                if (parentPanel != null)
                {
                    RectTransform panelRect = parentPanel.GetComponent<RectTransform>();
                    if (panelRect != null)
                    {
                        // Ensure slider fits within panel bounds
                        EnsureSliderFitsInPanel(sliderRect, panelRect, slider.name);
                    }
                }
            }
        }
        
        Debug.Log($"Fixed positions for {sliders.Length} sliders");
    }

    Transform FindParentPanel(Transform transform)
    {
        Transform current = transform.parent;
        
        while (current != null)
        {
            if (current.name.ToLower().Contains("panel") || 
                current.GetComponent<Image>() != null)
            {
                return current;
            }
            current = current.parent;
        }
        
        return null;
    }

    void EnsureSliderFitsInPanel(RectTransform sliderRect, RectTransform panelRect, string sliderName)
    {
        // Get slider bounds relative to panel
        Vector3[] sliderCorners = new Vector3[4];
        Vector3[] panelCorners = new Vector3[4];
        
        sliderRect.GetWorldCorners(sliderCorners);
        panelRect.GetWorldCorners(panelCorners);
        
        // Convert to local space of panel
        Vector2 sliderLocalMin = panelRect.InverseTransformPoint(sliderCorners[0]);
        Vector2 sliderLocalMax = panelRect.InverseTransformPoint(sliderCorners[2]);
        
        Vector2 panelSize = panelRect.sizeDelta;
        Vector2 adjustedPosition = sliderRect.anchoredPosition;
        Vector2 adjustedSize = sliderRect.sizeDelta;
        
        bool positionChanged = false;
        
        // Check if slider extends beyond panel boundaries and adjust
        if (sliderLocalMax.x > panelSize.x / 2)
        {
            adjustedPosition.x -= (sliderLocalMax.x - panelSize.x / 2) + 10; // 10 pixel padding
            positionChanged = true;
        }
        
        if (sliderLocalMin.x < -panelSize.x / 2)
        {
            adjustedPosition.x += (-panelSize.x / 2 - sliderLocalMin.x) + 10;
            positionChanged = true;
        }
        
        if (sliderLocalMax.y > panelSize.y / 2)
        {
            adjustedPosition.y -= (sliderLocalMax.y - panelSize.y / 2) + 10;
            positionChanged = true;
        }
        
        if (sliderLocalMin.y < -panelSize.y / 2)
        {
            adjustedPosition.y += (-panelSize.y / 2 - sliderLocalMin.y) + 10;
            positionChanged = true;
        }
        
        // Apply adjustments if needed
        if (positionChanged)
        {
            sliderRect.anchoredPosition = adjustedPosition;
            Debug.Log($"Adjusted {sliderName} position to fit within panel bounds");
        }
    }

    // Runtime detection and fixes
    void Update()
    {
        // Check for orientation changes or screen size changes
        if (Screen.width != screenSize.x || Screen.height != screenSize.y)
        {
            screenSize = new Vector2(Screen.width, Screen.height);
            if (applySafeArea)
            {
                ApplySafeArea();
            }
        }
    }

    private Vector2 screenSize;

    void Awake()
    {
        screenSize = new Vector2(Screen.width, Screen.height);
    }
}