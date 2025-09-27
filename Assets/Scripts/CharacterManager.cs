using UnityEngine;

public class CharacterManager : MonoBehaviour 
{
    public GameObject capibara;
    public GameObject tungTung;
    
    void Start()
    {
        // Por defecto mostrar Capibara (para coincidir con dropdown Value = 0)
        ShowCapibara();
    }
    
    public void OnCharacterChanged(int characterIndex) 
    {
        Debug.Log("Cambiando a personaje con índice: " + characterIndex);
        
        if (characterIndex == 0) // Capibara
        {
            ShowCapibara();
        }
        else if (characterIndex == 1) // TungTung
        {
            ShowTungTung();
        }
    }
    
    void ShowCapibara()
    {
        capibara.SetActive(true);
        tungTung.SetActive(false);
        Debug.Log("Capibara activo");
    }
    
    void ShowTungTung()
    {
        capibara.SetActive(false);
        tungTung.SetActive(true);
        Debug.Log("TungTung activo");
    }
}