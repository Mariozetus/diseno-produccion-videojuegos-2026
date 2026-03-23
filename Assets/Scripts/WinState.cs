using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinState : MonoBehaviour
{
    [SerializeField] MotorEntity myEntity;
    [SerializeField] GameObject winPanel;

    [SerializeField] string layerForDamageCheck = "WinFlag";

    void Start()
    {
        myEntity.motor.onControllerCollidedEvent += OnCollide;
        myEntity.onReset += () => { winPanel.SetActive(false); };
    }

    void OnPlayerWin()
    {
        winPanel.SetActive(true);
        GameManager.gameState = GameState.win;
    }

    public void OnCollide(RaycastHit2D hit)
    {
        if(hit.collider.gameObject.layer == LayerMask.NameToLayer(layerForDamageCheck))
        {
            OnPlayerWin();
        }
    }
}
