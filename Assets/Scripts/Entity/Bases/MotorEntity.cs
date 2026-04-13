using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorEntity : MoveableEntity
{
    public MovementMotor motor;
    [SerializeField] public float gravityMultiplier = 1f;

    protected float normalizedHorizontalSpeed = 0;
    protected Vector3 _velocity;

    public float EffectiveGravity => motor.gravity * gravityMultiplier;
}
