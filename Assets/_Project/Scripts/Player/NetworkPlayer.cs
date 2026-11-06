using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        Debug.Log($"Player {OwnerClientId} spawned!");
    }
}