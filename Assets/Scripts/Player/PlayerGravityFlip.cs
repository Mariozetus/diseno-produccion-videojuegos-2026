using UnityEngine;

public class PlayerGravityFlip : MonoBehaviour
{
    [SerializeField] float flipDuration = 3f;
    [SerializeField] float flipCooldown = 5f;

    Player player;
    PlayerVFX vfx;

    float activeTimer;
    float cooldownTimer;

    void Awake()
    {
        player = GetComponent<Player>();
        vfx = GetComponent<PlayerVFX>();
    }

    void Start()
    {
        if (player != null)
        {
            player.onReset += ResetFlip;
        }
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (InputManager.gravityFlipPressed && cooldownTimer <= 0f && activeTimer <= 0f)
        {
            ActivateFlip();
        }

        if (activeTimer <= 0f)
        {
            return;
        }

        activeTimer -= Time.deltaTime;
        if (activeTimer <= 0f)
        {
            DeactivateFlip();
        }
    }

    void ActivateFlip()
    {
        activeTimer = flipDuration;
        cooldownTimer = flipCooldown;
        player.SetGravityInverted(true);
        vfx?.SetGravityInvertedVisual(true);
    }

    void DeactivateFlip()
    {
        activeTimer = 0f;
        player.SetGravityInverted(false);
        vfx?.SetGravityInvertedVisual(false);
    }

    void ResetFlip()
    {
        activeTimer = 0f;
        cooldownTimer = 0f;
        player.SetGravityInverted(false);
        vfx?.SetGravityInvertedVisual(false);
    }
}

