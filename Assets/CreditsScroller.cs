using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class CreditsScroller2D : MonoBehaviour
{
    [Header("UI References")]
    public GameObject creditsPanel;      // The root panel (initially inactive)
    public RectTransform content;        // The RectTransform of your scrolling credits text
    public Button quitButton;            // The Quit button (initially inactive)

    [Header("Scroll Settings")]
    public float scrollSpeed    = 20f;   // Units per second
    public float endPositionY   = 1000f; // Y to stop at (in anchoredPosition units)

    bool isScrolling = false;
    Collider2D triggerCollider;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;            // ensure it’s a trigger
    }

    void Start()
    {
        creditsPanel.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isScrolling) return;

        content.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (content.anchoredPosition.y >= endPositionY)
        {
            isScrolling = false;
            quitButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Preps and kicks off the scroll.
    /// </summary>
    public void StartScroll()
    {
        creditsPanel.SetActive(true);

        // place content just below the panel bottom
        float panelHeight = creditsPanel.GetComponent<RectTransform>().rect.height;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, -panelHeight);

        isScrolling = true;
    }

    /// <summary>
    /// 2D trigger callback
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isScrolling && other.CompareTag("Player"))
        {
            StartScroll();
            triggerCollider.enabled = false;   // fire only once
            AudioManager.instance.Play("Credits");
        }
    }

    /// <summary>
    /// Hook this to your Quit Button’s OnClick
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
