using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    playing,
    pause,
    win,
    lose
};


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Flag[] flags;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public static GameState gameState;

    public bool AllFlagsCollected()
    {
        if (flags == null || flags.Length == 0) return true;

        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i] != null && !flags[i].completed)
                return false;
        }

        return true;
    }
    
}