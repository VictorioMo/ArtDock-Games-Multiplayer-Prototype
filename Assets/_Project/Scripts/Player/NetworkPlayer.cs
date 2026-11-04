using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        // Spawn players at different positions.
        transform.position = new Vector3(
            OwnerClientId * 2f,
            1f,
            0f
        );

        Debug.Log($"Player {OwnerClientId} spawned!");
    }
}