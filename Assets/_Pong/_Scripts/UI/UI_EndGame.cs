using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FusionPong.UI
{
    public class UI_EndGame : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI winningPlayerText;
        [SerializeField] private float timeToDisconnect = 5f;
    
        private void Awake()
        {
            GameManager.OnGameEnd.AddListener((player) =>
            {
                gameObject.SetActive(true);
                StartCoroutine(DoHandleOnGameEnd(player, timeToDisconnect));
            });
    
            gameObject.SetActive(false);
        }
    
        private IEnumerator DoHandleOnGameEnd(Player player, float duration)
        {
            gameOverText.text = "Game Over!";
            
            var winningPlayerNumber = player.PlayerPosition == PlayerPosition.Left ? 1 : 2;
            winningPlayerText.text = $"Player {winningPlayerNumber.ToString()} wins" +
                                     $"{Environment.NewLine}" +
                                     $"{GameManager.GetScoreByPosition(PlayerPosition.Left)}" +
                                     $" - " +
                                     $"{GameManager.GetScoreByPosition(PlayerPosition.Right)}";
            
            yield return new WaitForSecondsRealtime(duration);
            gameObject.SetActive(false);
            NetworkManager.Instance.Disconnect();
        }
    }
}
