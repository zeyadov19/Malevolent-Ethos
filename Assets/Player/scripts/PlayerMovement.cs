using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;

    [Header("Better Jump")]
    public float fallMultiplier = 2.5f;

    [Header("Knockback Residual")]
    [Tooltip("How fast the horizontal knockback effect decays back to zero.")]
    public float knockbackDecayRate = 20f;
    [HideInInspector]
    public float knockbackResidualX = 0f;

    private float moveInput;
    private bool isGrounded;
    private bool isDashing;
    private float dashTime;
    private bool canDash = true;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (!isDashing)
        {
            // Flip sprite
            if (moveInput != 0)
                sr.flipX = moveInput < 0;

            // Jump
            if (Input.GetButtonDown("Jump") && isGrounded)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // Dash
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
                StartDash();
        }

        // Animator
        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // Ground check
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (!wasGrounded && isGrounded)
            canDash = true;

        // Dashing controls
        if (isDashing)
        {
            float dir = sr.flipX ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
            dashTime -= Time.fixedDeltaTime;
            if (dashTime <= 0f) isDashing = false;
            return;
        }

        // Normal movement plus residual knockback
        float horiz = moveInput * moveSpeed + knockbackResidualX;
        rb.linearVelocity = new Vector2(horiz, rb.linearVelocity.y);

        // Decay knockback residual toward zero
        knockbackResidualX = Mathf.MoveTowards(knockbackResidualX, 0f,
            knockbackDecayRate * Time.fixedDeltaTime);

        // Better fall gravity
        if (!Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = dashDuration;
        canDash = false;
        anim.SetTrigger("Dash");
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}