using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
   public void Start()
    {
        AudioManager.instance.Play("MMBGM");
    }
    public void StartGame()
    {
        Debug.Log("Start button clicked");
        AudioManager.instance.Play("Button");
        AudioManager.instance.Stop("MMBGM");
        SceneManager.LoadScene("Loading Scene"); // Replace with your actual scene name
    }

    public void QuitGame()
    {
        Debug.Log("Quit button clicked");
         AudioManager.instance.Play("Button");
        Application.Quit();

        // This won't quit in the editor, just logs it
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
