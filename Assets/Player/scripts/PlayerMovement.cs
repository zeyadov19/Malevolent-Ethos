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

    [Header("Better Jump")]
    public float fallMultiplier = 2.5f;

    [Header("Knockback Residual")]
    [Tooltip("How fast the horizontal knockback effect decays back to zero.")]
    public float knockbackDecayRate = 20f;
    [HideInInspector] public float knockbackResidualX = 0f;

    [HideInInspector] public bool canDash = true;

    private float moveInput;
    private bool  isGrounded;
    private float walkSoundTimer = 0f;

    private Rigidbody2D rb;
    private Animator      anim;
    private SpriteRenderer sr;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Flip
        if (moveInput != 0)
            sr.flipX = moveInput < 0;

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            AudioManager.instance.PlayAt("PlayerJump", gameObject);
        }

        // Play walk sound every 0.3 seconds while moving and grounded
        if (Mathf.Abs(moveInput) > 0.01f && isGrounded)
        {
            if (walkSoundTimer <= 0f)
            {
                AudioManager.instance.PlayAt("PlayerWalk", gameObject);
                walkSoundTimer = 0.1f;
            }
        }
        else
        {
            AudioManager.instance.StopAt("PlayerWalk", gameObject);
            walkSoundTimer = 0f;
        }

        // Anim
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

        // Movement + knockback
        float horiz = moveInput * moveSpeed + knockbackResidualX;
        rb.linearVelocity = new Vector2(horiz, rb.linearVelocity.y);

        // Decay residual
        knockbackResidualX = Mathf.MoveTowards(knockbackResidualX, 0f,
            knockbackDecayRate * Time.fixedDeltaTime);

        // Gravity logic my own cooking
        if (rb.linearVelocityY <= 0.1f || !Input.GetButton("Jump"))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                * (fallMultiplier - 1f) * Time.fixedDeltaTime;
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
