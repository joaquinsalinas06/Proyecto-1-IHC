using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Background Music Settings")]
    [Tooltip("The background music audio clip")]
    public AudioClip backgroundMusic;
    
    [Range(0f, 1f)]
    [Tooltip("Volume of the background music")]
    public float musicVolume = 0.5f;
    
    [Tooltip("Should music play on start")]
    public bool playOnStart = true;
    
    [Tooltip("Fade in duration when music starts")]
    public float fadeInDuration = 2f;
    
    private AudioSource audioSource;
    private static BackgroundMusicManager instance;
    
    // Public singleton accessor
    public static BackgroundMusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BackgroundMusicManager>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        // Singleton pattern - keep music playing across scene loads
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (playOnStart && backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
    }
    
    void SetupAudioSource()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source for background music
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f; // Start at 0 for fade in
    }
    
    public void PlayBackgroundMusic()
    {
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.Play();
            StartCoroutine(FadeIn());
        }
    }
    
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip != null)
        {
            backgroundMusic = musicClip;
            SetupAudioSource();
            PlayBackgroundMusic();
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (audioSource != null)
        {
            StartCoroutine(FadeOut());
        }
    }
    
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
        }
    }
    
    System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            audioSource.volume = Mathf.Lerp(0f, musicVolume, progress);
            yield return null;
        }
        
        audioSource.volume = musicVolume;
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }
        
        audioSource.Stop();
        audioSource.volume = 0f;
    }
    
    // Public methods for external control
    public void PauseMusic()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }
    
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
    
    // Context menu for testing
    [ContextMenu("Play Music")]
    void TestPlay()
    {
        PlayBackgroundMusic();
    }
    
    [ContextMenu("Stop Music")]
    void TestStop()
    {
        StopBackgroundMusic();
    }
}