using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableEntity : Entity
{
    private Vector3 startPosition;
    private Vector3 startRotation;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        startRotation = transform.rotation.eulerAngles;
    }

    protected override void ResetEntity()
    {
        transform.rotation = Quaternion.Euler(startRotation);
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);
    }
}
