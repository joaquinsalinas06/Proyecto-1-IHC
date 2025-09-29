using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ProjectRestorer : MonoBehaviour
{
    [Header("Restoration Settings")]
    public bool fixCharacterOrientations = true;
    public bool fixUIFonts = true;
    public bool enableButtons = true;
    public bool resetTungTungToNaturalPose = true;
    public bool fixMobileUI = true;
    [Header("Character References")]
    public GameObject capybaraGameObject;
    public GameObject tungTungGameObject;
    [Header("Font Settings")]
    public TMP_FontAsset rethinkSansFont;
    [Header("UI Elements to Fix")]
    public Button[] buttonsToEnable;
    public TextMeshProUGUI[] textsToFixFont;
    void Start()
    {
        Invoke("StartRestoration", 0.5f);
    }
    [ContextMenu("Start Restoration Process")]
    public void StartRestoration()
    {
        if (fixCharacterOrientations)
        {
            FixCharacterOrientations();
        }
        if (fixUIFonts)
        {
            FixUIFonts();
        }
        if (enableButtons)
        {
            EnableAllButtons();
        }
        if (resetTungTungToNaturalPose)
        {
            ResetTungTungPose();
        }
        if (fixMobileUI)
        {
            FixMobileUILayout();
        }
    }
    void FixCharacterOrientations()
    {
        if (capybaraGameObject != null)
        {
            capybaraGameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        if (tungTungGameObject != null)
        {
            tungTungGameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    void FixUIFonts()
    {
        if (rethinkSansFont == null)
        {
            rethinkSansFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/RethinkSans-VariableFont_wght SDF");
            if (rethinkSansFont == null)
            {
                var fontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                foreach (var font in fontAssets)
                {
                    if (font.name.Contains("RethinkSans") || font.name.Contains("Rethink"))
                    {
                        rethinkSansFont = font;
                        break;
                    }
                }
            }
            if (rethinkSansFont == null)
            {
                rethinkSansFont = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime.fontsettings");
            }
        }
        if (textsToFixFont != null)
        {
            foreach (var text in textsToFixFont)
            {
                if (text != null)
                {
                    text.font = rethinkSansFont;
                }
            }
        }
        var allTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text.font == null || text.font.name.Contains("LiberationSans"))
            {
                text.font = rethinkSansFont;
            }
            if (text.transform.parent != null && text.transform.parent.GetComponent<Button>() != null)
            {
                text.color = Color.white;
                text.fontSize = Mathf.Max(text.fontSize, 14f);
                text.fontStyle = FontStyles.Bold;
            }
        }
    }
    void EnableAllButtons()
    {
        if (buttonsToEnable != null)
        {
            foreach (var button in buttonsToEnable)
            {
                if (button != null)
                {
                    button.interactable = true;
                }
            }
        }
        var allButtons = FindObjectsOfType<Button>();
        foreach (var button in allButtons)
        {
            button.interactable = true;
        }
    }
    void ResetTungTungPose()
    {
        if (tungTungGameObject != null)
        {
            var simpleTungTungAnimator = tungTungGameObject.GetComponent<SimpleTungTungAnimator>();
            if (simpleTungTungAnimator != null)
            {
            }
            else
            {
                tungTungGameObject.AddComponent<SimpleTungTungAnimator>();
            }
        }
    }
    [ContextMenu("Debug Scene State")]
    public void DebugSceneState()
    {
        if (capybaraGameObject != null)
        {
        }
        if (tungTungGameObject != null)
        {
        }
    }
    [ContextMenu("Force Font Refresh")]
    public void ForceRefreshFonts()
    {
        var allTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            text.SetAllDirty();
            text.Rebuild(CanvasUpdate.PreRender);
        }
    }
    void FixMobileUILayout()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
    [ContextMenu("Fix Mobile UI Only")]
    public void FixMobileUIOnly()
    {
        FixMobileUILayout();
    }
}

