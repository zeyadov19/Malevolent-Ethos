// CheckpointManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("UI & Fade")]
    public GameObject CurrentCheckPoint;
    public Image fadeImage;        // full-screen black Image
    public CanvasGroup deathTextGroup;  // CanvasGroup containing “You Died”
    public float      fadeDuration     = 1f;
    public float      deathTextDuration = 1.5f;

    private Vector2 respawnPosition;    // world space of last bonfire

    void Awake()
    {
        // singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // if (fadeImage != null)
            //     DontDestroyOnLoad(fadeImage.canvas.gameObject);
            // if (deathTextGroup != null)
            //     DontDestroyOnLoad(deathTextGroup.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // start invisible
        if (fadeImage != null)
            fadeImage.color = Color.clear;
        if (deathTextGroup != null)
            deathTextGroup.alpha = 0f;
    }

    /// <summary>
    /// Called by each bonfire when activated.
    /// </summary>
    public void SetCheckpoint(Vector2 worldPos)
    {
        respawnPosition = worldPos;
    }

    /// <summary>
    /// Called by PlayerStats when the player finishes dying.
    /// </summary>
    public void HandlePlayerDeath(GameObject player)
    {
        StartCoroutine(RespawnRoutine(player));
    }

    private IEnumerator RespawnRoutine(GameObject player)
    {
        // 1) Fade in “You Died”
        float t = 0f;
        while (t < fadeDuration)
        {
            deathTextGroup.alpha = t / fadeDuration;
            t += Time.deltaTime;
            yield return null;
        }
        deathTextGroup.alpha = 1f;

        yield return new WaitForSeconds(deathTextDuration);

        // 2) Fade to black
        t = 0f;
        while (t < fadeDuration)
        {
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.black;

        // 3) Reload scene
        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
        yield return null; // wait one frame for load

        // 4) Find the player in the new scene
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 5) Teleport to last bonfire
            player.transform.position = respawnPosition;

            // 6) Reset health
            var ps = player.GetComponent<PlayerStats>();
            if (ps != null)
                ps.currentHealth = ps.maxHealth;

            // 7) Re-enable movement
            var pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.enabled = true;
        }

        // 8) Fade back in from black
        t = fadeDuration;
        while (t > 0f)
        {
            fadeImage.color = new Color(0, 0, 0, t / fadeDuration);
            t -= Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.clear;

        // 9) Hide “You Died”
        t = fadeDuration;
        while (t > 0f)
        {
            deathTextGroup.alpha = t / fadeDuration;
            t -= Time.deltaTime;
            yield return null;
        }
        deathTextGroup.alpha = 0f;
    }
}
