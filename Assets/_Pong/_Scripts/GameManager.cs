using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using FusionPong.Game;
using FusionPong.UI;
using NoMossStudios.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace FusionPong
{
    public class GameManager : NetworkBehaviourSingleton<GameManager>, ISpawned,IGameManager
    {
        [SerializeField] private BallController ball;
        [SerializeField] private List<PaddleController> paddles = new();
        [SerializeField] private UI_Game gameUi;
        
        public static UnityEvent OnGameStart = new();
        public static UnityEvent<Player> OnGameEnd = new();

        public static bool GamePlaying => Instance.gamePlaying;
        [Networked(OnChanged = nameof(OnGamePlayingChanged))] public NetworkBool gamePlaying { get; set; } = false;
        public static void OnGamePlayingChanged(Changed<GameManager> changed)
        {
            if (changed.Behaviour.gamePlaying)
                changed.Behaviour.HandleOnGamePlayingChanged();
        }

        private void HandleOnGamePlayingChanged()
        {
            OnGameStart?.Invoke();
        }

        public UI_Game GameUi => gameUi;

        public const int GoalTarget = 3;
        public const float StartDelay = 3f;

        public static PaddleController GetPaddleByPosition(PlayerPosition position)
        {
            return position == PlayerPosition.Left ? Instance.paddles[0] : Instance.paddles[1];
        }

        public override void Spawned()
        {
            base.Spawned();
            
            Debug.Log($"GameManager Spawned - State Auth {Object.HasStateAuthority}");
            if (!Instance.Object.HasStateAuthority) return;

            StartCoroutine(SetupGame());
        }

        private IEnumerator SetupGame()
        {
            Debug.Log($"GameManager SetupGame called");
            yield return new WaitForEndOfFrame();
            
            
            foreach (var player in NetworkManager.Players)
            {
                SetupPlayer(player);
            }
            
            yield return new WaitForSecondsRealtime(1f);

            if (_playersSetup == 2)
                StartGame();
            else
            {
                //handle edge case where we dont have 2 players here, possibly a disconnect during scene change
                UI_StatusText.SetStatusText($"Error - Unable to start game");
                yield return new WaitForSecondsRealtime(1f);
                NetworkManager.Instance.Disconnect();
            }
        }

        private int _playersSetup = 0;
        
        private void SetupPlayer(Player player)
        {
            if (!NetworkManager.IsMaster &&
                (Runner.Topology != SimulationConfig.Topologies.Shared || player.Object.InputAuthority != Runner.LocalPlayer)) return;
            
            Debug.Log($"GameManager::HandlePlayerJoined called - PlayerRef {player.Object.InputAuthority}");
            
            //assign the player to a paddle

            if (_playersSetup == 0)
            {
                var pos = Random.value > 0.5f ? PlayerPosition.Left : PlayerPosition.Right;
                Debug.Log($"setting player {player.PlayerNumber} to pos {pos}");
                player.PlayerPosition = pos;
            }
            else
            {
                //second player to join, get OTHER paddle
                var otherPos = NetworkManager.Instance.GetOtherPlayer(player.Object.InputAuthority).PlayerPosition;
                var pos = otherPos == PlayerPosition.Left ? PlayerPosition.Right : PlayerPosition.Left;
                Debug.Log($"setting player {player.PlayerNumber} to pos {pos}");
                player.PlayerPosition = pos;
            }
            _playersSetup++;
        }

        private void HandlePlayerLeft(PlayerRef playerRef)
        {
            //player disconnected, end game with the remaining player as the winner
            UI_StatusText.SetStatusText($"PlayerRef {playerRef} Disconnected");
            EndGame(GetOtherPlayer(playerRef).Object.InputAuthority);
        }

        private void StartGame()
        {
            Debug.Log($"GameManager::StartGame called");
            
            if (!Instance.Object.HasStateAuthority) return;
            
            foreach (var player in NetworkManager.Players)
            {
                player.ResetPlayer();
            }
            
            Instance.ball.ResetBall(startDelay:StartDelay);
            
            gamePlaying = true;
        }
    
        public static Player GetPlayer(PlayerRef playerRef)
        {
            return NetworkManager.Instance.GetPlayer(playerRef);
        }

        public static Player GetPlayerByPos(PlayerPosition playerPosition)
        {
            return NetworkManager.Instance.GetPlayerByPos(playerPosition);
        }
        
        public static Player GetOtherPlayer(PlayerRef playerRef)
        {
            return NetworkManager.Instance.GetOtherPlayer(playerRef);
        }
        
        public static Player GetOtherPlayerByPos(PlayerPosition playerPosition)
        {
            return NetworkManager.Instance.GetOtherPlayerByPos(playerPosition);
        }
        
        public static Player GetLocalPlayer()
        {
            return NetworkManager.Instance.GetLocalPlayer();
        }

        public static int GetScoreByPosition(PlayerPosition playerPosition)
        {
            return NetworkManager.Instance.GetPlayerByPos(playerPosition).Score;
        }
    
        public static void GoalScored(PlayerPosition playerPosition)
        {
            if (!Instance.Object.HasStateAuthority) return;
            
            Debug.Log($"Player {playerPosition} scored a goal!");
    
            var player = GetPlayerByPos(playerPosition);
            Debug.Log($"{playerPosition} - PlayerNum: {player.PlayerNumber}");
            player.IncrementScore();
            
            if (player.Score >= GoalTarget)
            {
                Instance.ball.HideBall();
            }
            else
            {
                Instance.ball.ResetBall();
            }
        }

        public  void CheckEndGame()
        {
            if (!Instance.Object.HasStateAuthority) return;

            var players = NetworkManager.Players;

            if (players == null || players.Count == 0)
            {
                Debug.LogError("Player list is null");
                return;
            }

            var winnerPlayer = players.OrderByDescending(p => p.Score).FirstOrDefault();
            if (winnerPlayer != null) EndGame(winnerPlayer.Object.InputAuthority);
        }

        public void EndGame(PlayerRef playerRef)
        {
            ball.HideBall();
            OnGameEnd.Invoke(GetPlayer(playerRef));
        }
    }
}
