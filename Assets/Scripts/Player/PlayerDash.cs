using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.12f;
    [SerializeField] float dashCooldown = 0.3f;
    [SerializeField] int maxAirDashes = 1;
    [SerializeField] LayerMask enemyMask;

    Player player;
    MovementMotor motor;
    PlayerVFX vfx;

    float dashTimeRemaining;
    float cooldownRemaining;
    int airDashesRemaining;
    int dashDirection = 1;

    public bool IsDashing => dashTimeRemaining > 0f;

    void Awake()
    {
        player = GetComponent<Player>();
        motor = GetComponent<MovementMotor>();
        vfx = GetComponent<PlayerVFX>();
        airDashesRemaining = maxAirDashes;
    }

    void Start()
    {
        if (enemyMask.value == 0)
        {
            enemyMask = LayerMask.GetMask("Enemy");
        }

        if (player != null)
        {
            player.onReset += ResetDashState;
        }
    }

    public void Tick(ref Vector3 velocity)
    {
        cooldownRemaining -= Time.fixedDeltaTime;

        if (player.IsGroundedByGravity)
        {
            airDashesRemaining = maxAirDashes;
        }

        if (InputManager.dashPressed && !IsDashing && cooldownRemaining <= 0f)
        {
            bool canDash = player.IsGroundedByGravity || airDashesRemaining > 0;
            if (canDash)
            {
                StartDash();
                if (!player.IsGroundedByGravity)
                {
                    airDashesRemaining -= 1;
                }
            }
        }

        if (!IsDashing)
        {
            return;
        }

        velocity.x = dashDirection * dashSpeed;
        velocity.y = 0f;

        KillEnemiesDuringDash();

        dashTimeRemaining -= Time.fixedDeltaTime;
        if (dashTimeRemaining <= 0f)
        {
            EndDash();
        }
    }

    void StartDash()
    {
        dashTimeRemaining = dashDuration;
        cooldownRemaining = dashCooldown;

        if (Mathf.Abs(InputManager.xMovement) > 0.01f)
        {
            dashDirection = InputManager.xMovement > 0f ? 1 : -1;
        }
        else
        {
            dashDirection = player.FacingDirection;
        }

        vfx?.SetDashTrail(true);
    }

    void EndDash()
    {
        dashTimeRemaining = 0f;
        vfx?.SetDashTrail(false);
    }

    public void CancelDash()
    {
        EndDash();
    }

    void KillEnemiesDuringDash()
    {
        var bounds = motor.boxCollider.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f, enemyMask);

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemyHealth = hits[i].GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(999f);
            }
        }
    }

    void ResetDashState()
    {
        airDashesRemaining = maxAirDashes;
        cooldownRemaining = 0f;
        EndDash();
    }
}

