using UnityEngine;

public class HeroMapRotation2D : MonoBehaviour
{
    [SerializeField] private Transform mapRoot;
    [SerializeField] private Transform heroTransform;
    [SerializeField] private float rotationSpeed = 180f;

    private void Update()
    {
        if (mapRoot == null)
            return;

        float rotateInput = 0f;

        if (Input.GetKey(KeyCode.Q))
            rotateInput = 1f;
        else if (Input.GetKey(KeyCode.E))
            rotateInput = -1f;

        mapRoot.Rotate(0f, 0f, rotateInput * rotationSpeed * Time.deltaTime);
        heroTransform.Rotate(0f, 0f, -rotateInput * rotationSpeed * Time.deltaTime);
    }
}