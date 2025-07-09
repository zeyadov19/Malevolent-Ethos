using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    [Header("Build Order (exclude Loading)")]
    [Tooltip("Scenes in play-order. First element is level after MainMenu.")]
    public string[] gameScenes = { "Map1", "Map2", "Map3" };

    [Tooltip("Name of your Main Menu scene.")]
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        // 1. Decide which comes next
        string last = SceneTracker.LastSceneName;
        string next;

        int idx = System.Array.IndexOf(gameScenes, last);
        if (idx >= 0 && idx < gameScenes.Length - 1)
        {
            // e.g. last was Map1 → go to Map2
            next = gameScenes[idx + 1];
        }
        else if (idx == -1)
        {
            // not in the gameScenes list – assume we're coming from MainMenu
            next = gameScenes.Length > 0 ? gameScenes[0] : mainMenuSceneName;
        }
        else
        {
            // idx == last level → wrap back to MainMenu
            next = mainMenuSceneName;
        }

        // (optional) show your loading UI here...
        yield return new WaitForSeconds(4f); // simulate loading UI delay
        // 2. Async load
        var op = SceneManager.LoadSceneAsync(next);
        while (!op.isDone)
            yield return null;
    }
}
