using UnityEngine;

public class Player : MotorEntity
{
    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    private float lastGroundedTime;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.1f;
    private float lastJumpTime;

    [Header("Variable Jump")]
    public float lowJumpMultiplier = 2f;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 1f;
    private bool isWallSliding = false;

    [Header("Wall Jump")]
    public float wallJumpForce = 13.7f;
    public Vector2 wallJumpDirection = new Vector2(0.5f, 1f).normalized;

    [Header("Dash")]
    public float dashSpeed = 100f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 0.4f;
    private float lastDashTime;
    private bool isDashing = false;

    void FixedUpdate()
    {
        if (GameManager.gameState != GameState.playing) { return; }

        // Update timers
        if (motor.IsGrounded)
        {
            lastGroundedTime = Time.time;
        }
        if (InputManager.jump > 0)
        {
            lastJumpTime = Time.time;
        }

        // Dash logic
        if (InputManager.dashPressed && Time.time - lastDashTime > dashCooldown)
        {
            isDashing = true;
            lastDashTime = Time.time;
            int dir = normalizedHorizontalSpeed != 0 ? (int)normalizedHorizontalSpeed : (transform.localScale.x > 0 ? 1 : -1);
            _velocity.x = dir * dashSpeed;
            _velocity.y = 0; // Optional: stop vertical movement during dash
            InputManager.dashPressed = false; // Consume the input
        }

        if (isDashing)
        {
            // During dash, maintain velocity, but slow down over time
            _velocity.x = Mathf.Lerp(_velocity.x, 0, Time.fixedDeltaTime * 5f);
            if (Time.time - lastDashTime > dashDuration)
            {
                isDashing = false;
            }
        }
        else
        {
            // Normal horizontal movement
            if (InputManager.xMovement != 0)
            {
                normalizedHorizontalSpeed = InputManager.xMovement;
                if (InputManager.xMovement < 0)
                {
                    if (transform.localScale.x > 0f)
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }
                else if (InputManager.xMovement > 0)
                {
                    if (transform.localScale.x < 0f)
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }
            }
            else
            {
                normalizedHorizontalSpeed = 0;
            }

            // Wall slide
            if ((motor.collisionState.left || motor.collisionState.right) && !motor.IsGrounded && _velocity.y < 0)
            {
                isWallSliding = true;
                _velocity.y = -wallSlideSpeed;
            }
            else
            {
                isWallSliding = false;
            }

            // Jump logic with coyote time and buffer
            bool canJump = motor.IsGrounded || (Time.time - lastGroundedTime < coyoteTime);
            bool jumpBuffered = Time.time - lastJumpTime < jumpBufferTime;

            if (canJump && jumpBuffered)
            {
                _velocity.y = Mathf.Sqrt(2f * motor.jumpHeight * -motor.gravity);
                lastJumpTime = 0; // Reset buffer
            }
            else if (isWallSliding && jumpBuffered)
            {
                // Wall jump
                int wallDir = motor.collisionState.left ? 1 : -1;
                _velocity.x = wallDir * wallJumpForce * wallJumpDirection.x;
                _velocity.y = wallJumpForce * wallJumpDirection.y;
                lastJumpTime = 0;
            }

            // Variable jump height
            if (_velocity.y > 0 && InputManager.jump <= 0)
            {
                _velocity.y += motor.gravity * lowJumpMultiplier * Time.fixedDeltaTime;
            }

            // Fast fall
            if (motor.IsGrounded && InputManager.jump < 0)
            {
                _velocity.y *= 3f;
            }
        }

        // Apply smoothing and gravity if not dashing
        if (!isDashing)
        {
            var smoothedMovementFactor = motor.IsGrounded ? motor.groundDamping : motor.inAirDamping;
            _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * motor.runSpeed, Time.fixedDeltaTime * smoothedMovementFactor);
            _velocity.y += motor.gravity * Time.fixedDeltaTime;
        }

        motor.Move(_velocity * Time.fixedDeltaTime);
        _velocity = motor.velocity;
    }
}