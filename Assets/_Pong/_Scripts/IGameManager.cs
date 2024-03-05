using FusionPong;
using UnityEngine.Events;

public interface IGameManager
{
    event UnityAction OnGameStart;
    event UnityAction<Player> OnGameEnd;
}