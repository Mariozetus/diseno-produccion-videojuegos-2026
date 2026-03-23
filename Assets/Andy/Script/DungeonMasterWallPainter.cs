using UnityEngine;

public class DungeonMasterWallPainter : MonoBehaviour
{
    [SerializeField] private Camera dungeonCamera;
    [SerializeField] private LayerMask clickableWallMask;
    [SerializeField] private float markDuration = 3f;
    [SerializeField] private float cooldown = 2f;

    private float nextUseTime;

    private void Update()
    {
        if (dungeonCamera == null)
            return;

        if (!IsMouseInsideRightHalf())
            return;

        if (Time.time < nextUseTime)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryMarkWall();
        }
    }

    private void TryMarkWall()
    {
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 world = dungeonCamera.ScreenToWorldPoint(mouseScreen);
        Vector2 point = new Vector2(world.x, world.y);

        Collider2D hit = Physics2D.OverlapPoint(point, clickableWallMask);

        if (hit == null)
            return;

        EnemyMarkableWall2D wall = hit.GetComponent<EnemyMarkableWall2D>();
        if (wall == null)
            return;

        wall.MarkAsEnemy(markDuration);
        nextUseTime = Time.time + cooldown;
    }

    private bool IsMouseInsideRightHalf()
    {
        return Input.mousePosition.x >= Screen.width * 0.5f;
    }

    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, nextUseTime - Time.time);
    }

    public float GetCooldownNormalized()
    {
        if (cooldown <= 0f)
            return 1f;

        if (Time.time >= nextUseTime)
            return 1f;

        float remaining = nextUseTime - Time.time;
        return 1f - Mathf.Clamp01(remaining / cooldown);
    }
}