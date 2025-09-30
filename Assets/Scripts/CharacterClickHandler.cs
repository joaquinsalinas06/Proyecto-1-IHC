using UnityEngine;

public class CharacterClickHandler : MonoBehaviour
{
    [Header("Click Detection")]
    [Tooltip("Reference to the SessionManager to notify when clicked")]
    public SessionManager sessionManager;
    
    [Header("Visual Feedback")]
    [Tooltip("Scale multiplier when character is pressed")]
    public float pressedScale = 0.9f;
    
    [Tooltip("Duration of the press animation")]
    public float pressAnimationDuration = 0.1f;
    
    private Vector3 originalScale;
    private bool isPressed = false;
    
    void Start()
    {
        // Store the original scale
        originalScale = transform.localScale;
        
        // Auto-find SessionManager if not assigned
        if (sessionManager == null)
        {
            sessionManager = FindObjectOfType<SessionManager>();
        }
        
        Debug.Log($"CharacterClickHandler initialized for: {gameObject.name}");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"SessionManager found: {sessionManager != null}");
        
        // Ensure the character has a collider for click detection
        SetupCollider();
    }
    
    void SetupCollider()
    {
        // Check if there's already a collider
        Collider existingCollider = GetComponent<Collider>();
        Collider2D existingCollider2D = GetComponent<Collider2D>();
        
        Debug.Log($"Setting up collider for {gameObject.name}");
        Debug.Log($"Has 3D Collider: {existingCollider != null}");
        Debug.Log($"Has 2D Collider: {existingCollider2D != null}");
        
        if (existingCollider == null && existingCollider2D == null)
        {
            // Check for various components to determine the best collider type
            Renderer renderer = GetComponent<Renderer>();
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            RectTransform rectTransform = GetComponent<RectTransform>();
            
            if (rectTransform != null)
            {
                // UI element - we'll handle this differently
                Debug.Log($"{gameObject.name} is a UI element (RectTransform detected)");
                // For UI elements, we don't add colliders, we'll use a different approach
                return;
            }
            else if (spriteRenderer != null)
            {
                // 2D Sprite
                BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
                boxCollider2D.isTrigger = false;
                Debug.Log($"Added BoxCollider2D to {gameObject.name}");
            }
            else if (renderer != null)
            {
                // 3D Object
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = false;
                Debug.Log($"Added BoxCollider to {gameObject.name}");
            }
            else
            {
                // Fallback - add a 2D collider
                BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
                boxCollider2D.isTrigger = false;
                Debug.Log($"Added fallback BoxCollider2D to {gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"{gameObject.name} already has a collider");
        }
    }
    
    void OnMouseDown()
    {
        // Handle mouse/touch press
        Debug.Log($"Character {gameObject.name} was pressed!");
        if (sessionManager != null)
        {
            OnCharacterPressed();
        }
        else
        {
            Debug.LogWarning($"SessionManager not found for {gameObject.name}");
        }
    }
    
    void OnMouseUpAsButton()
    {
        // Handle mouse/touch release (only if released over the same object)
        Debug.Log($"Character {gameObject.name} was clicked!");
        if (sessionManager != null)
        {
            OnCharacterReleased();
            OnCharacterClicked();
        }
        else
        {
            Debug.LogWarning($"SessionManager not found for {gameObject.name}");
        }
    }
    
    void OnCharacterPressed()
    {
        if (!isPressed)
        {
            isPressed = true;
            // Animate scale down for visual feedback
            StartCoroutine(AnimateScale(originalScale * pressedScale));
        }
    }
    
    void OnCharacterReleased()
    {
        if (isPressed)
        {
            isPressed = false;
            // Animate scale back to original
            StartCoroutine(AnimateScale(originalScale));
        }
    }
    
    void OnCharacterClicked()
    {
        // Notify SessionManager that this character was clicked
        sessionManager.OnCharacterClicked();
    }
    
    System.Collections.IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < pressAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / pressAnimationDuration;
            
            // Smooth animation curve
            progress = Mathf.SmoothStep(0f, 1f, progress);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    // Alternative method for UI Button if characters are UI elements
    public void OnCharacterButtonClicked()
    {
        OnCharacterPressed();
        OnCharacterReleased();
        OnCharacterClicked();
    }
    
    // Method to handle touch input on mobile devices
    void Update()
    {
        // Handle mouse clicks and touch input
        bool inputDetected = false;
        Vector3 inputPosition = Vector3.zero;
        
        // Handle mouse input (for testing in editor)
        if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
            inputDetected = true;
            Debug.Log($"Mouse click detected at: {inputPosition}");
        }
        
        // Handle touch input for mobile devices
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                inputPosition = touch.position;
                inputDetected = true;
                Debug.Log($"Touch detected at: {inputPosition}");
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (isPressed)
                {
                    OnCharacterReleased();
                    OnCharacterClicked();
                }
                return;
            }
        }
        
        if (inputDetected)
        {
            // Use raycast to detect if we clicked on this character
            bool hitThisCharacter = IsInputOnCharacter(inputPosition);
            Debug.Log($"Character {gameObject.name} checking input at {inputPosition} - Hit: {hitThisCharacter}");
            
            if (hitThisCharacter)
            {
                Debug.Log($"SUCCESS! Input hit character: {gameObject.name}");
                // Check if this is the closest character to avoid multiple triggers
                if (IsClosestCharacterToClick(inputPosition))
                {
                    Debug.Log($"This is the closest character to the click!");
                    OnCharacterPressed();
                    // For mouse clicks, immediately release
                    if (Input.GetMouseButtonDown(0))
                    {
                        OnCharacterReleased();
                        OnCharacterClicked();
                    }
                }
            }
        }
    }
    
    bool IsTouchingCharacter(Vector3 worldPos)
    {
        // Use collider bounds to check if touch is within character
        Collider col = GetComponent<Collider>();
        Collider2D col2D = GetComponent<Collider2D>();
        
        if (col != null)
        {
            return col.bounds.Contains(worldPos);
        }
        else if (col2D != null)
        {
            return col2D.bounds.Contains(worldPos);
        }
        
        return false;
    }
    
    bool IsInputOnCharacter(Vector3 screenPosition)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("Main camera not found!");
            return false;
        }
        
        Debug.Log($"[{gameObject.name}] Checking input at screen position: {screenPosition}");
        Debug.Log($"[{gameObject.name}] World position: {transform.position}");
        
        // Try raycast for 3D objects
        Ray ray = camera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"[{gameObject.name}] 3D Raycast hit: {hit.collider.gameObject.name}");
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log($"[{gameObject.name}] 3D Raycast SUCCESS!");
                return true;
            }
        }
        
        // Try raycast for 2D objects  
        Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane));
        Debug.Log($"[{gameObject.name}] Converted to world pos: {worldPos}");
        
        RaycastHit2D hit2D = Physics2D.Raycast(worldPos, Vector2.zero);
        
        if (hit2D.collider != null)
        {
            Debug.Log($"[{gameObject.name}] 2D Raycast hit: {hit2D.collider.gameObject.name}");
            if (hit2D.collider.gameObject == gameObject)
            {
                Debug.Log($"[{gameObject.name}] 2D Raycast SUCCESS!");
                return true;
            }
        }
        
        // Fallback: Check if screen position is within bounds
        Vector3 screenPoint = camera.WorldToScreenPoint(transform.position);
        float distance = Vector2.Distance(screenPosition, screenPoint);
        
        Debug.Log($"[{gameObject.name}] Screen point: {screenPoint}, Distance: {distance}");
        
        // Use a much larger click radius to account for character positioning
        float clickRadius = 500f; // pixels - increased to cover the character area
        bool withinBounds = distance <= clickRadius;
        
        if (withinBounds)
        {
            Debug.Log($"[{gameObject.name}] FALLBACK SUCCESS - Input within bounds (distance: {distance})");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Outside click radius (distance: {distance}, radius: {clickRadius})");
        }
        
        return withinBounds;
    }
    
    bool IsClosestCharacterToClick(Vector3 screenPosition)
    {
        // Find all CharacterClickHandler components in the scene
        CharacterClickHandler[] allHandlers = FindObjectsOfType<CharacterClickHandler>();
        
        Camera camera = Camera.main;
        if (camera == null) return true; // If no camera, assume this one
        
        Vector3 thisScreenPoint = camera.WorldToScreenPoint(transform.position);
        float thisDistance = Vector2.Distance(screenPosition, thisScreenPoint);
        
        // Check if any other character is closer
        foreach (CharacterClickHandler handler in allHandlers)
        {
            if (handler == this) continue; // Skip self
            
            Vector3 otherScreenPoint = camera.WorldToScreenPoint(handler.transform.position);
            float otherDistance = Vector2.Distance(screenPosition, otherScreenPoint);
            
            if (otherDistance < thisDistance)
            {
                Debug.Log($"Character {handler.gameObject.name} is closer ({otherDistance}) than {gameObject.name} ({thisDistance})");
                return false; // Another character is closer
            }
        }
        
        return true; // This is the closest character
    }
}