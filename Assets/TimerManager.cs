using System;
using System.Collections;
using System.Collections.Generic;
using FusionPong;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private bool isGameFinished;
    private double timer;
    
    private readonly IGameManager gameManager;

    public TimerManager(IGameManager gameManager)
    {
        this.gameManager = gameManager;
    }
    private void Awake()
    {
        StartCoroutine(StartTimer());
    }

    private IEnumerator StartTimer()
    {
        isGameFinished = false;

        while(timer > 0)
        {
            // Countdown the timer with update time
            timer -= Time.deltaTime;
            Debug.Log("TIMER ISS " + timer);
            
            yield return null;
        }

        // End of time 
        isGameFinished = true;
        gameManager.CheckEndGame();
    }
}
