using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEditor;

public class Health : MonoBehaviour
{
    [SerializeField] MotorEntity myEntity;
    [SerializeField] string layerForDamageCheck = "Enemy";
    [SerializeField] float invincibilityDuration = 1f;
    [SerializeField] Vector2 knockbackForce = new Vector2(6f, 4f);

    public float healthMax = 100;
    public float health;

    public Action onDead;
    public Action<float, float> onDamage;

    float invincibilityTimer;

    Player player;
    PlayerVFX playerVfx;

    public bool IsInvincible => invincibilityTimer > 0f;

    public void Start()
    {
        health = healthMax;

        player = myEntity as Player;
        playerVfx = GetComponent<PlayerVFX>();

        myEntity.motor.onControllerCollidedEvent += OnCollide;
        myEntity.onReset += Reset;
    }

    void Update()
    {
        invincibilityTimer -= Time.deltaTime;
    }

    public void OnCollide(RaycastHit2D hit)
    {
        if (hit.collider.gameObject.layer != LayerMask.NameToLayer(layerForDamageCheck))
        {
            return;
        }

        if (IsInvincible)
        {
            return;
        }

        if (player != null && player.IsDashing())
        {
            return;
        }

        health -= 1f;
        invincibilityTimer = invincibilityDuration;

        Vector2 hitDirection = ((Vector2)transform.position - hit.point).normalized;
        if (hitDirection == Vector2.zero)
        {
            int facing = player != null ? player.FacingDirection : 1;
            hitDirection = Vector2.right * -facing;
        }

        Vector2 directionalKnockback = new Vector2(
            Mathf.Sign(hitDirection.x) * knockbackForce.x,
            player != null ? player.GetJumpVelocity(knockbackForce.y) : knockbackForce.y
        );

        player?.AddImpulse(directionalKnockback);
        playerVfx?.PlayInvincibilityBlink(invincibilityDuration);

        onDamage?.Invoke(health, healthMax);

        if (health <= 0)
        {
            onDead?.Invoke();
        }
    }

    public void Reset()
    {
        health = healthMax;
        invincibilityTimer = 0f;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Health))]
public class HealthEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var myScript = (Health)target;
        if (!Application.isPlaying)
        {
            myScript.health = myScript.healthMax;
        }

        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("myEntity"));

        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthMax"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("layerForDamageCheck"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("invincibilityDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("knockbackForce"));

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.LabelField("Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
    }
}
#endif
