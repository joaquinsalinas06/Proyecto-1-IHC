using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CustomButtonController : MonoBehaviour
{
    [Header("Button References")]
    public Button playPauseButton;
    public Button resetButton;
    [Header("Button Colors (You can edit these!)")]
    public Color iniciarColor = new Color(0.2f, 0.8f, 0.3f, 1f);
    public Color pausarColor = new Color(0.3f, 0.6f, 0.9f, 1f);
    public Color continuarColor = new Color(0.8f, 0.6f, 0.2f, 1f);
    public Color reiniciarColor = new Color(0.8f, 0.3f, 0.3f, 1f);
    public Color confirmarColor = new Color(0.9f, 0.5f, 0.1f, 1f);
    [Header("Control Settings")]
    public bool enableAutomaticColorChanges = true;
    public bool enableAutomaticTextChanges = true;
    private SessionManager sessionManager;
    private Image playPauseImage;
    private Image resetImage;
    private TextMeshProUGUI playPauseText;
    private TextMeshProUGUI resetText;
    private bool lastIsRunning = false;
    private bool lastResetConfirmation = false;
    void Start()
    {
        sessionManager = FindObjectOfType<SessionManager>();
        if (playPauseButton != null)
        {
            playPauseImage = playPauseButton.GetComponent<Image>();
            playPauseText = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();
        }
        if (resetButton != null)
        {
            resetImage = resetButton.GetComponent<Image>();
            resetText = resetButton.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    void Update()
    {
        if (sessionManager == null) return;
        bool currentIsRunning = GetSessionManagerBool("isRunning");
        bool currentResetConfirmation = GetSessionManagerBool("resetConfirmationPending");
        if (currentIsRunning != lastIsRunning || currentResetConfirmation != lastResetConfirmation)
        {
            if (enableAutomaticColorChanges || enableAutomaticTextChanges)
            {
                UpdatePlayPauseButton();
                UpdateResetButton();
            }
            lastIsRunning = currentIsRunning;
            lastResetConfirmation = currentResetConfirmation;
        }
    }
    void UpdatePlayPauseButton()
    {
        if (playPauseButton == null) return;
        bool isRunning = GetSessionManagerBool("isRunning");
        float currentTime = GetSessionManagerFloat("currentTime");
        float originalSessionDuration = GetSessionManagerFloat("originalSessionDuration");
        if (Time.frameCount % 60 == 0)
        {
        }
        if (enableAutomaticTextChanges && playPauseText != null)
        {
            if (!isRunning && Mathf.Approximately(currentTime, originalSessionDuration))
            {
                playPauseText.text = "Iniciar";
            }
            else if (isRunning)
            {
                playPauseText.text = "Pausar";
            }
            else
            {
                playPauseText.text = "Continuar";
            }
        }
        if (enableAutomaticColorChanges && playPauseImage != null)
        {
            if (!isRunning && Mathf.Approximately(currentTime, originalSessionDuration))
            {
                playPauseImage.color = iniciarColor;
            }
            else if (isRunning)
            {
                playPauseImage.color = pausarColor;
            }
            else
            {
                playPauseImage.color = continuarColor;
            }
        }
    }
    void UpdateResetButton()
    {
        if (resetButton == null) return;
        bool resetConfirmationPending = GetSessionManagerBool("resetConfirmationPending");
        if (enableAutomaticColorChanges && resetImage != null)
        {
            if (!resetConfirmationPending)
            {
                resetImage.color = reiniciarColor;
            }
            else
            {
                resetImage.color = confirmarColor;
            }
        }
    }
    bool GetSessionManagerBool(string fieldName)
    {
        if (sessionManager == null) return false;
        var field = sessionManager.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null && field.FieldType == typeof(bool))
        {
            return (bool)field.GetValue(sessionManager);
        }
        return false;
    }
    float GetSessionManagerFloat(string fieldName)
    {
        if (sessionManager == null) return 0f;
        var field = sessionManager.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null && field.FieldType == typeof(float))
        {
            return (float)field.GetValue(sessionManager);
        }
        return 0f;
    }
    T GetSessionManagerField<T>(string fieldName)
    {
        if (sessionManager == null) return default(T);
        var field = sessionManager.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null && field.FieldType == typeof(T))
        {
            return (T)field.GetValue(sessionManager);
        }
        return default(T);
    }
    [ContextMenu("Set Play Button to Iniciar")]
    public void SetPlayButtonToIniciar()
    {
        if (playPauseImage != null) playPauseImage.color = iniciarColor;
        if (playPauseText != null) playPauseText.text = "Iniciar";
    }
    [ContextMenu("Set Play Button to Pausar")]
    public void SetPlayButtonToPausar()
    {
        if (playPauseImage != null) playPauseImage.color = pausarColor;
        if (playPauseText != null) playPauseText.text = "Pausar";
    }
    [ContextMenu("Set Play Button to Continuar")]
    public void SetPlayButtonToContinuar()
    {
        if (playPauseImage != null) playPauseImage.color = continuarColor;
        if (playPauseText != null) playPauseText.text = "Continuar";
    }
    [ContextMenu("Set Reset Button to Reiniciar")]
    public void SetResetButtonToReiniciar()
    {
        if (resetImage != null) resetImage.color = reiniciarColor;
        if (resetText != null) resetText.text = "Reiniciar";
    }
    [ContextMenu("Set Reset Button to Confirmar")]
    public void SetResetButtonToConfirmar()
    {
        if (resetImage != null) resetImage.color = confirmarColor;
        if (resetText != null) resetText.text = "¿Confirmar?";
    }
    public void ForceUpdateButtonStates()
    {
        UpdatePlayPauseButton();
        UpdateResetButton();
    }
    [ContextMenu("IMMEDIATE TEST: Toggle Button Colors")]
    public void ImmediateToggleTest()
    {
        if (playPauseImage != null)
        {
            if (playPauseImage.color == iniciarColor)
            {
                playPauseImage.color = pausarColor;
                playPauseText.text = "Pausar";
            }
            else
            {
                playPauseImage.color = iniciarColor;
                playPauseText.text = "Iniciar";
            }
        }
    }

}

