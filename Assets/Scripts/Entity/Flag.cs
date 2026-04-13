using UnityEngine;

public class Flag : MonoBehaviour
{
    public bool completed;
    
    // Caching variables
    private SpriteRenderer spriteRenderer;
    private Collider2D flagCollider;

    void Start()
    {
        // Get the "tools" once at the beginning
        spriteRenderer = GetComponent<SpriteRenderer>();
        flagCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            completed = true;
            
            // Turn off the visual
            spriteRenderer.enabled = false;
            
            // Turn off the physics so the player can definitely pass through
            flagCollider.enabled = false;

            Debug.Log("Flag collected!");
        }
    }
}