using System;
using Unity.Netcode;
using UnityEngine;

public sealed class ConnectionApprovalService : IDisposable
{
    private const int MaxPlayers = 4;

    private readonly NetworkManager _networkManager;

    public ConnectionApprovalService(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        _networkManager.ConnectionApprovalCallback += ApproveConnection;
    }

    private void ApproveConnection(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        bool hasRoom =
            _networkManager.ConnectedClientsIds.Count < MaxPlayers;

        response.Approved = hasRoom;
        response.CreatePlayerObject = hasRoom;
        response.Pending = false;

        if (!hasRoom)
        {
            response.Reason = "Session is full (maximum 4 players).";
            return;
        }

        // Spawn players at different positions.
        response.Position = new Vector3(
            UnityEngine.Random.Range(-3.5f, 3.5f),
            1.01f,
            UnityEngine.Random.Range(-3.5f, 3.5f)
        );

        response.Rotation = Quaternion.identity;
    }

    public void Dispose()
    {
        _networkManager.ConnectionApprovalCallback -= ApproveConnection;
    }
}