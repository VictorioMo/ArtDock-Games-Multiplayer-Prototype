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
            Random.Range(-2f, 2f),
            1.01f,
            Random.Range(-2f, 2f)
        );

        Debug.Log($"Player {OwnerClientId} spawned!");
    }
}