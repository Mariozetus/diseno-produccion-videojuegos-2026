using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static float jump;
    public static float xMovement;

    public static bool jumpPressed;
    public static bool jumpReleased;
    public static bool dashPressed;
    public static bool gravityFlipPressed;

    private void Update()
    {
        jump = Input.GetAxis("Jump");
        xMovement = Input.GetAxis("Horizontal");

        jumpPressed = Input.GetButtonDown("Jump");
        jumpReleased = Input.GetButtonUp("Jump");
        dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        gravityFlipPressed = Input.GetKeyDown(KeyCode.G);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.gameState == GameState.playing || GameManager.gameState == GameState.pause)
            {
                GameManager.gameState = GameManager.gameState == GameState.playing ? GameState.pause : GameState.playing;
            }
        }
    }
}
