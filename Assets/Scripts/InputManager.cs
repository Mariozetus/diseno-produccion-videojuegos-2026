using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static float jump;
    public static float xMovement;

    private void Update()
    {
        jump = Input.GetAxis("Jump");
        xMovement = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.gameState == GameState.playing || GameManager.gameState == GameState.pause)
            {
                GameManager.gameState = GameManager.gameState == GameState.playing ? GameState.pause : GameState.playing;
            }
        }
    }
}
