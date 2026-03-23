using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { playing, pause, win, lose };

public class GameManager : MonoBehaviour
{

    public static GameState gameState;
}
