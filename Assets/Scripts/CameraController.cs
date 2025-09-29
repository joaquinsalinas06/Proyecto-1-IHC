using UnityEngine;
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Range(30f, 120f)]
    public float fieldOfView = 75f;
    [Header("Auto Apply")]
    public bool autoApplyOnStart = true;
    private Camera arCamera;
    void Start()
    {
        arCamera = GetComponent<Camera>();
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
        if (autoApplyOnStart && arCamera != null)
        {
            ApplyFOV();
        }
    }
    void Update()
    {
        if (arCamera != null && arCamera.fieldOfView != fieldOfView)
        {
            ApplyFOV();
        }
    }
    [ContextMenu("Apply FOV")]
    public void ApplyFOV()
    {
        if (arCamera != null)
        {
            arCamera.fieldOfView = fieldOfView;
        }
        else
        {
        }
    }
    [ContextMenu("Reset FOV to Default")]
    public void ResetFOV()
    {
        fieldOfView = 60f;
        ApplyFOV();
    }
    [ContextMenu("Wide FOV (90°)")]
    public void SetWideFOV()
    {
        fieldOfView = 90f;
        ApplyFOV();
    }
    [ContextMenu("Ultra Wide FOV (110°)")]
    public void SetUltraWideFOV()
    {
        fieldOfView = 110f;
        ApplyFOV();
    }
}

