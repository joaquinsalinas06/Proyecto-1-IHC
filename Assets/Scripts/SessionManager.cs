using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI phraseText;
    public Button startButton;
    public Button pauseButton;
    public Button resetButton;
    
    [Header("Session Settings")]
    public float sessionDuration = 1500f; // 25 minutos (Pomodoro)
    public float breakDuration = 300f; // 5 minutos
    
    private float currentTime;
    private bool isRunning = false;
    private bool isBreakTime = false;
    private int sessionPhraseIndex = 0;
    
    // Mensaje inicial separado de las frases de sesión
    private string initialMessage = "Prepárate para tu sesión de productividad";
    
    // Frases solo para durante la sesión activa
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
        currentTime = sessionDuration;
        UpdateTimerDisplay();
        phraseText.text = initialMessage; // Mostrar mensaje inicial
        
        // Agregar componente Button al texto para detectar toques
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
        }
        else if (isRunning && currentTime <= 0)
        {
            SessionComplete();
        }
    }
    
    // Método que se llama cuando se toca el texto
    public void OnTextClicked()
    {
        // Solo cambiar frases si la sesión está activa
        if (isRunning && !isBreakTime)
        {
            NextPhrase();
        }
    }
    
    public void StartSession()
    {
        isRunning = true;
        startButton.interactable = false;
        pauseButton.interactable = true;
        
        // Empezar con la primera frase de sesión
        sessionPhraseIndex = 0;
        phraseText.text = sessionPhrases[sessionPhraseIndex];
    }
    
    public void PauseSession()
    {
        isRunning = !isRunning;
        
        // Cambiar texto del botón
        var buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = isRunning ? "Pausar" : "Continuar";
    }

    public void ResetSession()
    {
        isRunning = false;
        currentTime = isBreakTime ? breakDuration : sessionDuration;
        sessionPhraseIndex = 0;
        
        startButton.interactable = true;
        pauseButton.interactable = false;
        
        UpdateTimerDisplay();
        phraseText.text = initialMessage; // Volver al mensaje inicial
        
        var buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "Pausar";
    }
    
    void SessionComplete()
    {
        isRunning = false;
        
        if (!isBreakTime)
        {
            isBreakTime = true;
            currentTime = breakDuration;
            phraseText.text = "¡Sesión completada! Tiempo de descanso";
        }
        else
        {
            isBreakTime = false;
            currentTime = sessionDuration;
            phraseText.text = "¡Descanso terminado! Nueva sesión";
        }
        
        startButton.interactable = true;
        pauseButton.interactable = false;
        UpdateTimerDisplay();
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