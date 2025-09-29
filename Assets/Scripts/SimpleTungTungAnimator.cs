using UnityEngine;
using System.Collections;

public class SimpleTungTungAnimator : MonoBehaviour
{
    [Header("Simple Animation Settings")]
    public float breathingSpeed = 1.5f;
    public float breathingAmount = 0.02f;
    public float jumpHeight = 0.3f;
    public float jumpDuration = 0.5f;
    
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isBreathing = true;
    
    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
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
    
    public void PlayIdle()
    {
        ResetToOriginalPose();
    }
    
    public void PlayBreathing()
    {
        StartCoroutine(SimpleBreath());
    }
    
    public void PlayJumping()
    {
        StartCoroutine(QuickJump());
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
    
    public void ResetToOriginalPose()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
    }
    
    // Method expected by SessionManager
    public void ResetToNaturalPose()
    {
        ResetToOriginalPose();
    }
    
    // Method expected by SessionManager
    public void ReactToPhrase(string phrase)
    {
        PlayBreathing();
    }
    
    // Method expected by SessionManager
    public void OnStartButtonPressed()
    {
        PlayJumping();
    }
    
    // Method expected by SessionManager
    public void OnStopButtonPressed()
    {
        PlayJumping();
    }
    
    // Method expected by SessionManager
    public void CelebrateSessionComplete()
    {
        StartCoroutine(CelebrationSequence());
    }
    
    IEnumerator CelebrationSequence()
    {
        // Do multiple jumps for celebration
        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(QuickJump());
            yield return new WaitForSeconds(0.2f);
        }
    }
}
