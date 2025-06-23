using UnityEngine;


public class PlayerController : MonoBehaviour
{


    private bool isFacingRight = true;

    [Header("Movement")]
    public float runSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 50f;
    private float currentSpeed;

    [Header("Jump")]
    public float jumpForce = 16f;
    public float variableJumpTime = 0.2f;
    private float jumpTimeCounter;
    private bool isJumping;

    [Header("Double Jump")]
    public bool enableDoubleJump = true;
    private bool canDoubleJump;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 2f;
    public Transform wallCheck;
    public float wallCheckRadius = 0.1f;
    public LayerMask wallLayer;
    private bool isTouchingWall;
    private bool isWallSliding;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    private bool isGrounded;


    [Header("Gravity")]
    [SerializeField] private float baseGravity = 3f;
    [SerializeField] private float maxFallGravity = 18f;
    [SerializeField] private float fallSpeedMultiplier = 2f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sprite;
    public GunController gunController;
    [SerializeField] private ParticleSystem smokeFX;
    public Transform gunRightPos;
    public Transform gunLeftPos;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CheckEnvironment();
        HandleMovementInput();
        HandleJumpInput();
        UpdateAnimations();

        if (Input.GetMouseButtonDown(0))
        {
            gunController.Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }

        // Quay nhân vật theo hướng chuột
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mouseWorldPos.x < transform.position.x)
            sprite.flipX = true;
        else
            sprite.flipX = false;

        // Đổi vị trí GunHolder theo hướng nhân vật
        if (gunController != null)
        {
            Transform gunHolder = gunController.transform.parent;

            // Đặt GunHolder về đúng vị trí tay (Left hoặc Right)
            if (sprite.flipX)
            {
                gunHolder.position = gunLeftPos.position;
                gunController.transform.localScale = new Vector3(1, -1, 1);
            }
            else
            {
                gunHolder.position = gunRightPos.position;
                gunController.transform.localScale = new Vector3(1, 1, 1);
            }

            // Tính hướng từ súng đến chuột
            Vector2 direction = (mouseWorldPos - gunHolder.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Xoay GunHolder theo hướng chuột
            gunHolder.rotation = Quaternion.Euler(0, 0, angle);

            // Không cần xoay hoặc scale GunController nữa
        }
    }

    private void GravityControl()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Apply increased gravity when falling
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallGravity));
        }
        else
        {
            // Reset gravity scale when not falling
            rb.gravityScale = baseGravity;
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyWallSlide();
    }

    void CheckEnvironment()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        // Reset double jump when grounded
        if (isGrounded)
            canDoubleJump = enableDoubleJump;
    }

    void HandleMovementInput()
    {
        float targetSpeed = Input.GetAxisRaw("Horizontal") * runSpeed;
        if (Mathf.Abs(targetSpeed) > 0.01f)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
    }

    void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            sprite.flipX = currentSpeed < 0;

            // Hiệu ứng khói khi chạy trên mặt đất
            if (isGrounded && !smokeFX.isPlaying)
            {
                smokeFX.Play();
            }
        }
        else
        {
            // Dừng hiệu ứng khói khi không chạy
            if (smokeFX.isPlaying)
            {
                smokeFX.Stop();
            }
        }
    }

    void HandleJumpInput()
    {
        // Start jump or double jump
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                isJumping = true;
                jumpTimeCounter = variableJumpTime;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (!isGrounded && canDoubleJump)
            {
                isJumping = true;
                jumpTimeCounter = variableJumpTime;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                // animator.SetTrigger("doubleJumpTrigger");
                canDoubleJump = false;

                // Hiệu ứng khói khi double jump
                if (smokeFX != null)
                {
                    smokeFX.Play();
                }
            }
        }

        // Hold for variable jump height
        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }
        if (Input.GetButtonUp("Jump"))
            isJumping = false;
    }


    void ApplyWallSlide()
    {
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    void UpdateAnimations()
    {
        bool isJumping = !isGrounded;
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isRunning", Mathf.Abs(currentSpeed) > 0.1f && isGrounded);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isWallSliding", isWallSliding);
        animator.SetFloat("verticalSpeed", rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        if (wallCheck)
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}
