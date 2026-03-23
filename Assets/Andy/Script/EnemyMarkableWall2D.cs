using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class EnemyMarkableWall2D : MonoBehaviour
{
    [SerializeField] private Color markedColor = Color.red;
    [SerializeField] private string enemyLayerName = "Enemy";

    private SpriteRenderer sr;
    private Color originalColor;
    private int originalLayer;
    private Coroutine markRoutine;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        originalLayer = gameObject.layer;
    }

    public void MarkAsEnemy(float duration)
    {
        if (markRoutine != null)
            StopCoroutine(markRoutine);

        markRoutine = StartCoroutine(MarkRoutine(duration));
    }

    private IEnumerator MarkRoutine(float duration)
    {
        sr.color = markedColor;

        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer != -1)
            gameObject.layer = enemyLayer;

        yield return new WaitForSeconds(duration);

        sr.color = originalColor;
        gameObject.layer = originalLayer;
        markRoutine = null;
    }
}