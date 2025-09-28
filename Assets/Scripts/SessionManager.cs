using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI phraseText;
    public Button playPauseButton;
    public Button resetButton;
    public Slider progressBar;
    public TextMeshProUGUI instructionsText;
    
    [Header("Session Settings")]
    public float sessionDuration = 1500f;
    public float breakDuration = 300f;
    
    private float currentTime;
    private float originalSessionDuration;
    private bool isRunning = false;
    private bool isBreakTime = false;
    private int sessionPhraseIndex = 0;
    private bool resetConfirmationPending = false;
    private float resetConfirmationTime;
    
    private string initialMessage = "Prepárate para tu sesión de productividad";
    private string instructionsMessage = "Toca el texto para cambiar frases durante la sesión";
    
    private string[] sessionPhrases = {
        "Concéntrate en tu respiración y en tus objetivos",
        "Cada minuto cuenta para tu crecimiento",
        "Mantén la calma y sigue enfocado",
        "Tu mente está clara y preparada",
        "Este es tu momento de productividad",
        "Respira profundo y continúa trabajando"
    };
    
    void Start()
    {
        originalSessionDuration = sessionDuration;
        currentTime = sessionDuration;
        UpdateTimerDisplay();
        UpdateProgressBar();
        
        phraseText.text = initialMessage;
        instructionsText.text = instructionsMessage;
        
        UpdateButtonStates();
        
        Button textButton = phraseText.GetComponent<Button>();
        if (textButton == null)
        {
            textButton = phraseText.gameObject.AddComponent<Button>();
        }
        textButton.onClick.AddListener(OnTextClicked);
    }
    
    void Update()
    {
        if (isRunning && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
            UpdateProgressBar();
        }
        else if (isRunning && currentTime <= 0)
        {
            SessionComplete();
        }
        
        if (resetConfirmationPending && Time.time > resetConfirmationTime + 3f)
        {
            CancelResetConfirmation();
        }
    }
    
    void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            float totalDuration = isBreakTime ? breakDuration : originalSessionDuration;
            float progress = 1f - (currentTime / totalDuration);
            progressBar.value = Mathf.Clamp01(progress);
        }
    }
    
    void UpdateButtonStates()
    {
        playPauseButton.interactable = true;
        resetButton.interactable = currentTime != originalSessionDuration;
        
        var playPauseText = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();
        if (!isRunning && currentTime == originalSessionDuration)
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
    
    public void OnTextClicked()
    {
        if (isRunning && !isBreakTime)
        {
            NextPhrase();
        }
    }
    
    public void TogglePlayPause()
    {
        if (!isRunning && currentTime == originalSessionDuration)
        {
            StartSession();
        }
        else
        {
            PauseResumeSession();
        }
        
        if (resetConfirmationPending)
        {
            CancelResetConfirmation();
        }
    }
    
    void StartSession()
    {
        isRunning = true;
        sessionPhraseIndex = 0;
        phraseText.text = sessionPhrases[sessionPhraseIndex];
        instructionsText.text = "Toca el texto para cambiar la frase motivacional";
        
        UpdateButtonStates();
    }
    
    void PauseResumeSession()
    {
        isRunning = !isRunning;
        UpdateButtonStates();
    }

    public void ResetSession()
    {
        if (resetConfirmationPending)
        {
            ConfirmReset();
        }
        else
        {
            if (currentTime != originalSessionDuration)
            {
                StartResetConfirmation();
            }
        }
    }
    
    void StartResetConfirmation()
    {
        resetConfirmationPending = true;
        resetConfirmationTime = Time.time;
        
        var buttonText = resetButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "¿Confirmar?";
        
        instructionsText.text = "Presiona 'Confirmar' nuevamente para reiniciar";
    }
    
    void CancelResetConfirmation()
    {
        resetConfirmationPending = false;
        
        var buttonText = resetButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "Reiniciar";
        
        if (isRunning)
        {
            instructionsText.text = "Toca el texto para cambiar la frase motivacional";
        }
        else
        {
            instructionsText.text = instructionsMessage;
        }
    }
    
    void ConfirmReset()
    {
        isRunning = false;
        isBreakTime = false;
        currentTime = sessionDuration;
        originalSessionDuration = sessionDuration;
        sessionPhraseIndex = 0;
        
        phraseText.text = initialMessage;
        instructionsText.text = instructionsMessage;
        
        CancelResetConfirmation();
        UpdateButtonStates();
        UpdateProgressBar();
    }
    
    void SessionComplete()
    {
        CancelResetConfirmation();
        
        if (!isBreakTime)
        {
            isBreakTime = true;
            currentTime = breakDuration;
            originalSessionDuration = breakDuration;
            phraseText.text = "¡Sesión completada! Tiempo de descanso";
            instructionsText.text = "Disfruta tu merecido descanso";
            
            // Iniciar automáticamente el break
            isRunning = true;
        }
        else
        {
            isRunning = false;
            isBreakTime = false;
            currentTime = sessionDuration;
            originalSessionDuration = sessionDuration;
            phraseText.text = "¡Descanso terminado! Nueva sesión";
            instructionsText.text = instructionsMessage;
        }
        
        UpdateTimerDisplay();
        UpdateButtonStates();
        UpdateProgressBar();
    }
    
    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    void NextPhrase()
    {
        sessionPhraseIndex = (sessionPhraseIndex + 1) % sessionPhrases.Length;
        phraseText.text = sessionPhrases[sessionPhraseIndex];
    }
}