using UnityEngine;

public class PlayerWallMechanics : MonoBehaviour
{
    [SerializeField] float wallSlideMaxFallSpeed = 2f;
    [SerializeField] float wallJumpHorizontalSpeed = 10f;
    [SerializeField] float wallJumpHeight = 2.5f;

    Player player;
    MovementMotor motor;

    void Awake()
    {
        player = GetComponent<Player>();
        motor = GetComponent<MovementMotor>();
    }

    public void Tick(ref Vector3 velocity)
    {
        bool touchingWall = motor.collisionState.left || motor.collisionState.right;
        bool inAir = !player.IsGroundedByGravity;

        if (touchingWall && inAir && player.IsMovingTowardGravity(velocity.y))
        {
            float clamped = player.GravityInverted ? wallSlideMaxFallSpeed : -wallSlideMaxFallSpeed;
            if (player.GravityInverted)
            {
                velocity.y = Mathf.Min(velocity.y, clamped);
            }
            else
            {
                velocity.y = Mathf.Max(velocity.y, clamped);
            }
        }

        if (!touchingWall || !InputManager.jumpPressed)
        {
            return;
        }

        int wallDirection = motor.collisionState.left ? -1 : 1;

        velocity.x = -wallDirection * wallJumpHorizontalSpeed;
        velocity.y = player.GetJumpVelocity(wallJumpHeight);

        player.SetFacingDirection(velocity.x > 0f ? 1 : -1);
        player.ConsumeJumpWindowThisFrame();
    }
}

