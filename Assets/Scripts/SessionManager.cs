using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
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

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    public AudioClip sessionCompleteMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;
    private AudioSource musicAudioSource;

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
    public float sessionDuration = 60f;    // Valor inicial: 1 minuto
    public float breakDuration = 30f;      // Valor inicial: 30 segundos (0.5 min)
    [Header("Time Selection Ranges")]
    public float minStudyTime = 30f;      // 30 segundos (0.5 min)
    public float maxStudyTime = 1800f;    // 30 minutos
    public float minBreakTime = 6f;       // 6 segundos (0.1 min)
    public float maxBreakTime = 360f;     // 6 minutos
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
    private string initialMessage = "Preparate para tu sesion de productividad";
    private string instructionsMessage = "Toca personaje o texto para nuevo mensaje!";

    [Header("Motivational Phrases")]
    private string[] idlePhrases = {
        "Preparate para ser productivo",
        "Tu momento de enfoque esta a punto de comenzar",
        "Respira hondo y preparate para concentrarte",
        "Es hora de alcanzar tus metas",
        "Cada sesion te acerca a tu objetivo",
        "La productividad comienza con tu decision",
        "Estas a un paso de tu mejor version",
        "El enfoque es tu superpoder secreto",
        "Hoy es el dia perfecto para ser productivo",
        "Tu mente esta lista para el desafio"
    };
    
    private string[] activePhrases = {
        "Concentrate al maximo!",
        "Cada minuto cuenta!",
        "Manten la calma y sigue!",
        "Mente clara, corazon firme!",
        "Momento productivo en accion!",
        "Respira profundo y continua!",
        "Estas en tu elemento!",
        "El flujo esta contigo!",
        "Cada segundo es una victoria!",
        "Tu enfoque es imparable!",
        "Construyendo el exito paso a paso!",
        "La disciplina te esta transformando!",
        "Eres mas fuerte que cualquier distraccion!",
        "El progreso se siente increible!",
        "Tu determinacion es inspiradora!"
    };
    
    private string[] startPhrases = {
        "Comienza tu jornada de productividad!",
        "Es hora de brillar!",
        "Vamos a hacer que este tiempo cuente!",
        "Tu sesion de enfoque ha comenzado!",
        "A conquistar tus objetivos!",
        "El momento perfecto es ahora!",
        "Desata todo tu potencial!",
        "La aventura productiva comienza!",
        "Tiempo de hacer magia!",
        "Tu zona de enfoque te espera!"
    };
    
    private string[] pausePhrases = {
        "Respira y recargas energias",
        "Un descanso merecido para tu mente",
        "Pausa estrategica para volver mas fuerte",
        "Relajate, lo estas haciendo genial",
        "Momento de calma y reflexion",
        "Descansa para volver con mas energia",
        "Tu mente necesita este respiro",
        "Pausa sabia, regreso poderoso",
        "Recarga completa en proceso",
        "El descanso tambien es productividad"
    };
    
    private string[] restartPhrases = {
        "De vuelta a la accion!",
        "Recargado y listo para mas!",
        "Segunda ronda, misma determinacion!",
        "Continuamos donde lo dejamos!",
        "Renovado y enfocado!",
        "El momentum regresa contigo!",
        "Vuelta al flujo productivo!",
        "Energia restaurada, objetivos claros!",
        "Listos para la siguiente fase!",
        "El enfoque nunca se fue, solo descanso!"
    };
    
    private int currentPhraseIndex = 0;
    private string[] currentPhraseSet;
    void Start()
    {
        Debug.Log("SessionManager Start() beginning...");
        
        // Check for null references before using them
        if (phraseText != null)
        {
            phraseText.text = initialMessage;
            Debug.Log("PhraseText initialized");
        }
        else
        {
            Debug.LogError("PhraseText is null! Please assign it in the Inspector.");
        }

        if (instructionsText != null)
        {
            instructionsText.text = "Presiona INICIAR cuando estes listo";
            Debug.Log("InstructionsText initialized");
        }
        else
        {
            Debug.LogError("InstructionsText is null! Please assign it in the Inspector.");
        }
        
        try
        {
            SetupTimeSelectionUI();
            Debug.Log("TimeSelectionUI setup complete");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetupTimeSelectionUI: {e.Message}");
        }

        // Aplicar estilo después de un frame para asegurar que todo esté inicializado
        StartCoroutine(ApplyStyleAfterDelay());
        
        originalSessionDuration = sessionDuration;
        currentTime = sessionDuration;
        
        try
        {
            UpdateTimerDisplay();
            Debug.Log("Timer display updated");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateTimerDisplay: {e.Message}");
        }
        
        try
        {
            UpdateProgressBar();
            Debug.Log("Progress bar updated");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateProgressBar: {e.Message}");
        }
        try
        {
            InitializeCharacters();
            Debug.Log("Characters initialized");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in InitializeCharacters: {e.Message}");
        }
        
        try
        {
            SetupCharacterDropdown();
            Debug.Log("Character dropdown setup complete");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetupCharacterDropdown: {e.Message}");
        }
        
        try
        {
            SetPhraseSet(idlePhrases);
            Debug.Log("Phrase set configured");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetPhraseSet: {e.Message}");
        }
        
        try
        {
            SetupBackgroundMusic();
            Debug.Log("Background music setup complete");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetupBackgroundMusic: {e.Message}");
        }
        
        try
        {
            UpdateButtonStates();
            Debug.Log("Button states updated");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateButtonStates: {e.Message}");
        }
        
        // Setup text button for phrase clicking
        try
        {
            if (phraseText != null)
            {
                Button textButton = phraseText.GetComponent<Button>();
                if (textButton == null)
                {
                    textButton = phraseText.gameObject.AddComponent<Button>();
                }
                textButton.onClick.AddListener(OnTextClicked);
                Debug.Log("Text button setup complete");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting up text button: {e.Message}");
        }
        
        Debug.Log("SessionManager Start() completed successfully!");
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
            // Configurar slider de estudio: 30 segundos a 30 minutos (en segundos)
            studyTimeSlider.minValue = minStudyTime;        // 30 segundos
            studyTimeSlider.maxValue = maxStudyTime;        // 1800 segundos (30 min)

            // Asegurar que el valor inicial esté dentro del rango
            float initialStudyValue = Mathf.Clamp(sessionDuration, minStudyTime, maxStudyTime);
            studyTimeSlider.value = initialStudyValue;

            studyTimeSlider.wholeNumbers = true;
            studyTimeSlider.onValueChanged.AddListener(OnStudyTimeChanged);
            if (!useFixedValues)
            {
                OnStudyTimeChanged(studyTimeSlider.value);
            }

            Debug.Log($"Study slider configurado: min={minStudyTime}, max={maxStudyTime}, value={studyTimeSlider.value}");

            // Forzar actualización inmediata del slider
            studyTimeSlider.minValue = minStudyTime;
            studyTimeSlider.maxValue = maxStudyTime;
        }
        else
        {
        }
        if (breakTimeSlider != null)
        {
            // Configurar slider de descanso: 10 segundos a 10 minutos (en segundos)
            breakTimeSlider.minValue = minBreakTime;        // 10 segundos
            breakTimeSlider.maxValue = maxBreakTime;        // 600 segundos (10 min)

            // Asegurar que el valor inicial esté dentro del rango
            float initialBreakValue = Mathf.Clamp(breakDuration, minBreakTime, maxBreakTime);
            breakTimeSlider.value = initialBreakValue;

            breakTimeSlider.wholeNumbers = true;
            breakTimeSlider.onValueChanged.AddListener(OnBreakTimeChanged);
            if (!useFixedValues)
            {
                OnBreakTimeChanged(breakTimeSlider.value);
            }

            Debug.Log($"Break slider configurado: min={minBreakTime}, max={maxBreakTime}, value={breakTimeSlider.value}");

            // Forzar actualización inmediata del slider
            breakTimeSlider.minValue = minBreakTime;
            breakTimeSlider.maxValue = maxBreakTime;
        }
        else
        {
        }
        UpdateTimeSelectionDisplay();
    }
    public void OnStudyTimeChanged(float seconds)
    {
        Debug.Log($"OnStudyTimeChanged llamado con {seconds} segundos");
        if (!useFixedValues)
        {
            sessionDuration = seconds;
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

    public void OnBreakTimeChanged(float seconds)
    {
        Debug.Log($"OnBreakTimeChanged llamado con {seconds} segundos");
        if (!useFixedValues)
        {
            breakDuration = seconds;
        }
        UpdateTimeSelectionDisplay();
    }
    void UpdateTimeSelectionDisplay()
    {
        if (studyTimeValue != null && studyTimeSlider != null)
        {
            float timeInSeconds = studyTimeSlider.value; // El slider ya está en segundos
            if (timeInSeconds < 60f)
            {
                studyTimeValue.text = $"{(int)timeInSeconds} seg";
            }
            else
            {
                float minutes = timeInSeconds / 60f;
                // Para múltiplos de 30 segundos, mostrar con decimales limpios
                if (timeInSeconds % 30 == 0)
                {
                    studyTimeValue.text = $"{minutes:0.#} min";
                }
                else
                {
                    studyTimeValue.text = $"{(int)minutes} min";
                }
            }
        }
        if (breakTimeValue != null && breakTimeSlider != null)
        {
            float timeInSeconds = breakTimeSlider.value; // El slider ya está en segundos
            if (timeInSeconds < 60f)
            {
                breakTimeValue.text = $"{(int)timeInSeconds} seg";
            }
            else
            {
                float minutes = timeInSeconds / 60f;
                // Para múltiplos de 6 segundos, mostrar con decimales limpios
                if (timeInSeconds % 6 == 0)
                {
                    breakTimeValue.text = $"{minutes:0.#} min";
                }
                else
                {
                    breakTimeValue.text = $"{(int)minutes} min";
                }
            }
        }
        if (studyTimeLabel != null)
        {
            studyTimeLabel.text = "TIEMPO DE\nESTUDIO";
        }
        if (breakTimeLabel != null)
        {
            breakTimeLabel.text = "TIEMPO DE\nDESCANSO";
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
                // Cambiar a gris oscuro
                panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            }

            // Mejorar el layout y espaciado
            ImproveTimeSelectionLayout();

            // Forzar centrado de elementos
            CenterTimeSelectionElements();
        }
    }

    void ImproveTimeSelectionLayout()
    {
        if (timeSelectionPanel == null)
        {
            Debug.LogError("timeSelectionPanel is NULL!");
            return;
        }

        Debug.Log("=== MEJORANDO LAYOUT DEL PANEL DE TIEMPO ===");

        // Buscar elementos automáticamente dentro del panel
        Debug.Log("Buscando elementos de UI en el panel...");

        if (studyTimeLabel == null)
        {
            studyTimeLabel = timeSelectionPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>()
                .FirstOrDefault(x => x.name.Contains("StudyTimeLabel") || x.name.Contains("Label"));
            Debug.Log($"Auto-found studyTimeLabel: {studyTimeLabel?.name ?? "NULL"}");
        }
        if (breakTimeLabel == null)
        {
            breakTimeLabel = timeSelectionPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>()
                .FirstOrDefault(x => x.name.Contains("BreakTimeLabel") || (x.name.Contains("Label") && !x.name.Contains("Study")));
            Debug.Log($"Auto-found breakTimeLabel: {breakTimeLabel?.name ?? "NULL"}");
        }
        if (studyTimeValue == null)
        {
            studyTimeValue = timeSelectionPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>()
                .FirstOrDefault(x => x.name.Contains("StudyTimeValue") || (x.name.Contains("Value") && x.name.Contains("Study")));
            Debug.Log($"Auto-found studyTimeValue: {studyTimeValue?.name ?? "NULL"}");
        }
        if (breakTimeValue == null)
        {
            breakTimeValue = timeSelectionPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>()
                .FirstOrDefault(x => x.name.Contains("BreakTimeValue") || (x.name.Contains("Value") && x.name.Contains("Break")));
            Debug.Log($"Auto-found breakTimeValue: {breakTimeValue?.name ?? "NULL"}");
        }
        if (studyTimeSlider == null)
        {
            studyTimeSlider = timeSelectionPanel.GetComponentsInChildren<Slider>()
                .FirstOrDefault(x => x.name.Contains("StudyTimeSlider") || x.name.Contains("Study"));
            Debug.Log($"Auto-found studyTimeSlider: {studyTimeSlider?.name ?? "NULL"}");
        }
        if (breakTimeSlider == null)
        {
            breakTimeSlider = timeSelectionPanel.GetComponentsInChildren<Slider>()
                .FirstOrDefault(x => x.name.Contains("BreakTimeSlider") || (x.name.Contains("Slider") && !x.name.Contains("Study")));
            Debug.Log($"Auto-found breakTimeSlider: {breakTimeSlider?.name ?? "NULL"}");
        }

        // Listar todos los elementos TextMeshPro y Sliders para debug
        Debug.Log("=== LISTANDO TODOS LOS ELEMENTOS ===");
        var allTexts = timeSelectionPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            Debug.Log($"TextMeshPro encontrado: {text.name} - Texto: '{text.text}'");
        }
        var allSliders = timeSelectionPanel.GetComponentsInChildren<Slider>();
        foreach (var slider in allSliders)
        {
            Debug.Log($"Slider encontrado: {slider.name}");
        }

        // DESHABILITAR COMPONENTES QUE SOBRESCRIBEN TAMAÑOS
        Debug.Log("=== DESHABILITANDO LAYOUT COMPONENTS ===");

        // Deshabilitar Content Size Fitters que podrían estar controlando el tamaño
        var contentSizeFitters = timeSelectionPanel.GetComponentsInChildren<UnityEngine.UI.ContentSizeFitter>();
        foreach (var fitter in contentSizeFitters)
        {
            Debug.Log($"Deshabilitando ContentSizeFitter en: {fitter.gameObject.name}");
            fitter.enabled = false;
        }

        // Deshabilitar temporalmente Layout Groups que controlan tamaños
        var layoutGroups = timeSelectionPanel.GetComponentsInChildren<UnityEngine.UI.LayoutGroup>();
        foreach (var layoutGroup in layoutGroups)
        {
            Debug.Log($"Configurando LayoutGroup en: {layoutGroup.gameObject.name}");
            if (layoutGroup is UnityEngine.UI.HorizontalLayoutGroup hLayout)
            {
                hLayout.childControlWidth = false;
                hLayout.childControlHeight = false;
            }
            else if (layoutGroup is UnityEngine.UI.VerticalLayoutGroup vLayout)
            {
                vLayout.childControlWidth = false;
                vLayout.childControlHeight = false;
            }
        }

        // Mejorar los textos de los labels - aumentar tamaño
        if (studyTimeLabel != null)
        {
            Debug.Log($"Configurando studyTimeLabel: {studyTimeLabel.name}");

            // Forzar configuración múltiple
            studyTimeLabel.fontSize = 50f;  // Extra grande
            studyTimeLabel.color = Color.white;
            studyTimeLabel.alignment = TMPro.TextAlignmentOptions.Center;
            studyTimeLabel.enableWordWrapping = true;
            studyTimeLabel.text = "TIEMPO DE\nESTUDIO";

            // Forzar actualización inmediata
            studyTimeLabel.ForceMeshUpdate();

            Debug.Log($"StudyTimeLabel configurado - FontSize: {studyTimeLabel.fontSize}");
        }
        else
        {
            Debug.LogError("studyTimeLabel is NULL even after auto-search!");
        }

        if (breakTimeLabel != null)
        {
            Debug.Log($"Configurando breakTimeLabel: {breakTimeLabel.name}");

            breakTimeLabel.fontSize = 50f;  // Extra grande
            breakTimeLabel.color = Color.white;
            breakTimeLabel.alignment = TMPro.TextAlignmentOptions.Center;
            breakTimeLabel.enableWordWrapping = true;
            breakTimeLabel.text = "TIEMPO DE\nDESCANSO";

            // Forzar actualización inmediata
            breakTimeLabel.ForceMeshUpdate();

            Debug.Log($"BreakTimeLabel configurado - FontSize: {breakTimeLabel.fontSize}");
        }
        else
        {
            Debug.LogError("breakTimeLabel is NULL even after auto-search!");
        }

        // Mejorar los textos de los valores - aumentar tamaño
        if (studyTimeValue != null)
        {
            Debug.Log($"Configurando studyTimeValue: {studyTimeValue.name}");

            studyTimeValue.fontSize = 40f;  // Extra súper grande
            studyTimeValue.color = new Color(1f, 0.9f, 0.2f, 1f); // Amarillo como acento
            studyTimeValue.alignment = TMPro.TextAlignmentOptions.Center;

            // Forzar actualización inmediata
            studyTimeValue.ForceMeshUpdate();

            Debug.Log($"StudyTimeValue configurado - FontSize: {studyTimeValue.fontSize}");
        }
        else
        {
            Debug.LogError("studyTimeValue is NULL even after auto-search!");
        }

        if (breakTimeValue != null)
        {
            Debug.Log($"Configurando breakTimeValue: {breakTimeValue.name}");

            breakTimeValue.fontSize = 40f;  // Extra súper grande
            breakTimeValue.color = new Color(1f, 0.9f, 0.2f, 1f); // Amarillo como acento
            breakTimeValue.alignment = TMPro.TextAlignmentOptions.Center;

            // Forzar actualización inmediata
            breakTimeValue.ForceMeshUpdate();

            Debug.Log($"BreakTimeValue configurado - FontSize: {breakTimeValue.fontSize}");
        }
        else
        {
            Debug.LogError("breakTimeValue is NULL even after auto-search!");
        }

        // Mejorar los sliders
        if (studyTimeSlider != null)
        {
            var sliderImage = studyTimeSlider.GetComponent<Image>();
            if (sliderImage != null)
            {
                sliderImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Fondo del slider
            }

            // Mejorar el handle del slider
            if (studyTimeSlider.handleRect != null)
            {
                var handleImage = studyTimeSlider.handleRect.GetComponent<Image>();
                if (handleImage != null)
                {
                    handleImage.color = new Color(1f, 0.9f, 0.2f, 1f); // Amarillo
                }
            }

            // Mejorar el fill del slider
            if (studyTimeSlider.fillRect != null)
            {
                var fillImage = studyTimeSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = new Color(0.8f, 0.7f, 0.2f, 1f); // Amarillo más oscuro
                }
            }
        }

        if (breakTimeSlider != null)
        {
            var sliderImage = breakTimeSlider.GetComponent<Image>();
            if (sliderImage != null)
            {
                sliderImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Fondo del slider
            }

            // Mejorar el handle del slider
            if (breakTimeSlider.handleRect != null)
            {
                var handleImage = breakTimeSlider.handleRect.GetComponent<Image>();
                if (handleImage != null)
                {
                    handleImage.color = new Color(1f, 0.9f, 0.2f, 1f); // Amarillo
                }
            }

            // Mejorar el fill del slider
            if (breakTimeSlider.fillRect != null)
            {
                var fillImage = breakTimeSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = new Color(0.8f, 0.7f, 0.2f, 1f); // Amarillo más oscuro
                }
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
    
    public void OnCharacterClicked()
    {
        Debug.Log($"OnCharacterClicked called! isRunning: {isRunning}, isBreakTime: {isBreakTime}");
        // Same functionality as OnTextClicked - change phrase when character is touched
        if (isRunning && !isBreakTime)
        {
            Debug.Log("Changing phrase from character click...");
            NextPhrase();
            string currentPhrase = currentPhraseSet != null && currentPhraseSet.Length > 0 ? currentPhraseSet[currentPhraseIndex] : "";
            phraseText.text = currentPhrase;
            Debug.Log($"New phrase: {currentPhrase}");
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
        else
        {
            Debug.Log($"Character click ignored - Session not active or in break time");
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
            instructionsText.text = instructionsMessage;
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
            instructionsText.text = "Sesion en pausa - Respira y relajate";
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
        buttonText.text = "Confirmar?";
        instructionsText.text = "Presiona REINICIAR de nuevo para confirmar";
    }
    void CancelResetConfirmation()
    {
        resetConfirmationPending = false;
        var buttonText = resetButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "Reiniciar";
        if (isRunning)
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
        instructionsText.text = "Presiona INICIAR cuando estes listo";
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
            phraseText.text = "Sesion completada! Tiempo de descanso";
            instructionsText.text = "Disfruta tu descanso merecido";
            if (isCapybaraActive)
                ShowSpeechBubbleFromCharacter("Mision cumplida. Ahora es momento de paz y tranquilidad.", "capybara");
            else
                ShowSpeechBubbleFromCharacter("Objetivo logrado! Eres increible!", "tungtung");
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
            phraseText.text = "Descanso terminado! Nueva sesion";
            instructionsText.text = "Presiona INICIAR cuando estes listo";
            if (isCapybaraActive)
                ShowSpeechBubbleFromCharacter("Comencemos de nuevo con mente clara y serena.", "capybara");
            else
                ShowSpeechBubbleFromCharacter("Nueva oportunidad! Vamos a brillar!", "tungtung");

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
        instructionsText.text = instructionsMessage;

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
        Debug.Log("ShowCapybara() called");
        if (capybaraGameObject != null)
        {
            capybaraGameObject.SetActive(true);
            Debug.Log($"Capybara activated: {capybaraGameObject.name}");
        }
        else
        {
            Debug.LogWarning("CapybaraGameObject is null!");
        }
        
        if (tungTungGameObject != null)
        {
            tungTungGameObject.SetActive(false);
            Debug.Log($"TungTung deactivated: {tungTungGameObject.name}");
        }
        else
        {
            Debug.LogWarning("TungTungGameObject is null!");
        }
    }
    public void ShowTungTung()
    {
        Debug.Log("ShowTungTung() called");
        if (capybaraGameObject != null)
        {
            capybaraGameObject.SetActive(false);
            Debug.Log($"Capybara deactivated: {capybaraGameObject.name}");
        }
        else
        {
            Debug.LogWarning("CapybaraGameObject is null!");
        }
        
        if (tungTungGameObject != null)
        {
            tungTungGameObject.SetActive(true);
            Debug.Log($"TungTung activated: {tungTungGameObject.name}");
            if (tungTungAnimator != null)
            {
            }
        }
    }
    
    [ContextMenu("Fix Character Display")]
    public void FixCharacterDisplay()
    {
        Debug.Log("=== Fixing Character Display ===");
        Debug.Log($"startWithCapybara: {startWithCapybara}");
        Debug.Log($"isCapybaraActive: {isCapybaraActive}");
        
        if (capybaraGameObject != null)
        {
            Debug.Log($"Capybara GameObject: {capybaraGameObject.name}, Active: {capybaraGameObject.activeInHierarchy}");
        }
        
        if (tungTungGameObject != null)
        {
            Debug.Log($"TungTung GameObject: {tungTungGameObject.name}, Active: {tungTungGameObject.activeInHierarchy}");
        }
        
        // Force the correct character to show
        if (startWithCapybara)
        {
            ShowCapybara();
        }
        else
        {
            ShowTungTung();
        }
        
        Debug.Log("=== Character Display Fixed ===");
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
            
            // Fix dropdown text sizing for mobile
            FixDropdownTextSizing();
        }
        else
        {
        }
    }

    void FixDropdownTextSizing()
    {
        if (characterDropdown == null) return;

        // Fix the label text (currently selected option)
        if (characterDropdown.captionText != null)
        {
            var labelText = characterDropdown.captionText;
            labelText.enableAutoSizing = false;  // Desactivar auto sizing
            labelText.fontSize = 12f;  // Tamaño más pequeño
            labelText.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            labelText.enableWordWrapping = false;
        }

        // Fix the template text (dropdown items)
        if (characterDropdown.template != null)
        {
            var itemText = characterDropdown.template.GetComponentInChildren<TextMeshProUGUI>();
            if (itemText != null)
            {
                itemText.enableAutoSizing = false;  // Desactivar auto sizing
                itemText.fontSize = 12f;  // Tamaño fijo pequeño para items
                itemText.overflowMode = TMPro.TextOverflowModes.Ellipsis;
                itemText.enableWordWrapping = false;
            }
        }

        // Mejorar el diseño visual del dropdown
        StyleDropdown();
    }

    void StyleDropdown()
    {
        if (characterDropdown == null) return;

        // Cambiar colores del dropdown principal
        var dropdownImage = characterDropdown.GetComponent<UnityEngine.UI.Image>();
        if (dropdownImage != null)
        {
            // Color amarillo como los botones
            dropdownImage.color = new Color(1f, 0.9f, 0.2f, 1f); // Amarillo
        }

        // Estilizar el template (lista desplegable)
        if (characterDropdown.template != null)
        {
            var templateImage = characterDropdown.template.GetComponent<UnityEngine.UI.Image>();
            if (templateImage != null)
            {
                // Fondo gris oscuro con bordes redondeados
                templateImage.color = new Color(0.15f, 0.15f, 0.15f, 0.98f);
            }

            // Estilizar los items de la lista
            var scrollRect = characterDropdown.template.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null)
            {
                var viewport = scrollRect.viewport;
                if (viewport != null)
                {
                    var content = viewport.GetComponentInChildren<UnityEngine.UI.VerticalLayoutGroup>();
                    if (content != null)
                    {
                        var toggle = content.GetComponentInChildren<UnityEngine.UI.Toggle>();
                        if (toggle != null)
                        {
                            var toggleImage = toggle.GetComponent<UnityEngine.UI.Image>();
                            if (toggleImage != null)
                            {
                                // Color de fondo de los items - gris medio
                                toggleImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);
                            }

                            // Configurar colores de selección
                            var colors = toggle.colors;
                            colors.normalColor = new Color(0.25f, 0.25f, 0.25f, 1f);      // Gris normal
                            colors.highlightedColor = new Color(0.8f, 0.7f, 0.2f, 1f);   // Amarillo claro al hover
                            colors.pressedColor = new Color(1f, 0.9f, 0.2f, 1f);         // Amarillo fuerte al presionar
                            colors.selectedColor = new Color(0.9f, 0.8f, 0.3f, 1f);      // Amarillo medio al seleccionar
                            toggle.colors = colors;

                            // Mejorar el texto de los items
                            var itemText = toggle.GetComponentInChildren<TextMeshProUGUI>();
                            if (itemText != null)
                            {
                                itemText.color = Color.white;  // Texto blanco
                                itemText.fontSize = 10f;       // Tamaño pequeño para los items
                            }
                        }
                    }
                }
            }
        }
    }

    void CenterTimeSelectionElements()
    {
        if (timeSelectionPanel == null) return;

        // Buscar y configurar el Layout Group principal
        var mainLayoutGroup = timeSelectionPanel.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (mainLayoutGroup != null)
        {
            mainLayoutGroup.childControlWidth = true;
            mainLayoutGroup.childControlHeight = false;
            mainLayoutGroup.childForceExpandWidth = true;
            mainLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            mainLayoutGroup.spacing = 20f; // Espaciado entre columnas
        }

        // Configurar cada columna para que esté centrada
        var columns = timeSelectionPanel.GetComponentsInChildren<UnityEngine.UI.VerticalLayoutGroup>();
        foreach (var column in columns)
        {
            column.childControlWidth = true;
            column.childControlHeight = false;
            column.childForceExpandWidth = true;
            column.childAlignment = TextAnchor.MiddleCenter;
            column.spacing = 10f; // Espaciado vertical dentro de cada columna

            // Asegurar padding uniforme
            column.padding = new UnityEngine.RectOffset(10, 10, 10, 10);
        }

        // Forzar recalculo del layout
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(timeSelectionPanel.GetComponent<RectTransform>());
    }

    IEnumerator ApplyStyleAfterDelay()
    {
        // Esperar 3 frames para asegurar que todo esté inicializado
        yield return null;
        yield return null;
        yield return null;

        Debug.Log("=== APLICANDO ESTILO CON DELAY ===");
        StyleTimeSelectionPanel();

        // Forzar configuración de sliders después del delay
        ForceSliderConfiguration();

        // Forzar actualización del display
        UpdateTimeSelectionDisplay();

        Debug.Log("=== ESTILO APLICADO COMPLETAMENTE ===");
    }

    void ForceSliderConfiguration()
    {
        Debug.Log("=== FORZANDO CONFIGURACION DE SLIDERS ===");

        if (studyTimeSlider != null)
        {
            Debug.Log($"Antes - Study slider: min={studyTimeSlider.minValue}, max={studyTimeSlider.maxValue}, value={studyTimeSlider.value}");

            studyTimeSlider.minValue = minStudyTime;  // 30 segundos
            studyTimeSlider.maxValue = maxStudyTime;  // 1800 segundos
            studyTimeSlider.value = Mathf.Max(minStudyTime, studyTimeSlider.value); // Asegurar que esté por encima del mínimo

            Debug.Log($"Después - Study slider: min={studyTimeSlider.minValue}, max={studyTimeSlider.maxValue}, value={studyTimeSlider.value}");
        }

        if (breakTimeSlider != null)
        {
            Debug.Log($"Antes - Break slider: min={breakTimeSlider.minValue}, max={breakTimeSlider.maxValue}, value={breakTimeSlider.value}");

            breakTimeSlider.minValue = minBreakTime;  // 6 segundos
            breakTimeSlider.maxValue = maxBreakTime;  // 360 segundos
            breakTimeSlider.value = Mathf.Max(minBreakTime, breakTimeSlider.value); // Asegurar que esté por encima del mínimo

            Debug.Log($"Después - Break slider: min={breakTimeSlider.minValue}, max={breakTimeSlider.maxValue}, value={breakTimeSlider.value}");
        }
    }

    void SetupBackgroundMusic()
    {
        if (backgroundMusic != null && BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.PlayMusic(backgroundMusic);
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
        
        // Force proper character selection - ensure only one is active
        Debug.Log($"Initializing characters - startWithCapybara: {startWithCapybara}");
        if (startWithCapybara)
        {
            Debug.Log("Showing Capybara, hiding TungTung");
            ShowCapybara();
        }
        else
        {
            Debug.Log("Showing TungTung, hiding Capybara");
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

