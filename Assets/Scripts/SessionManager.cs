using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class SessionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI phraseText;
    public Button playPauseButton;
    public Button resetButton;
    public Slider progressBar;
    public TextMeshProUGUI instructionsText;

    [Header("Character Animation")]
    public SimpleCapybaraAnimator capybaraAnimator;
    public SimpleTungTungAnimator tungTungAnimator;
    [Header("Character Management")]
    public GameObject capybaraGameObject;
    public GameObject tungTungGameObject;
    public bool startWithCapybara = true;
    [Header("UI References")]
    public TMPro.TMP_Dropdown characterDropdown;
    [Header("Automatic Phrase Display")]
    public bool showPhrasesAutomatically = true;
    public float phraseInterval = 30f;
    public bool showPhraseOnSessionStart = true;

    [Header("Character Scaling")]
    public float characterScaleMultiplier = 1f;


    [Header("Progress Bar Colors")]
    public Color progressStartColor = Color.green;
    public Color progressMidColor = Color.yellow;
    public Color progressEndColor = Color.red;
    [Header("Timer Colors")]
    public Color normalTimerColor = Color.black;
    public Color warningTimerColor = Color.red;
    public Color breakTimerColor = Color.blue;
    [Header("Time Selection UI")]
    public GameObject timeSelectionPanel;
    public Slider studyTimeSlider;
    public Slider breakTimeSlider;
    public TextMeshProUGUI studyTimeLabel;
    public TextMeshProUGUI breakTimeLabel;
    public TextMeshProUGUI studyTimeValue;
    public TextMeshProUGUI breakTimeValue;
    [Header("Session Settings")]
    [Tooltip("If checked, uses fixed Inspector values. If unchecked, uses slider values.")]
    public bool useFixedValues = false;
    public float sessionDuration = 1500f;
    public float breakDuration = 300f;
    [Header("Time Selection Ranges")]
    public float minStudyTime = 600f;
    public float maxStudyTime = 3600f;
    public float minBreakTime = 300f;
    public float maxBreakTime = 1800f;
    private float currentTime;
    private float originalSessionDuration;
    private bool isRunning = false;
    private bool isBreakTime = false;
    private bool resetConfirmationPending = false;
    private float resetConfirmationTime;
    private bool isCapybaraActive 
    {
        get 
        {
            return capybaraGameObject != null && capybaraGameObject.activeInHierarchy;
        }
    }
    private bool isTungTungActive
    {
        get
        {
            return tungTungGameObject != null && tungTungGameObject.activeInHierarchy;
        }
    }

    private Coroutine automaticPhraseCoroutine;
    private float lastPhraseTime;
    private string initialMessage = "Prepárate para tu sesión de productividad";
    private string instructionsMessage = "Toca el texto para cambiar frases durante la sesión";
    
    [Header("Motivational Phrases")]
    private string[] idlePhrases = {
        "Prepárate para ser productivo",
        "Tu momento de enfoque está a punto de comenzar",
        "Respira hondo y prepárate para concentrarte",
        "Es hora de alcanzar tus metas",
        "Cada sesión te acerca a tu objetivo",
        "La productividad comienza con tu decisión",
        "Estás a un paso de tu mejor versión",
        "El enfoque es tu superpoder secreto",
        "Hoy es el día perfecto para ser productivo",
        "Tu mente está lista para el desafío"
    };
    
    private string[] activePhrases = {
        "¡Concéntrate al máximo!",
        "¡Cada minuto cuenta!",
        "¡Mantén la calma y sigue!",
        "¡Mente clara, corazón firme!",
        "¡Momento productivo en acción!",
        "¡Respira profundo y continúa!",
        "¡Estás en tu elemento!",
        "¡El flujo está contigo!",
        "¡Cada segundo es una victoria!",
        "¡Tu enfoque es imparable!",
        "¡Construyendo el éxito paso a paso!",
        "¡La disciplina te está transformando!",
        "¡Eres más fuerte que cualquier distracción!",
        "¡El progreso se siente increíble!",
        "¡Tu determinación es inspiradora!"
    };
    
    private string[] startPhrases = {
        "¡Comienza tu jornada de productividad!",
        "¡Es hora de brillar!",
        "¡Vamos a hacer que este tiempo conte!",
        "¡Tu sesión de enfoque ha comenzado!",
        "¡A conquistar tus objetivos!",
        "¡El momento perfecto es ahora!",
        "¡Desata todo tu potencial!",
        "¡La aventura productiva comienza!",
        "¡Tiempo de hacer magia!",
        "¡Tu zona de enfoque te espera!"
    };
    
    private string[] pausePhrases = {
        "Respira y recargas energías",
        "Un descanso merecido para tu mente",
        "Pausa estratégica para volver más fuerte",
        "Relájate, lo estás haciendo genial",
        "Momento de calma y reflexión",
        "Descansa para volver con más energía",
        "Tu mente necesita este respiro",
        "Pausa sabia, regreso poderoso",
        "Recarga completa en proceso",
        "El descanso también es productividad"
    };
    
    private string[] restartPhrases = {
        "¡De vuelta a la acción!",
        "¡Recargado y listo para más!",
        "¡Segunda ronda, misma determinación!",
        "¡Continúamos donde lo dejamos!",
        "¡Renovado y enfocado!",
        "¡El momentum regresa contigo!",
        "¡Vuelta al flujo productivo!",
        "¡Energía restaurada, objetivos claros!",
        "¡Listos para la siguiente fase!",
        "¡El enfoque nunca se fue, solo descansó!"
    };
    
    private int currentPhraseIndex = 0;
    private string[] currentPhraseSet;
    void Start()
    {
        phraseText.text = initialMessage;
        instructionsText.text = instructionsMessage;
        SetupTimeSelectionUI();
        originalSessionDuration = sessionDuration;
        currentTime = sessionDuration;
        UpdateTimerDisplay();
        UpdateProgressBar();
        InitializeCharacters();
        SetupCharacterDropdown();
        SetPhraseSet(idlePhrases);
        UpdateButtonStates();
        Button textButton = phraseText.GetComponent<Button>();
        if (textButton == null)
        {
            textButton = phraseText.gameObject.AddComponent<Button>();
        }
        textButton.onClick.AddListener(OnTextClicked);
    }
    
    void SetPhraseSet(string[] newPhraseSet)
    {
        currentPhraseSet = newPhraseSet;
        currentPhraseIndex = 0;
    }
    
    string GetRandomPhrase(string[] phraseSet)
    {
        if (phraseSet == null || phraseSet.Length == 0) return "";
        return phraseSet[Random.Range(0, phraseSet.Length)];
    }
    
    void ShowRandomPhraseFromSet(string[] phraseSet)
    {
        string randomPhrase = GetRandomPhrase(phraseSet);
        if (!string.IsNullOrEmpty(randomPhrase))
        {
            ShowSpeechBubble(randomPhrase);
        }
    }

    public void ResetToInitialState()
    {
        if (capybaraAnimator != null)
            capybaraAnimator.ResetToOriginalPose();
        if (tungTungAnimator != null)
            tungTungAnimator.ResetToNaturalPose();
        if (isRunning)
        {
            ToggleSessionTimer();
        }
        currentTime = sessionDuration;
        UpdateTimerDisplay();
        if (startWithCapybara)
        {
            ShowCapybara();
        }
        else
        {
            ShowTungTung();
        }
    }

    void SetupTimeSelectionUI()
    {
        if (studyTimeSlider != null)
        {
            studyTimeSlider.minValue = minStudyTime / 60f;
            studyTimeSlider.maxValue = maxStudyTime / 60f;
            studyTimeSlider.value = sessionDuration / 60f;
            studyTimeSlider.wholeNumbers = true;
            studyTimeSlider.onValueChanged.AddListener(OnStudyTimeChanged);
            if (!useFixedValues)
            {
                OnStudyTimeChanged(studyTimeSlider.value);
            }
        }
        else
        {
        }
        if (breakTimeSlider != null)
        {
            breakTimeSlider.minValue = minBreakTime / 60f;
            breakTimeSlider.maxValue = maxBreakTime / 60f;
            breakTimeSlider.value = breakDuration / 60f;
            breakTimeSlider.wholeNumbers = true;
            breakTimeSlider.onValueChanged.AddListener(OnBreakTimeChanged);
            if (!useFixedValues)
            {
                OnBreakTimeChanged(breakTimeSlider.value);
            }
        }
        else
        {
        }
        UpdateTimeSelectionDisplay();
    }
    public void OnStudyTimeChanged(float minutes)
    {
        if (!useFixedValues)
        {
            sessionDuration = minutes * 60f;
            originalSessionDuration = sessionDuration;
            if (!isRunning)
            {
                currentTime = sessionDuration;
                UpdateTimerDisplay();
                UpdateProgressBar();
            }
        }
        UpdateTimeSelectionDisplay();
    }
    public void OnBreakTimeChanged(float minutes)
    {
        if (!useFixedValues)
        {
            breakDuration = minutes * 60f;
        }
        UpdateTimeSelectionDisplay();
    }
    void UpdateTimeSelectionDisplay()
    {
        if (studyTimeValue != null && studyTimeSlider != null)
        {
            studyTimeValue.text = $"{(int)studyTimeSlider.value} min";
        }
        if (breakTimeValue != null && breakTimeSlider != null)
        {
            breakTimeValue.text = $"{(int)breakTimeSlider.value} min";
        }
        if (studyTimeLabel != null)
        {
            studyTimeLabel.text = "Tiempo De Estudio";
        }
        if (breakTimeLabel != null)
        {
            breakTimeLabel.text = "Tiempo De Descanso";
        }
    }
    [ContextMenu("Toggle Time Selection Panel")]
    public void ToggleTimeSelectionPanel()
    {
        if (timeSelectionPanel != null)
        {
            timeSelectionPanel.SetActive(!timeSelectionPanel.activeInHierarchy);
        }
    }
    [ContextMenu("Show Time Selection Panel")]
    public void ShowTimeSelectionPanel()
    {
        if (timeSelectionPanel != null)
        {
            timeSelectionPanel.SetActive(true);
        }
    }
    [ContextMenu("Hide Time Selection Panel")]
    public void HideTimeSelectionPanel()
    {
        if (timeSelectionPanel != null)
        {
            timeSelectionPanel.SetActive(false);
        }
    }
    [ContextMenu("Style Time Selection Panel")]
    public void StyleTimeSelectionPanel()
    {
        if (timeSelectionPanel != null)
        {
            var panelImage = timeSelectionPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0.95f, 0.95f, 0.95f, 0.9f);
            }
        }
    }
    void Update()
    {
        if (isRunning && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
            UpdateProgressBar();
            if (showPhrasesAutomatically && !isBreakTime && Time.time - lastPhraseTime >= phraseInterval)
            {
                ShowNextAutomaticPhrase();
            }
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
            UpdateProgressBarColor(progress);
        }
    }
    void UpdateProgressBarColor(float progress)
    {
        if (progressBar != null && progressBar.fillRect != null)
        {
            Image fillImage = progressBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                Color targetColor;
                if (progress < 0.5f)
                {
                    targetColor = Color.Lerp(progressStartColor, progressMidColor, progress * 2f);
                }
                else
                {
                    targetColor = Color.Lerp(progressMidColor, progressEndColor, (progress - 0.5f) * 2f);
                }
                fillImage.color = targetColor;
            }
        }
    }
    void UpdateButtonStates()
    {
        playPauseButton.interactable = true;
        resetButton.interactable = currentTime != originalSessionDuration;
        var playPauseText = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();
        if (!isRunning && currentTime == originalSessionDuration)
        {
            if (playPauseText != null) playPauseText.text = "Iniciar";
        }
        else if (isRunning)
        {
            if (playPauseText != null) playPauseText.text = "Pausar";
        }
        else
        {
            if (playPauseText != null) playPauseText.text = "Continuar";
        }

        NotifyCustomButtonController();
    }
    void NotifyCustomButtonController()
    {
        var customController = FindObjectOfType<CustomButtonController>();
        if (customController != null)
        {
            customController.ForceUpdateButtonStates();
        }
    }
    public void OnTextClicked()
    {
        if (isRunning && !isBreakTime)
        {
            NextPhrase();
            string currentPhrase = currentPhraseSet != null && currentPhraseSet.Length > 0 ? currentPhraseSet[currentPhraseIndex] : "";
            ShowSpeechBubble(currentPhrase);
            if (capybaraAnimator != null && capybaraGameObject != null && capybaraGameObject.activeInHierarchy)
            {
                capybaraAnimator.ReactToPhrase(currentPhrase);
            }
            if (tungTungAnimator != null && tungTungGameObject != null && tungTungGameObject.activeInHierarchy)
            {
                tungTungAnimator.ReactToPhrase(currentPhrase);
            }
        }
    }
    void ShowNextAutomaticPhrase()
    {
        if (!isRunning || isBreakTime) return;
        NextPhrase();
        string currentPhrase = currentPhraseSet != null && currentPhraseSet.Length > 0 ? currentPhraseSet[currentPhraseIndex] : "";
        phraseText.text = currentPhrase;
        ShowSpeechBubble(currentPhrase);
        if (capybaraAnimator != null && capybaraGameObject != null && capybaraGameObject.activeInHierarchy)
        {
            capybaraAnimator.ReactToPhrase(currentPhrase);
        }
        if (tungTungAnimator != null && tungTungGameObject != null && tungTungGameObject.activeInHierarchy)
        {
            tungTungAnimator.ReactToPhrase(currentPhrase);
        }
        lastPhraseTime = Time.time;
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
    void PauseResumeSession()
    {
        isRunning = !isRunning;
        if (isRunning)
        {
            SetPhraseSet(activePhrases);
            ShowRandomPhraseFromSet(restartPhrases);
            if (capybaraAnimator != null && capybaraGameObject != null && capybaraGameObject.activeInHierarchy)
            {
                capybaraAnimator.OnStartButtonPressed();
            }
            if (tungTungAnimator != null && tungTungGameObject != null && tungTungGameObject.activeInHierarchy)
            {
                tungTungAnimator.OnStartButtonPressed();
            }
        }
        else
        {
            SetPhraseSet(pausePhrases);
            ShowRandomPhraseFromSet(pausePhrases);
            if (capybaraAnimator != null && capybaraGameObject != null && capybaraGameObject.activeInHierarchy)
            {
                capybaraAnimator.OnStopButtonPressed();
            }
            if (tungTungAnimator != null && tungTungGameObject != null && tungTungGameObject.activeInHierarchy)
            {
                tungTungAnimator.OnStopButtonPressed();
            }
        }
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
        SetPhraseSet(idlePhrases);
        ShowRandomPhraseFromSet(idlePhrases);
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
            if (isCapybaraActive)
                ShowSpeechBubbleFromCharacter("Misión cumplida. Ahora es momento de paz y tranquilidad.", "capybara");
            else
                ShowSpeechBubbleFromCharacter("¡Objetivo logrado! ¡Eres increíble!", "tungtung");
            if (capybaraAnimator != null && capybaraGameObject != null)
            {
                bool wasActive = capybaraGameObject.activeInHierarchy;
                if (!wasActive)
                {
                    capybaraGameObject.SetActive(true);
                }
                capybaraAnimator.OnTimerFinish();
                if (!wasActive)
                {
                    StartCoroutine(RestoreCapybaraActiveState(wasActive, 3f));
                }
            }
            else
            {
            }
            if (tungTungAnimator != null && tungTungGameObject != null && tungTungGameObject.activeInHierarchy)
            {
                tungTungAnimator.CelebrateSessionComplete();
            }
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
            if (isCapybaraActive)
                ShowSpeechBubbleFromCharacter("Comencemos de nuevo con mente clara y serena.", "capybara");
            else
                ShowSpeechBubbleFromCharacter("¡Nueva oportunidad! ¡Vamos a brillar!", "tungtung");

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
        UpdateTimerColor();
        if (isRunning && currentTime <= 60f && !isBreakTime)
        {
            StartTimerPulse();
        }
    }
    void UpdateTimerColor()
    {
        if (timerText != null)
        {
            if (isBreakTime)
            {
                timerText.color = breakTimerColor;
            }
            else if (!isBreakTime && currentTime <= 300f)
            {
                timerText.color = warningTimerColor;
            }
            else
            {
                timerText.color = normalTimerColor;
            }
        }
    }
    void StartTimerPulse()
    {
        if (timerText != null)
        {
            float pulseScale = 1f + Mathf.Sin(Time.time * 5f) * 0.1f;
            timerText.transform.localScale = Vector3.one * pulseScale;
        }
    }
    void NextPhrase()
    {
        if (currentPhraseSet != null && currentPhraseSet.Length > 0)
        {
            currentPhraseIndex = (currentPhraseIndex + 1) % currentPhraseSet.Length;
            phraseText.text = currentPhraseSet[currentPhraseIndex];
        }
    }






    public void ShowSpeechBubble(string message)
    {
        if (phraseText != null)
        {
            phraseText.text = message;
        }
    }
    public void ShowSpeechBubbleFromCharacter(string message, string characterName = null)
    {
        ShowSpeechBubble(message);
    }



    void StartSession()
    {
        isRunning = true;
        SetPhraseSet(activePhrases);
        
        ShowRandomPhraseFromSet(startPhrases);
        
        instructionsText.text = showPhrasesAutomatically ? 
            "Las frases aparecerán automáticamente. Toca para cambiar manualmente." : 
            "Toca el texto para cambiar la frase motivacional";
            
        if (capybaraAnimator != null && capybaraGameObject != null && capybaraGameObject.activeInHierarchy)
        {
            capybaraAnimator.OnStartButtonPressed();
            capybaraAnimator.OnTimerStart();
        }
        if (tungTungAnimator != null && tungTungGameObject != null && tungTungGameObject.activeInHierarchy)
        {
            tungTungAnimator.OnStartButtonPressed();
        }
        lastPhraseTime = Time.time;
        UpdateButtonStates();
    }
    public void OnCharacterChanged(int characterIndex)
    {
        if (characterIndex == 0)
        {
            ShowCapybara();
        }
        else if (characterIndex == 1)
        {
            ShowTungTung();
        }
        else
        {
        }
    }
    public void ShowCapybara()
    {
        if (capybaraGameObject != null)
        {
            capybaraGameObject.SetActive(true);
        }
        if (tungTungGameObject != null)
        {
            tungTungGameObject.SetActive(false);
        }
    }
    public void ShowTungTung()
    {
        if (capybaraGameObject != null)
        {
            capybaraGameObject.SetActive(false);
        }
        else
        {
        }
        if (tungTungGameObject != null)
        {
            tungTungGameObject.SetActive(true);
            if (tungTungAnimator != null)
            {
            }
        }
    }
    [ContextMenu("Test Switch to Capybara")]
    public void TestSwitchToCapybara()
    {
        OnCharacterChanged(0);
    }
    [ContextMenu("Test Switch to TungTung")]
    public void TestSwitchToTungTung()
    {
        OnCharacterChanged(1);
    }
    void SetupCharacterDropdown()
    {
        if (characterDropdown != null)
        {
            characterDropdown.onValueChanged.RemoveAllListeners();
            characterDropdown.onValueChanged.AddListener(OnCharacterChanged);
        }
        else
        {
        }
    }
    [ContextMenu("Auto Find Character GameObjects")]
    public void AutoFindCharacterGameObjects()
    {
        if (capybaraGameObject == null && capybaraAnimator != null)
        {
            capybaraGameObject = capybaraAnimator.gameObject;
        }
        if (tungTungGameObject == null && tungTungAnimator != null)
        {
            tungTungGameObject = tungTungAnimator.gameObject;
        }
        if (startWithCapybara)
        {
            ShowCapybara();
        }
        else
        {
            ShowTungTung();
        }
    }
    [ContextMenu("Auto Find Character Animators")]
    public void AutoFindCharacterAnimators()
    {
        if (tungTungAnimator == null)
        {
            SimpleTungTungAnimator foundTungTung = FindObjectOfType<SimpleTungTungAnimator>();
            if (foundTungTung != null)
            {
                tungTungAnimator = foundTungTung;
            }
        }
        if (capybaraAnimator == null)
        {
            SimpleCapybaraAnimator foundCapybara = FindObjectOfType<SimpleCapybaraAnimator>();
            if (foundCapybara != null)
            {
                capybaraAnimator = foundCapybara;
            }
        }
        if (tungTungAnimator != null && tungTungGameObject == null)
        {
            tungTungGameObject = tungTungAnimator.gameObject;
        }
        if (capybaraAnimator != null && capybaraGameObject == null)
        {
            capybaraGameObject = capybaraAnimator.gameObject;
        }
    }
    void InitializeCharacters()
    {
        if (tungTungAnimator == null || capybaraAnimator == null || 
            tungTungGameObject == null || capybaraGameObject == null)
        {
            AutoFindCharacterAnimators();
        }
        if (tungTungGameObject != null && tungTungAnimator != null)
        {
        }
        ApplyCharacterScaling();
        if (startWithCapybara)
        {
            ShowCapybara();
        }
        else
        {
            ShowTungTung();
        }
    }
    private void ApplyCharacterScaling()
    {
        if (characterScaleMultiplier != 1f)
        {
            if (capybaraAnimator != null)
            {
                Vector3 originalScale = capybaraAnimator.transform.localScale;
                capybaraAnimator.transform.localScale = originalScale * characterScaleMultiplier;
            }
            if (tungTungAnimator != null)
            {
                Vector3 originalScale = tungTungAnimator.transform.localScale;
                tungTungAnimator.transform.localScale = originalScale * characterScaleMultiplier;
            }
        }
    }
    public void ToggleSessionTimer()
    {
        if (isRunning)
        {
            PauseSession();
        }
        else
        {
            ResumeSession();
        }
    }
    void PauseSession()
    {
        isRunning = false;
        UpdateButtonStates();
    }
    void ResumeSession()
    {
        isRunning = true;
        UpdateButtonStates();
    }
    IEnumerator RestoreCapybaraActiveState(bool originalActiveState, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (capybaraGameObject != null)
        {
            capybaraGameObject.SetActive(originalActiveState);
        }
    }
}

