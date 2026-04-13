using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 1f;
    [SerializeField] float flashDuration = 0.08f;
    [SerializeField] Color flashColor = Color.white;

    float currentHealth;
    bool dead;

    SpriteRenderer spriteRenderer;
    Color defaultColor;
    Entity entity;

    public event Action onDead;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }

        entity = GetComponent<Entity>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (entity != null)
        {
            entity.onReset += ResetEnemy;
        }
    }

    public void TakeDamage(float amount)
    {
        if (dead)
        {
            return;
        }

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            StartCoroutine(DieRoutine());
        }
    }

    IEnumerator DieRoutine()
    {
        dead = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = defaultColor;
        }

        onDead?.Invoke();
        gameObject.SetActive(false);
    }

    void ResetEnemy()
    {
        dead = false;
        currentHealth = maxHealth;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }

        gameObject.SetActive(true);
    }
}

