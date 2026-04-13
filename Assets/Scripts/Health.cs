using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEditor;

public class Health : MonoBehaviour
{
    [SerializeField] MotorEntity myEntity;
    public float healthMax = 100;
    public float health;

    [SerializeField] string layerForDamageCheck = "Enemy";

    public Action onDead;

    public void Start()
    {
        health = healthMax;
        myEntity.motor.onControllerCollidedEvent += OnCollide;
        myEntity.onReset += Reset;
    }

    public void OnCollide(RaycastHit2D hit)
    {
        if(hit.collider.gameObject.layer == LayerMask.NameToLayer(layerForDamageCheck))
        {
            health -= 1;
            if(health <= 0) { onDead.Invoke(); }
        }
    }

    public void Reset()
    {
        health = healthMax;
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
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.LabelField("Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
    }
}
#endif
