using UnityEngine;
using UnityEngine.UI;

public class DungeonMasterVisionMaskUI : MonoBehaviour
{
    [SerializeField] private Image maskImage;
    [SerializeField] private float holeRadius = 0.12f;
    [SerializeField] private float edgeSoftness = 0.02f;

    private Material runtimeMaterial;

    private void Awake()
    {
        if (maskImage != null && maskImage.material != null)
        {
            runtimeMaterial = Instantiate(maskImage.material);
            maskImage.material = runtimeMaterial;
        }
    }

    private void Update()
    {
        if (runtimeMaterial == null)
            return;

        runtimeMaterial.SetFloat("_HoleRadius", holeRadius);
        runtimeMaterial.SetFloat("_EdgeSoftness", edgeSoftness);

        Vector2 holeUV = GetMouseUVInRightPanel();
        runtimeMaterial.SetVector("_HoleCenter", new Vector4(holeUV.x, holeUV.y, 0f, 0f));
    }

    private Vector2 GetMouseUVInRightPanel()
    {
        Vector3 mouse = Input.mousePosition;

        float rightStartX = Screen.width * 0.5f;
        float rightWidth = Screen.width * 0.5f;

        float localX = Mathf.Clamp(mouse.x - rightStartX, 0f, rightWidth);
        float normalizedX = Mathf.Clamp01(localX / rightWidth);
        float normalizedY = Mathf.Clamp01(mouse.y / Screen.height);

        return new Vector2(normalizedX, normalizedY);
    }
}