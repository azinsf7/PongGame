using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FusionPong.UI
{
    public class UI_Game : MonoBehaviour
    {
        [SerializeField] private List<UI_PlayerInfo> playerInfos = new();
        [SerializeField] private TextMeshProUGUI startGameText;

        private void Start()
        {
            if (GameManager.InstanceExists)
                GameManager.OnGameStart.AddListener(HandleOnGameStart);
        }

        private void HandleOnGameStart()
        {
            StartCoroutine(DoGameStart());
        }

        private IEnumerator DoGameStart()
        {
            var localPlayer = GameManager.Instance.GetLocalPlayer();
            if (localPlayer.PlayerPosition == PlayerPosition.None)
            {
                Debug.LogError($"cant access position before it's set");
                yield return null;
            }
            
            var text = localPlayer.PlayerPosition == PlayerPosition.Left ? "<- YOU" : "YOU ->";

            startGameText.text = text;
            yield return new WaitForSecondsRealtime(GameManager.StartDelay);
            startGameText.text = string.Empty;
        }

        public UI_PlayerInfo GetPlayerInfo(PlayerPosition playerPosition)
        {
            return playerPosition switch
            {
                PlayerPosition.Left => playerInfos[0], //replace ids
                PlayerPosition.Right => playerInfos[1],
                _ => null
            };
        }
    }
}
