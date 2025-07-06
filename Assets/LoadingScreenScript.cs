using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimpleCountdown : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(2);
    }
}
