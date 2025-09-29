using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneController : MonoBehaviour
{
    public void EmpezarSesion()
    {
        SceneManager.LoadScene("MainScene");
    }
}

