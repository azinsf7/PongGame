using System;
using System.Collections;
using System.Collections.Generic;
using FusionPong;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private bool isGameFinished;
    [SerializeField] private double timer;
    [SerializeField] private TextMeshProUGUI timerText;
    
    private void Awake()
    {
        StartCoroutine(StartTimer());
    }

    private IEnumerator StartTimer()
    {
        isGameFinished = false;

        while(timer >= 0)
        {
            // Countdown the timer with update time
            timer -= Time.deltaTime;
       //     Debug.Log("TIMER ISS " + timer);
            timerText.text = timer.ToString("F2");
            yield return null;
        }

        // End of time 
        isGameFinished = true;
        GameManager.Instance.CheckEndGame();
    }
}
