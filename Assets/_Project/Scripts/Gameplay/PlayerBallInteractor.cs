using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBallInteractor : NetworkBehaviour
{
    [SerializeField] private float grabRange = 3f;

    private Camera _playerCamera;
    private PlayerMovement _playerMovement;

    // Server-side reference to the currently held ball.
    private NetworkBall _heldBall;

    private void Awake()
    {
        _playerCamera = GetComponentInChildren<Camera>(true);
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner || !_playerMovement.IsGameplayActive)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        // E: Grab or drop.
        if (keyboard.eKey.wasPressedThisFrame)
        {
            ulong targetId = ulong.MaxValue;

            if (Physics.Raycast(
                _playerCamera.transform.position,
                _playerCamera.transform.forward,
                out RaycastHit hit,
                grabRange,
                ~0,
                QueryTriggerInteraction.Ignore))
            {
                if (hit.rigidbody != null &&
                    hit.rigidbody.TryGetComponent(
                        out NetworkBall ball) &&
                    ball.IsSpawned)
                {
                    targetId = ball.NetworkObjectId;
                }
            }

            ToggleGrabRpc(targetId);
        }

        // F: Throw.
        if (keyboard.fKey.wasPressedThisFrame)
        {
            ThrowHeldRpc();
        }

        // X: Delete held ball.
        if (keyboard.xKey.wasPressedThisFrame)
        {
            DeleteHeldRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void ToggleGrabRpc(
        ulong objectId,
        RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        // Already holding a ball? Drop it.
        if (_heldBall != null)
        {
            _heldBall.TryRelease(OwnerClientId, false);
            _heldBall = null;
            return;
        }

        // No valid target.
        if (objectId == ulong.MaxValue)
            return;

        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
            objectId, out NetworkObject target))
            return;

        if (!target.TryGetComponent(out NetworkBall ball))
            return;

        // Host validates distance.
        float distance = Vector3.Distance(
            transform.position,
            ball.transform.position
        );

        if (distance > grabRange + 0.5f)
            return;

        if (!ball.TryGrab(OwnerClientId))
            return;

        _heldBall = ball;
    }

    [Rpc(SendTo.Server)]
    private void ThrowHeldRpc(RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (_heldBall == null)
            return;

        _heldBall.TryRelease(OwnerClientId, true);
        _heldBall = null;
    }

    [Rpc(SendTo.Server)]
    private void DeleteHeldRpc(RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (_heldBall == null)
            return;

        if (_heldBall.HolderId != OwnerClientId)
            return;

        NetworkBall ball = _heldBall;
        _heldBall = null;

        ball.NetworkObject.Despawn(true);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;

        if (_heldBall != null)
        {
            _heldBall.TryRelease(OwnerClientId, false);
            _heldBall = null;
        }
    }
}