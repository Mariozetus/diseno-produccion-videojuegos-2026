using UnityEngine;

public class PlatformActivator : MonoBehaviour
{
    public Platform myPlatform;
    [SerializeField] MotorEntity playerEntity;

    private void Start()
    {
        myPlatform = GetComponent<Platform>();
        myPlatform.enabled = false;

        if (playerEntity == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerEntity = playerObj.GetComponent<MotorEntity>();
            }
        }

        if (playerEntity != null && playerEntity.motor != null)
        {
            playerEntity.motor.onControllerCollidedEvent += OnCollide;
        }
    }

    private void OnCollide(RaycastHit2D hit)
    {
        if (hit.collider.gameObject == gameObject)
        {
            myPlatform.enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            myPlatform.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            myPlatform.enabled = true;
        }
    }
}
