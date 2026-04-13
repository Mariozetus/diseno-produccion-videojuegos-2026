using UnityEngine;

public class PlayerStomp : MonoBehaviour
{
    [SerializeField] float bounceHeight = 2.25f;
    [SerializeField] float stompCheckDistance = 0.15f;
    [SerializeField] LayerMask enemyMask;

    Player player;
    MovementMotor motor;

    void Awake()
    {
        player = GetComponent<Player>();
        motor = GetComponent<MovementMotor>();
    }

    void Start()
    {
        if (enemyMask.value == 0)
        {
            enemyMask = LayerMask.GetMask("Enemy");
        }
    }

    public void Tick(ref Vector3 velocity)
    {
        if (player.IsDashing() || !player.IsMovingTowardGravity(velocity.y))
        {
            return;
        }

        Bounds bounds = motor.boxCollider.bounds;
        Vector2 castOrigin = player.GravityInverted
            ? new Vector2(bounds.center.x, bounds.max.y)
            : new Vector2(bounds.center.x, bounds.min.y);

        Vector2 castDirection = player.GravityInverted ? Vector2.up : Vector2.down;

        RaycastHit2D hit = Physics2D.BoxCast(castOrigin, new Vector2(bounds.size.x * 0.8f, 0.1f), 0f, castDirection, stompCheckDistance, enemyMask);
        if (!hit.collider)
        {
            return;
        }

        EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            return;
        }

        bool validTopHit = !player.GravityInverted
            ? transform.position.y > enemyHealth.transform.position.y
            : transform.position.y < enemyHealth.transform.position.y;

        if (!validTopHit)
        {
            return;
        }

        enemyHealth.TakeDamage(999f);
        velocity.y = player.GetJumpVelocity(bounceHeight);
        player.ConsumeJumpWindowThisFrame();
    }
}

