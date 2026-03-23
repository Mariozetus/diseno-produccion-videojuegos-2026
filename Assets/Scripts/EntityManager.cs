using System.Collections.Generic;
using UnityEngine;
using Weasel.Utils;

public class EntityManager : MonoBehaviour
{
    [HideInInspector] public List<Entity> allEntities = new List<Entity>();

    private void Awake()
    {
        RefreshEntities();
    }

    public void RefreshEntities()
    {
        allEntities.Clear();
        allEntities.AddRange(SceneUtils.FindObjectsOfTypeAll<Entity>());
    }

    public void ResetAll()
    {
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (allEntities[i] == null)
                continue;

            allEntities[i].onReset?.Invoke();
        }
    }
}