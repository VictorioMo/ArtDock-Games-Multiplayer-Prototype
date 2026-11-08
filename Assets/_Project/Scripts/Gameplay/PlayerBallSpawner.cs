using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBallSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject ballPrefab;

    private PlayerMovement _movement;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner || !_movement.IsGameplayActive)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.bKey.wasPressedThisFrame)
        {
            SpawnBallRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnBallRpc()
    {
        if (!IsServer || ballPrefab == null)
            return;

        Vector3 spawnPosition =
            transform.position +
            transform.forward * 2f +
            Vector3.up * 1.5f;

        NetworkObject ball = Instantiate(
            ballPrefab,
            spawnPosition,
            Quaternion.identity
        );

        ball.Spawn();
    }
}