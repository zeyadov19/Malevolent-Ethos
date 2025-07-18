using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class DashAbility : MonoBehaviour
{
    [Header("Dash Settings")]
    [Tooltip("Unlocked dash speed.")]
    public float dashSpeed = 20f;
    [Tooltip("Duration of one dash in seconds.")]
    public float dashDuration = 0.2f;
    [Tooltip("Stamina cost per dash.")]
    public int dashStaminaCost = 10;

    private PlayerMovement movement;
    private PlayerStats    stats;
    private Rigidbody2D    rb;
    private Animator       anim;
    private SpriteRenderer sr;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        stats    = GetComponent<PlayerStats>();
        rb       = GetComponent<Rigidbody2D>();
        anim     = GetComponent<Animator>();
        sr       = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // If no dash component, locked out, or already dashing, skip
        if (movement == null || !movement.canDash)
            return;

        // Attempt dash on LeftShift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // Spend stamina first
            if (stats != null && stats.SpendStamina(dashStaminaCost))
            {
                StartCoroutine(PerformDash());
            }
            else
                Debug.Log("Not enough stamina to dash!");
        }
    }

    private IEnumerator PerformDash()
    {
        // Lock out further dashes & normal movement
        //movement.canDash = false;
        movement.enabled = false;

        anim.SetTrigger("Dash");

        float timer = dashDuration;
        while (timer > 0f)
        {
            float dir = sr.flipX ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);

            timer -= Time.deltaTime;
            yield return null;
        }

        // Re-enable normal movement
        movement.enabled = true;
    }
}
