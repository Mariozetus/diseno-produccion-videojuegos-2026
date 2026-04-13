using UnityEngine;
using UnityEngine.SceneManagement;

public class InnovativeLevelBuilder : MonoBehaviour
{
    const string BuilderRootName = "InnovativeMechanicsLevel";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "Test Scene")
        {
            return;
        }

        if (GameObject.Find(BuilderRootName) != null)
        {
            return;
        }

        GameObject root = new GameObject(BuilderRootName);

        BuildGround(root.transform);
        BuildWallJumpSection(root.transform);
        BuildDashGapSection(root.transform);
        BuildGravityFlipSection(root.transform);
        BuildStompPit(root.transform);
    }

    static void BuildGround(Transform root)
    {
        CreatePlatform(root, "Ground_Extension", new Vector2(55f, -4f), new Vector2(20f, 1f), new Color(0.4f, 0.4f, 0.4f));
    }

    static void BuildWallJumpSection(Transform root)
    {
        CreatePlatform(root, "Wall_Left", new Vector2(38f, -0.25f), new Vector2(1f, 7f), new Color(0.55f, 0.6f, 0.8f));
        CreatePlatform(root, "Wall_Right", new Vector2(42f, 0.5f), new Vector2(1f, 8f), new Color(0.55f, 0.6f, 0.8f));
        CreatePlatform(root, "Wall_Target", new Vector2(45f, 5f), new Vector2(4f, 1f), new Color(0.8f, 0.8f, 0.4f));
    }

    static void BuildDashGapSection(Transform root)
    {
        CreatePlatform(root, "Dash_Start", new Vector2(50f, -1f), new Vector2(3f, 1f), new Color(0.4f, 0.8f, 0.5f));
        CreatePlatform(root, "Dash_End", new Vector2(58.5f, -1f), new Vector2(3f, 1f), new Color(0.4f, 0.8f, 0.5f));
        CreateEnemy(root, "DashEnemy", new Vector2(54.8f, -0.1f));
    }

    static void BuildGravityFlipSection(Transform root)
    {
        CreatePlatform(root, "Flip_Floor", new Vector2(66f, -2f), new Vector2(8f, 1f), new Color(0.8f, 0.4f, 0.8f));
        CreatePlatform(root, "Flip_Ceiling", new Vector2(66f, 4f), new Vector2(8f, 1f), new Color(0.8f, 0.4f, 0.8f));
        CreatePlatform(root, "Flip_Exit", new Vector2(72f, 4f), new Vector2(2f, 1f), new Color(0.9f, 0.7f, 0.2f));
    }

    static void BuildStompPit(Transform root)
    {
        CreatePlatform(root, "Stomp_Ledge", new Vector2(78f, 2f), new Vector2(5f, 1f), new Color(0.2f, 0.8f, 0.8f));
        CreateEnemy(root, "StompEnemy", new Vector2(78f, -1.2f));
    }

    static GameObject CreatePlatform(Transform root, string name, Vector2 pos, Vector2 scale, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(root);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.GetBuiltinResource<Sprite>("Square.psd");
        sr.color = color;

        go.AddComponent<BoxCollider2D>();
        return go;
    }

    static void CreateEnemy(Transform root, string name, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.layer = LayerMask.NameToLayer("Enemy");
        go.transform.SetParent(root);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(1f, 2f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.GetBuiltinResource<Sprite>("Square.psd");
        sr.color = new Color(0.85f, 0.3f, 0.3f);

        go.AddComponent<BoxCollider2D>();
        go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        go.AddComponent<EnemyHealth>();
    }
}
