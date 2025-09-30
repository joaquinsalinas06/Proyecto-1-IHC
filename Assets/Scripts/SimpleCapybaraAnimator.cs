using UnityEngine;
using System.Collections;
public class SimpleCapybaraAnimator : MonoBehaviour
{
    [Header("Simple Animation Settings")]
    public float breathingSpeed = 1.5f;
    public float breathingAmount = 0.02f;
    public float jumpHeight = 0.3f;
    public float jumpDuration = 0.5f;
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip spinSound;
    public AudioClip selectSound;
    
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isBreathing = true;
    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
        
        // Get or create AudioSource component
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        StartCoroutine(SimpleBreathingAnimation());
    }
    IEnumerator SimpleBreathingAnimation()
    {
        while (isBreathing)
        {
            float breathingPhase = Mathf.Sin(Time.time * breathingSpeed) * breathingAmount;
            float breathingScale = 1f + Mathf.Abs(breathingPhase);
            transform.localScale = originalScale * breathingScale;
            yield return null;
        }
    }
    public void OnTimerStart()
    {
        StartCoroutine(QuickJump());
    }
    public void OnTimerFinish()
    {
        StartCoroutine(CelebrationJumpSpin());
    }
    IEnumerator QuickJump()
    {
        Vector3 startPos = transform.position;
        Vector3 jumpPos = startPos + Vector3.up * jumpHeight;
        float elapsed = 0f;
        while (elapsed < jumpDuration / 2)
        {
            float progress = elapsed / (jumpDuration / 2);
            transform.position = Vector3.Lerp(startPos, jumpPos, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < jumpDuration / 2)
        {
            float progress = elapsed / (jumpDuration / 2);
            transform.position = Vector3.Lerp(jumpPos, startPos, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }
    IEnumerator CelebrationJumpSpin()
    {
        Vector3 startPos = transform.position;
        Vector3 jumpPos = startPos + Vector3.up * (jumpHeight * 2f);
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;
        float totalDuration = jumpDuration * 2f;
        float totalRotation = 720f;
        while (elapsed < totalDuration)
        {
            float progress = elapsed / totalDuration;
            float jumpCurve = Mathf.Sin(progress * Mathf.PI);
            Vector3 currentPos = Vector3.Lerp(startPos, jumpPos, jumpCurve);
            transform.position = currentPos;
            float currentRotation = progress * totalRotation;
            transform.rotation = startRot * Quaternion.Euler(0, currentRotation, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
    public void ResetToOriginalPose()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
    }
    public void StopAnimations()
    {
        isBreathing = false;
        StopAllCoroutines();
        ResetToOriginalPose();
    }
    public void ReactToPhrase(string phrase) 
    { 
        StartCoroutine(SimpleBreath());
    }
    public void CelebrateSessionComplete() 
    { 
        StartCoroutine(CelebrationJumpSpin());
    }
    public void OnStartButtonPressed() 
    { 
        StartCoroutine(QuickJump());
    }
    public void OnStopButtonPressed() 
    { 
        StartCoroutine(CelebrationJumpSpin());
    }
    IEnumerator SimpleBreath()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float breathScale = 1f + Mathf.Sin(progress * Mathf.PI) * breathingAmount;
            transform.localScale = originalScale * breathScale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

}

