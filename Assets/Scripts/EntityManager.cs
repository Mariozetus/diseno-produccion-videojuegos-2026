using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weasel.Utils;

public class EntityManager : MonoBehaviour
{
    [HideInInspector] public List<Entity> allEntities;

    public void Start()
    {
        allEntities= new List<Entity>();
        allEntities.AddRange(SceneUtils.FindObjectsOfTypeAll<Entity>());
    }

    public void ResetAll()
    {
        for (int i = 0; i < allEntities.Count; i++)
        {
            allEntities[i].onReset.Invoke();
        }
    }
}
