using UnityEngine;
using UnityEngine.UI;

public class DungeonMasterCooldownUI : MonoBehaviour
{
    [SerializeField] private DungeonMasterWallPainter wallPainter;
    [SerializeField] private Image fillImage;

    private void Update()
    {
        if (wallPainter == null || fillImage == null)
            return;

        fillImage.fillAmount = wallPainter.GetCooldownNormalized();
    }
}