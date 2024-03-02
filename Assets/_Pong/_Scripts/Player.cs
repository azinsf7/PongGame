using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

namespace FusionPong
{
    public enum PlayerPosition
    {
        None,
        Left,
        Right
    }
    
    public class Player : NetworkBehaviour, ISpawned
    {
        public UnityEvent<int> OnScoreChangedEvent = new();
        private List<IPlayerObserver> observers = new List<IPlayerObserver>();

        public int PlayerNumber => Object.InputAuthority + 1;

        [Networked(OnChanged = nameof(OnPlayerPositionChanged))] public PlayerPosition PlayerPosition { get; set; } = PlayerPosition.None;
        public static void OnPlayerPositionChanged(Changed<Player> changed)
        {
            Debug.Log($"OnPlayerPositionChanged called - PlayerNum {changed.Behaviour.PlayerNumber} - Position {changed.Behaviour.PlayerPosition}");
            
            if (changed.Behaviour.PlayerPosition == PlayerPosition.None) return;
            if (!GameManager.InstanceExists) return;

            GameManager.Instance.GameUi.GetPlayerInfo(changed.Behaviour.PlayerPosition).Init(changed.Behaviour);
            
            if (!changed.Behaviour.Object.HasStateAuthority) return;
            
            var paddle = GameManager.GetPaddleByPosition(changed.Behaviour.PlayerPosition);
            paddle.Object.AssignInputAuthority(changed.Behaviour.Object.InputAuthority);
        }
        
        [Networked(OnChanged = nameof(OnScoreChanged))] public int Score { get; set; }
        public static void OnScoreChanged(Changed<Player> changed)
        {
            changed.Behaviour.OnScoreChangedEvent.Invoke(changed.Behaviour.Score);
            
            //did we win?
            // if (changed.Behaviour.Score >= GameManager.GoalTarget)
            //     GameManager.Instance.EndGame(changed.Behaviour.Object.InputAuthority);
        }
        
        public override void Spawned()
        {
            Debug.Log($"Player {Object.InputAuthority} Spawned");
            
            NetworkManager.Instance.SetPlayer(Object.InputAuthority, this);
            
            if (Object.HasStateAuthority)
                Score = 0;
        }

        public void ResetPlayer()
        {
            if (Object.HasStateAuthority)
                Score = 0;
        }

        public void IncrementScore()
        {
            if (Object.HasStateAuthority)
                Score++;
        }
    }
}
