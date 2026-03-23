using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    public Transform centerPoint;
    public float rotationSpeed = 50f;

    void Update()
    {
        transform.RotateAround(centerPoint.position, Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}