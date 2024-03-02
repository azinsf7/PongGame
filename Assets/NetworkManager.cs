using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using FusionPong;
using FusionPong.UI;
using NoMossStudios.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Failed,
    WaitingForPlayers,
    Starting,
    Started,
    Ended
}

public struct NetworkInputData : INetworkInput
{
    public float Direction;
}

public class NetworkManager : PersistentSingleton<NetworkManager>, INetworkRunnerCallbacks
{
    [SerializeField] private string menuScene;
    [SerializeField] private string gameScene;
    public static string GameScene => Instance.gameScene;
    
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private Session sessionPrefab;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private int maxPlayer=2;

    private NetworkSceneManagerBase _loader;
    private NetworkRunner _runner;
    private Session _session;
    
    public Session Session
    {
        get => _session;
        set { _session = value; _session.transform.SetParent(_runner.transform); }
    }
    
    private readonly Dictionary<PlayerRef, Player> _players = new();
    public static ICollection<Player> Players => Instance._players.Values;
    
    public static bool IsMaster => Instance._runner != null && (Instance._runner.IsServer || Instance._runner.IsSharedModeMasterClient);

    public static UnityEvent<PlayerRef> PlayerJoinedEvent = new();
    public static UnityEvent<PlayerRef> PlayerLeftEvent = new();

    public static ConnectionStatus ConnectionStatus { get; private set; } = ConnectionStatus.Disconnected;
    
    protected override void Awake()
    {
        base.Awake();
        _loader ??= gameObject.AddComponent<NetworkSceneManagerDefault>();
    }

    private void Connect()
    {
        SetConnectionStatus(ConnectionStatus.Connecting);
        _players.Clear();

        if (_runner is not null) return;
        
        _runner = Instantiate(networkRunnerPrefab, transform);
        _runner.AddCallbacks(this);
    }
    
    public void Disconnect()
    {
        Debug.Log($"Disconnect Called");
        if (_runner is null) return;
        
        SetConnectionStatus(ConnectionStatus.Disconnected);
        _runner.Shutdown();
    }

    public void StartQuickGame(GameMode mode)
    {
        Connect();

        SetConnectionStatus(ConnectionStatus.Starting);
        
        _runner.ProvideInput = (mode != GameMode.Server);
        _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            CustomLobbyName = "ClassicPong",
            PlayerCount = maxPlayer,
            SceneManager = _loader
        });
    }
    
    public void SetPlayer(PlayerRef playerRef, Player player)
    {
        _players[playerRef] = player;
        player.transform.SetParent(_runner.transform);

        Debug.Log($"Setting Player reference for PlayerRef {playerRef}");
        
        if (Players.Count == maxPlayer)
            Session.LoadGameScene();
    }

    public Player GetPlayer(PlayerRef playerRef)
    {
        if (_runner is null)
        {
            Debug.LogWarning($"GetPlayer returning null, runner is null");
            return null;
        }
        
        _players.TryGetValue(playerRef, out var player);
        
        if (player is null)
            Debug.LogError($"GetPlayer returning null, TryGetValue fail");
        
        return player;
    }
    public Player GetPlayerByPos(PlayerPosition playerPosition)
    {
        if (_runner is null)
        {
            Debug.LogWarning($"GetPlayer returning null, runner is null");
            return null;
        }

        var player = Players.FirstOrDefault(p => p.PlayerPosition == playerPosition);
        
        if (player is null)
            Debug.LogError($"GetPlayer returning null, FirstOrDefault fail");
        
        return player;
    }
    public Player GetOtherPlayer(PlayerRef playerRef)
    {
        if (_runner is null)
        {
            Debug.LogWarning($"GetOtherPlayer returning null, runner is null");
            return null;
        }
        
        var kvp = Instance._players.FirstOrDefault(p => p.Key != playerRef);
        
        if (kvp.Value is null)
            Debug.LogError($"GetOtherPlayer returning null, kvp.Value is null");
        
        return kvp.Value;
    }
    public Player GetOtherPlayerByPos(PlayerPosition playerPosition)
    {
        if (_runner is null)
        {
            Debug.LogWarning($"GetOtherPlayer returning null, runner is null");
            return null;
        }
        
        var kvp = Instance._players.FirstOrDefault(p => p.Value.PlayerPosition != playerPosition);
        
        if (kvp.Value is null)
            Debug.LogError($"GetOtherPlayer returning null, kvp.Value is null");
        
        return kvp.Value;
    }
    public Player GetLocalPlayer()
    {
        if (_runner is null)
        {
            Debug.LogWarning($"GetOtherPlayer returning null, runner is null");
            return default;
        }

        _players.TryGetValue(_runner.Simulation.LocalPlayer, out var player);

        if (player is null)
            Debug.LogError($"GetLocalPlayer returning null, _runner.Simulation.LocalPlayer: {_runner.Simulation.LocalPlayer}");
        
        return player;
    }

    private static void SetConnectionStatus(ConnectionStatus status)
    {
        if (ConnectionStatus == status)
            return;
        
        ConnectionStatus = status;
        UI_StatusText.SetStatusText($"ConnectionStatus= {status}");
    }

#region CALLBACKS

    public void OnConnectedToServer(NetworkRunner runner)
    {
        UI_StatusText.SetStatusText("NetworkGameManager::OnConnectedToServer");
        SetConnectionStatus(ConnectionStatus.Connected);
    }
    
    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        UI_StatusText.SetStatusText("NetworkGameManager::OnDisconnectedFromServer");
        Disconnect();
    }
    
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        SetConnectionStatus(ConnectionStatus.Failed);
        Disconnect();
        
        UI_StatusText.SetStatusText($"NetworkGameManager::OnConnectFailed{Environment.NewLine}" +
                                    $"{remoteAddress.ToString()}{Environment.NewLine}" +
                                    $"{reason.ToString()}");
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnPlayerJoined{Environment.NewLine}" +
                                    $"{runner.SessionInfo.Name}{Environment.NewLine}" +
                                    $"PlayerRef: {playerRef.PlayerId}");
        
        if (ConnectionStatus == ConnectionStatus.Connecting)
            SetConnectionStatus(ConnectionStatus.Connected);

        if (_session is null && IsMaster)
        {
            Debug.Log("Spawning session");
            _session = runner.Spawn(sessionPrefab, Vector3.zero, Quaternion.identity);
        }

        if (runner.IsServer || runner.Topology == SimulationConfig.Topologies.Shared && playerRef == runner.LocalPlayer)
        {
            Debug.Log($"Spawning player for PlayerRef {playerRef}");
            runner.Spawn(
                playerPrefab,
                Vector3.zero,
                Quaternion.identity, 
                playerRef
                );
        }
        
        SetConnectionStatus(ConnectionStatus.Started);
        
        PlayerJoinedEvent?.Invoke(playerRef);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnPlayerLeft{Environment.NewLine}" +
                                    $"PlayerRef: {playerRef}");
        
        if (_players.TryGetValue(playerRef, out var player))
        {
            if (player.Object != null && player.Object.HasStateAuthority)
            {
                Debug.Log("Despawning Player");
                runner.Despawn(player.Object);
            }
            _players.Remove(playerRef);
        }
        
        PlayerLeftEvent?.Invoke(playerRef);
    }
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"OnShutdown Called");
        
        SetConnectionStatus(ConnectionStatus.Disconnected);
        UI_StatusText.SetStatusText($"NetworkGameManager::OnShutdown - {shutdownReason.ToString()}");
        
        if (_runner is not null && _runner.gameObject)
            Destroy(_runner.gameObject);

        _players.Clear();
        
        _runner = null;
        _session = null;

        if (Application.isPlaying)
        {
            SceneManager.LoadSceneAsync(menuScene);
        }
            
    }
    
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnConnectRequest - {request.RemoteAddress.ToString()}");
        request.Accept();
    }
    

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData
        {
            Direction = 0f
        };
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            data.Direction += 1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            data.Direction -= 1f;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        //Debug.Log($"NetworkGameManager::OnInputMissing - PlayerRef: {player}");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnUserSimulationMessage");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnSessionListUpdated - {sessionList.Count} sessions");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnCustomAuthenticationResponse");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnHostMigration");
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnReliableDataReceived - PlayerId: {player.PlayerId}");
    }
    
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnSceneLoadStart");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        UI_StatusText.SetStatusText($"NetworkGameManager::OnSceneLoadDone");
    }
    
#endregion
}
