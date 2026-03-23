using System;
using System.Collections;
using System.Collections.Generic;
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
