using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    public Health playerHealth;
    public Slider healthSlider;

    void Update()
    {
        healthSlider.value = Mathf.Clamp01(playerHealth.health / playerHealth.healthMax);
    }
}
