using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MotorEntity
{

    [SerializeField] Transform[] movementPoints;
    int index = 0;

    protected override void Start()
    {
        base.Start();
        if(movementPoints.Length == 0) { gameObject.SetActive(false); }
    }

    void FixedUpdate()
    {
        if (GameManager.gameState != GameState.playing) { return; }
        Vector3 goalPosition = movementPoints[index].position;
        normalizedHorizontalSpeed = (goalPosition - transform.position).normalized.x;
        
        if(Mathf.Abs(transform.position.x - goalPosition.x) <  0.1f)
        {
            index += 1;
            if (index >= movementPoints.Length)
            {
                index = 0;
            }
        }

        var smoothedMovementFactor = motor.IsGrounded ? motor.groundDamping : motor.inAirDamping;
        _velocity.x = Mathf.Lerp(_velocity.x, normalizedHorizontalSpeed * motor.runSpeed, Time.fixedDeltaTime * smoothedMovementFactor);
        _velocity.y += motor.gravity * Time.fixedDeltaTime;
        motor.Move(_velocity * Time.fixedDeltaTime);
        _velocity = motor.velocity;
    }

    protected override void ResetEntity()
    {
        base.ResetEntity();
        index = 0;
    }
}
