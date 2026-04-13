using UnityEngine;

public class Player : MotorEntity
{
    [Header("Jump Assist")]
    [SerializeField] float coyoteTimeWindow = 0.1f;
    [SerializeField] float jumpBufferWindow = 0.1f;
    [SerializeField] float jumpCutFactor = 0.5f;

    float coyoteTimer;
    float jumpBufferTimer;

    int facingDirection = 1;
    bool gravityInverted;
    bool jumpConsumedThisFrame;

    Vector3 baseScale;

    PlayerDash dash;
    PlayerWallMechanics wallMechanics;
    PlayerStomp stomp;
    PlayerGravityFlip gravityFlip;

    public bool GravityInverted => gravityInverted;
    public int FacingDirection => facingDirection;
    public bool IsGroundedByGravity => gravityInverted ? motor.collisionState.above : motor.collisionState.below;

    protected override void Start()
    {
        base.Start();
        baseScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), transform.localScale.z);

        dash = GetOrAdd<PlayerDash>();
        wallMechanics = GetOrAdd<PlayerWallMechanics>();
        stomp = GetOrAdd<PlayerStomp>();
        gravityFlip = GetOrAdd<PlayerGravityFlip>();
        GetOrAdd<PlayerVFX>();

        onReset += OnResetPlayer;
    }

    void FixedUpdate()
    {
        if (GameManager.gameState != GameState.playing)
        {
            return;
        }

        jumpConsumedThisFrame = false;

        HandleMovementInput();
        UpdateJumpTimers();

        if (wallMechanics != null)
        {
            wallMechanics.Tick(ref _velocity);
        }

        if (!jumpConsumedThisFrame && ShouldPerformJump())
        {
            PerformJump();
        }

        if (dash != null)
        {
            dash.Tick(ref _velocity);
        }

        if (stomp != null)
        {
            stomp.Tick(ref _velocity);
        }

        var smoothedMovementFactor = IsGroundedByGravity ? motor.groundDamping : motor.inAirDamping;

        if (!IsDashing())
        {
            _velocity.x = Mathf.Lerp(
                _velocity.x,
                normalizedHorizontalSpeed * motor.runSpeed,
                Time.fixedDeltaTime * smoothedMovementFactor
            );
        }

        if (InputManager.jumpReleased && IsMovingAwayFromGravity(_velocity.y))
        {
            _velocity.y *= jumpCutFactor;
        }

        _velocity.y += EffectiveGravity * Time.fixedDeltaTime;
        motor.Move(_velocity * Time.fixedDeltaTime);
        _velocity = motor.velocity;
    }

    void HandleMovementInput()
    {
        if (InputManager.xMovement != 0)
        {
            normalizedHorizontalSpeed = InputManager.xMovement;
            SetFacingDirection(InputManager.xMovement < 0f ? -1 : 1);
        }
        else
        {
            normalizedHorizontalSpeed = 0f;
        }
    }

    void UpdateJumpTimers()
    {
        if (IsGroundedByGravity)
        {
            coyoteTimer = coyoteTimeWindow;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }

        if (InputManager.jumpPressed)
        {
            jumpBufferTimer = jumpBufferWindow;
        }
        else
        {
            jumpBufferTimer -= Time.fixedDeltaTime;
        }
    }

    bool ShouldPerformJump()
    {
        return jumpBufferTimer > 0f && coyoteTimer > 0f;
    }

    void PerformJump()
    {
        _velocity.y = GetJumpVelocity(motor.jumpHeight);
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        jumpConsumedThisFrame = true;
    }

    public float GetJumpVelocity(float jumpHeight)
    {
        var speed = Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(EffectiveGravity));
        return -Mathf.Sign(EffectiveGravity) * speed;
    }

    public bool IsMovingTowardGravity(float velocityY)
    {
        return Mathf.Sign(velocityY) == Mathf.Sign(EffectiveGravity) && Mathf.Abs(velocityY) > 0.01f;
    }

    public bool IsMovingAwayFromGravity(float velocityY)
    {
        return Mathf.Sign(velocityY) != Mathf.Sign(EffectiveGravity) && Mathf.Abs(velocityY) > 0.01f;
    }

    public void ConsumeJumpWindowThisFrame()
    {
        jumpConsumedThisFrame = true;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
    }

    public void SetFacingDirection(int direction)
    {
        if (direction == 0)
        {
            return;
        }

        facingDirection = direction > 0 ? 1 : -1;
        ApplyVisualOrientation();
    }

    public void SetGravityInverted(bool value)
    {
        gravityInverted = value;
        gravityMultiplier = gravityInverted ? -1f : 1f;
        ApplyVisualOrientation();
    }

    public void AddImpulse(Vector2 impulse)
    {
        _velocity += (Vector3)impulse;
        if (dash != null)
        {
            dash.CancelDash();
        }
    }

    public bool IsDashing()
    {
        return dash != null && dash.IsDashing;
    }

    void ApplyVisualOrientation()
    {
        transform.localScale = new Vector3(
            baseScale.x * facingDirection,
            baseScale.y * (gravityInverted ? -1f : 1f),
            baseScale.z
        );
    }

    void OnResetPlayer()
    {
        gravityInverted = false;
        gravityMultiplier = 1f;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        normalizedHorizontalSpeed = 0f;
        _velocity = Vector3.zero;
        ApplyVisualOrientation();
    }

    T GetOrAdd<T>() where T : Component
    {
        var component = GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }
}
