using System.Collections;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float blinkInterval = 0.08f;
    [SerializeField] Color dashColor = new Color(1f, 0.95f, 0.5f, 1f);

    Color defaultColor;
    Coroutine blinkRoutine;

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }
    }

    public void SetDashTrail(bool enabled)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = enabled ? dashColor : defaultColor;
    }

    public void SetGravityInvertedVisual(bool inverted)
    {
        // El flip visual principal lo aplica Player por escala; este hook queda para futuros VFX.
    }

    public void PlayInvincibilityBlink(float duration)
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        blinkRoutine = StartCoroutine(BlinkRoutine(duration));
    }

    IEnumerator BlinkRoutine(float duration)
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        float timeLeft = duration;
        bool visible = true;

        while (timeLeft > 0f)
        {
            visible = !visible;
            Color color = spriteRenderer.color;
            color.a = visible ? 1f : 0.25f;
            spriteRenderer.color = color;

            yield return new WaitForSeconds(blinkInterval);
            timeLeft -= blinkInterval;
        }

        spriteRenderer.color = defaultColor;
        blinkRoutine = null;
    }
}

