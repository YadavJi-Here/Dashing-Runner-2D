using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGamet()
    {
        SceneManager.LoadScene("Demo 1"); // Make sure "Gameplay" is in Build Settings
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // This only shows in Editor
    }
}
