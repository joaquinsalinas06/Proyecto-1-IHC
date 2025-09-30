using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fixes dropdown text sizing and overflow issues for mobile UI
/// Specifically addresses issues with long character names like "TungTungSahur"
/// </summary>
public class DropdownUIFixer : MonoBehaviour
{
    [Header("Dropdown Settings")]
    [Tooltip("The dropdown to fix")]
    public TMP_Dropdown targetDropdown;
    
    [Header("Text Sizing")]
    [Tooltip("Enable automatic text size adjustment")]
    public bool enableAutoSizing = true;
    
    [Tooltip("Minimum font size for auto-sizing")]
    public float minFontSize = 8f;
    
    [Tooltip("Maximum font size for auto-sizing")]
    public float maxFontSize = 20f;
    
    [Header("Dropdown Dimensions")]
    [Tooltip("Automatically adjust dropdown width to fit content")]
    public bool autoAdjustWidth = true;
    
    [Tooltip("Minimum width for the dropdown")]
    public float minDropdownWidth = 150f;
    
    [Tooltip("Maximum width for the dropdown")]
    public float maxDropdownWidth = 300f;
    
    [Header("Template Settings")]
    [Tooltip("Adjust the dropdown template (list) sizing")]
    public bool adjustTemplateSize = true;
    
    void Start()
    {
        if (targetDropdown == null)
        {
            targetDropdown = GetComponent<TMP_Dropdown>();
        }
        
        if (targetDropdown != null)
        {
            FixDropdownTextSizing();
            FixDropdownDimensions();
            
            // Subscribe to value changes to maintain fixes
            targetDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }
    
    [ContextMenu("Fix Dropdown")]
    public void FixDropdownTextSizing()
    {
        if (targetDropdown == null) return;
        
        // Fix the label (currently selected option display)
        FixLabelText();
        
        // Fix the dropdown template (the list that appears when opened)
        FixTemplateText();
        
        Debug.Log("Dropdown text sizing fixed for: " + targetDropdown.name);
    }
    
    void FixLabelText()
    {
        // Get the label component that shows the selected option
        TextMeshProUGUI labelText = targetDropdown.captionText as TextMeshProUGUI;
        
        if (labelText != null)
        {
            if (enableAutoSizing)
            {
                labelText.enableAutoSizing = true;
                labelText.fontSizeMin = minFontSize;
                labelText.fontSizeMax = maxFontSize;
            }
            
            // Ensure text doesn't overflow
            labelText.overflowMode = TextOverflowModes.Ellipsis;
            labelText.enableWordWrapping = false;
            
            Debug.Log("Fixed label text for dropdown: " + targetDropdown.name);
        }
    }
    
    void FixTemplateText()
    {
        // Get the template (dropdown list)
        RectTransform template = targetDropdown.template;
        
        if (template != null)
        {
            // Find the item text component in the template
            TextMeshProUGUI itemText = template.GetComponentInChildren<TextMeshProUGUI>();
            
            if (itemText != null)
            {
                if (enableAutoSizing)
                {
                    itemText.enableAutoSizing = true;
                    itemText.fontSizeMin = minFontSize;
                    itemText.fontSizeMax = maxFontSize;
                }
                
                itemText.overflowMode = TextOverflowModes.Ellipsis;
                itemText.enableWordWrapping = false;
                
                Debug.Log("Fixed template text for dropdown: " + targetDropdown.name);
            }
            
            // Adjust template size if enabled
            if (adjustTemplateSize)
            {
                AdjustTemplateSize(template);
            }
        }
    }
    
    void FixDropdownDimensions()
    {
        if (!autoAdjustWidth || targetDropdown == null) return;
        
        RectTransform dropdownRect = targetDropdown.GetComponent<RectTransform>();
        
        if (dropdownRect != null)
        {
            // Calculate optimal width based on content
            float optimalWidth = CalculateOptimalWidth();
            
            // Clamp to min/max bounds
            float newWidth = Mathf.Clamp(optimalWidth, minDropdownWidth, maxDropdownWidth);
            
            // Apply new width
            Vector2 sizeDelta = dropdownRect.sizeDelta;
            sizeDelta.x = newWidth;
            dropdownRect.sizeDelta = sizeDelta;
            
            Debug.Log($"Adjusted dropdown width to: {newWidth} for {targetDropdown.name}");
        }
    }
    
    float CalculateOptimalWidth()
    {
        if (targetDropdown == null || targetDropdown.options == null) 
            return minDropdownWidth;
        
        float maxTextWidth = 0f;
        
        // Get the font and size from the label text
        TextMeshProUGUI labelText = targetDropdown.captionText as TextMeshProUGUI;
        TMP_FontAsset font = labelText?.font;
        float fontSize = labelText?.fontSize ?? 14f;
        
        if (font == null) return minDropdownWidth;
        
        // Calculate width for each option
        foreach (TMP_Dropdown.OptionData option in targetDropdown.options)
        {
            if (!string.IsNullOrEmpty(option.text))
            {
                // Estimate text width (this is an approximation)
                float textWidth = EstimateTextWidth(option.text, font, fontSize);
                maxTextWidth = Mathf.Max(maxTextWidth, textWidth);
            }
        }
        
        // Add padding (arrow button, margins, etc.)
        return maxTextWidth + 60f; // 60f accounts for arrow and padding
    }
    
    float EstimateTextWidth(string text, TMP_FontAsset font, float fontSize)
    {
        // Simple estimation - in a real implementation, you'd use TextMeshPro's 
        // text generation settings for more accuracy
        return text.Length * (fontSize * 0.6f);
    }
    
    void AdjustTemplateSize(RectTransform template)
    {
        // Ensure template is wide enough for content
        Vector2 templateSize = template.sizeDelta;
        
        if (autoAdjustWidth)
        {
            float optimalWidth = CalculateOptimalWidth();
            templateSize.x = Mathf.Clamp(optimalWidth, minDropdownWidth, maxDropdownWidth);
        }
        
        // Adjust height based on number of options (optional)
        int optionCount = targetDropdown.options?.Count ?? 0;
        float itemHeight = 30f; // Approximate item height
        float maxHeight = 200f; // Maximum dropdown height
        
        templateSize.y = Mathf.Min(optionCount * itemHeight, maxHeight);
        
        template.sizeDelta = templateSize;
        
        Debug.Log($"Adjusted template size to: {templateSize} for {targetDropdown.name}");
    }
    
    void OnDropdownValueChanged(int newValue)
    {
        // Ensure text stays properly sized after selection changes
        StartCoroutine(DelayedTextFix());
    }
    
    System.Collections.IEnumerator DelayedTextFix()
    {
        // Wait one frame to ensure dropdown has updated
        yield return null;
        FixLabelText();
    }
    
    // Public methods for manual control
    [ContextMenu("Force Refresh All")]
    public void ForceRefreshAll()
    {
        FixDropdownTextSizing();
        FixDropdownDimensions();
    }
    
    [ContextMenu("Auto Find Dropdown")]
    public void AutoFindDropdown()
    {
        if (targetDropdown == null)
        {
            targetDropdown = FindObjectOfType<TMP_Dropdown>();
            if (targetDropdown != null)
            {
                Debug.Log("Found dropdown: " + targetDropdown.name);
            }
        }
    }
    
    // Editor helper for testing specific problematic text
    [ContextMenu("Test Long Text")]
    public void TestLongText()
    {
        if (targetDropdown != null)
        {
            // Test with the problematic "TungTungSahur" text
            var options = targetDropdown.options;
            if (options != null && options.Count > 1)
            {
                options[1].text = "TungTungSahur"; // Assuming this is index 1
                targetDropdown.RefreshShownValue();
                FixDropdownTextSizing();
            }
        }
    }
}