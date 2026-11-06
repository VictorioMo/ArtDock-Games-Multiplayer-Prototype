using System;
using Unity.Netcode;
using UnityEngine;

public sealed class LobbyState : NetworkBehaviour, ILobbyRoster
{
    private NetworkList<ulong> _players;
    private bool _available;

    public event Action Changed;

    public bool IsAvailable => _available;
    public int PlayerCount => _players?.Count ?? 0;

    public string GetPlayerName(int index)
    {
        ulong clientId = _players[index];

        return $"Player {clientId}" +
               (clientId == NetworkManager.ServerClientId
                   ? " (HOST)"
                   : "");
    }

    private void Awake()
    {
        _players = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        _available = true;

        _players.OnListChanged += OnPlayersChanged;

        if (IsServer)
        {
            _players.Clear();

            NetworkManager.OnClientConnectedCallback +=
                OnClientConnected;

            NetworkManager.OnClientDisconnectCallback +=
                OnClientDisconnected;

            // Include players already connected.
            foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
            {
                AddPlayer(clientId);
            }
        }

        Changed?.Invoke();
    }

    private void OnClientConnected(ulong clientId)
    {
        AddPlayer(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i] == clientId)
            {
                _players.RemoveAt(i);
                break;
            }
        }
    }

    private void AddPlayer(ulong clientId)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i] == clientId)
                return;
        }

        _players.Add(clientId);
    }

    private void OnPlayersChanged(NetworkListEvent<ulong> change)
    {
        Changed?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -=
                OnClientConnected;

            NetworkManager.OnClientDisconnectCallback -=
                OnClientDisconnected;
        }

        _players.OnListChanged -= OnPlayersChanged;

        _available = false;
        Changed?.Invoke();
    }

    private void OnDestroy()
    {
        _players?.Dispose();
    }
}