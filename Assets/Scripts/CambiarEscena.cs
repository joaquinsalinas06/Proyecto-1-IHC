using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscena : MonoBehaviour
{
    public void CargarEscena(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    public void IrAMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void IrAMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void IrATutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
