using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnSystem : MonoBehaviour
{
    [SerializeField] EntityManager entityManager;
    [SerializeField] Health playerHealth;
    [SerializeField] GameObject lostPanel;

    void Start()
    {
        playerHealth.onDead += OnPlayerDead;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            lostPanel.SetActive(false);
            entityManager.ResetAll();
            GameManager.gameState = GameState.playing;
        }
    }

    void OnPlayerDead()
    {
        lostPanel.SetActive(true);
        GameManager.gameState = GameState.lose;
    }
}
