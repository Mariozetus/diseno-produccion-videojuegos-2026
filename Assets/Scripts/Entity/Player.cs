using UnityEngine;

public class Player : MotorEntity
{
    void FixedUpdate()
    {
        if (GameManager.gameState != GameState.playing) { return; }
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

        if (motor.IsGrounded && InputManager.jump > 0)
        {
            _velocity.y = Mathf.Sqrt(2f * motor.jumpHeight * -motor.gravity);
        }

        if (motor.IsGrounded && InputManager.jump < 0)
        {
            _velocity.y *= 3f;
        }

        var smoothedMovementFactor = motor.IsGrounded ? motor.groundDamping : motor.inAirDamping;
        _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * motor.runSpeed, Time.fixedDeltaTime * smoothedMovementFactor);
        _velocity.y += motor.gravity * Time.fixedDeltaTime;
        motor.Move(_velocity * Time.fixedDeltaTime);
        _velocity = motor.velocity;
    }
}
