using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Health targetHealth;
    [SerializeField] private Image fillImage;

    private void Update()
    {
        if (targetHealth == null || fillImage == null)
            return;

        if (targetHealth.healthMax <= 0f)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        fillImage.fillAmount = Mathf.Clamp01(targetHealth.health / targetHealth.healthMax);
    }
}