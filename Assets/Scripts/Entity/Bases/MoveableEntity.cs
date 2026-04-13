using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableEntity : Entity
{
    private Vector3 startPosition;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
    }

    protected override void ResetEntity()
    {
        transform.position = startPosition;
    }
}
