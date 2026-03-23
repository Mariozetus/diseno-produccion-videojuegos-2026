using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public Action onReset;

    protected virtual void Start()
    {
        onReset += ResetEntity;
    }

    protected abstract void ResetEntity();
}