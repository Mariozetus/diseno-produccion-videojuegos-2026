using UnityEngine;

public class MotorEntity : MoveableEntity
{
    public MovementMotor motor;
    protected float normalizedHorizontalSpeed = 0;
    protected Vector3 _velocity;
}