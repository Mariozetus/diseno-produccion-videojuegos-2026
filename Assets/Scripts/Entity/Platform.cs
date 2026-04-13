using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MoveableEntity
{
    public float speed = 3;
    [SerializeField] Transform[] movementPoints;
    int index = 0;

    [HideInInspector] public Vector3 velocity;

    protected override void Start()
    {
        base.Start();
        if (movementPoints.Length == 0) { gameObject.SetActive(false); }
    }

    void FixedUpdate()
    {
        if (GameManager.gameState != GameState.playing) { return; }
        Vector3 goalPosition = movementPoints[index].position;
        Vector2 _velocity = (goalPosition - transform.position).normalized;

        if (Vector3.Distance(transform.position,goalPosition) < 0.1f)
        {
            index += 1;
            if (index >= movementPoints.Length)
            {
                index = 0;
            }
        }
        velocity = Vector3.Lerp(velocity, _velocity * speed, Time.fixedDeltaTime * 3f);

        transform.position += velocity * Time.fixedDeltaTime;
    }
    protected override void ResetEntity()
    {
        base.ResetEntity();
        index = 0;
    }
}
