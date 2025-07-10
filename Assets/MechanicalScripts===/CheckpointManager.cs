using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Last Activated Bonfire")]
    [Tooltip("The GameObject of the last bonfire you hit.")]
    public GameObject CurrentCheckPoint;

    [Header("UI & Fade")]
    public CanvasGroup deathTextGroup;    // CanvasGroup containing “You Died”
    public TextMeshProUGUI DeathText;     // the big “You Died” text
    public float fadeDuration      = 1f;
    public float deathTextDuration = 1.5f;
    public float textDuration      = 2f;

    [Header("Respawn Data")]
    [SerializeField]
    private Vector2 respawnPosition;

    // internal
    private bool   isRespawning  = false;
    private string sceneToReload = null;

    void Awake()
    {
        // singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // start fully hidden
        if (deathTextGroup != null) deathTextGroup.alpha = 0f;
        if (DeathText       != null) DeathText.color      = Color.clear;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Call from your bonfire trigger. Now also stores the bonfire GO.
    /// </summary>
    public void SetCheckpoint(Vector2 worldPos, GameObject checkpointGO)
    {
        respawnPosition   = worldPos;
        CurrentCheckPoint = checkpointGO;
        Debug.Log($"Checkpoint set: {checkpointGO.name} @ {worldPos}");
    }

    /// <summary>
    /// Call this from PlayerStats when the player dies.
    /// </summary>
    public void HandlePlayerDeath()
    {
        if (isRespawning) return;
        StartCoroutine(DeathAndReload());
    }

    private IEnumerator DeathAndReload()
    {
        isRespawning = true;

        // 1) Fade in the “You Died” canvas
        float t = 0f;
        while (t < fadeDuration)
        {
            deathTextGroup.alpha = t / fadeDuration;
            t += Time.deltaTime;
            yield return null;
        }
        deathTextGroup.alpha = 1f;

        yield return new WaitForSeconds(deathTextDuration);

        // 2) Fade the text itself to full red
        t = 0f;
        while (t < fadeDuration)
        {
            DeathText.color = new Color(170f/255f, 0f, 0f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(textDuration);

        // 3) Actually reload the scene
        sceneToReload = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneToReload);
        // OnSceneLoaded will fire next
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isRespawning || scene.name != sceneToReload)
            return;

        // 4) Find the *new* player instance
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // reposition
            player.transform.position = respawnPosition;

            // reset HP
            var ps = player.GetComponent<PlayerStats>();
            if (ps != null)
                ps.currentHealth = ps.maxHealth;

            // re-enable movement
            var pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.enabled = true;
        }

        // 5) Fade everything back out
        StartCoroutine(FadeEverythingOut());
    }

    private IEnumerator FadeEverythingOut()
    {
        // fade text
        float t = fadeDuration;
        while (t > 0f)
        {
            DeathText.color = new Color(170f/255f, 0f, 0f, t / fadeDuration);
            t -= Time.deltaTime;
            yield return null;
        }
        DeathText.color = Color.clear;

        // fade canvas
        t = fadeDuration;
        while (t > 0f)
        {
            deathTextGroup.alpha = t / fadeDuration;
            t -= Time.deltaTime;
            yield return null;
        }
        deathTextGroup.alpha = 0f;

        isRespawning = false;
    }
}
