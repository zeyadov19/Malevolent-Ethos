using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Call SceneTracker.GoToLoading() instead of SceneManager.LoadScene("Loading")  
/// to remember which scene you came from for the next step.
/// </summary>
public static class SceneTracker
{
    /// <summary>
    /// Name of the scene that just requested the load.
    /// </summary>
    public static string LastSceneName { get; private set; }

    /// <summary>
    /// Save the current scene’s name, then jump to your Loading scene.
    /// </summary>
    public static void GoToLoading()
    {
        LastSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("Loading Scene");  // make sure this matches your Loading scene’s name
    }
}
