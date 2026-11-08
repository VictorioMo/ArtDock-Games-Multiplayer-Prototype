using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkBall : NetworkBehaviour
{
    public const ulong NoHolder = ulong.MaxValue;

    private readonly NetworkVariable<ulong> _holderId = new(NoHolder);

    public bool IsHeld => _holderId.Value != NoHolder;

    public bool TryGrab(ulong clientId)
    {
        if (!IsServer || !IsSpawned || IsHeld)
            return false;

        _holderId.Value = clientId;

        NetworkObject.DontDestroyWithOwner = true;
        NetworkObject.ChangeOwnership(clientId);

        return true;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer || _holderId.Value != clientId)
            return;

        _holderId.Value = NoHolder;
        NetworkObject.RemoveOwnership();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}