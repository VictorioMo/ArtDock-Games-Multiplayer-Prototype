using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkBall : NetworkBehaviour
{
    public const ulong NoHolder = ulong.MaxValue;

    [Header("Holding")]
    [SerializeField] private float holdDistance = 1.7f;
    [SerializeField] private float holdHeight = 1.1f;

    [Header("Throwing")]
    [SerializeField] private float throwForce = 10f;

    private Rigidbody _rigidbody;

    private readonly NetworkVariable<ulong> _holderId = new(NoHolder);

    public bool IsHeld => _holderId.Value != NoHolder;
    public ulong HolderId => _holderId.Value;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            _rigidbody.isKinematic = true;
            return;
        }

        NetworkObject.DontDestroyWithOwner = true;

        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void FixedUpdate()
    {
        if (!IsSpawned || !IsServer || !IsHeld)
            return;

        // Find the player holding this ball.
        if (!NetworkManager.ConnectedClients.TryGetValue(
            HolderId, out NetworkClient client) ||
            client.PlayerObject == null)
        {
            TryRelease(HolderId, false);
            return;
        }

        Transform player = client.PlayerObject.transform;

        // Calculate holding position on the server.
        Vector3 targetPosition =
            player.position +
            player.forward * holdDistance +
            Vector3.up * holdHeight;

        _rigidbody.MovePosition(targetPosition);
    }

    public bool TryGrab(ulong clientId)
    {
        if (!IsServer || !IsSpawned || IsHeld)
            return false;

        if (!NetworkManager.ConnectedClients.TryGetValue(
            clientId, out NetworkClient client) ||
            client.PlayerObject == null)
            return false;

        _holderId.Value = clientId;

        // Stop physical simulation while held.
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.isKinematic = true;

        // Transfer ownership to the holder.
        if (NetworkObject.OwnerClientId != clientId)
            NetworkObject.ChangeOwnership(clientId);

        return true;
    }

    public bool TryRelease(ulong clientId, bool throwBall)
    {
        if (!IsServer || !IsSpawned || HolderId != clientId)
            return false;

        Vector3 throwDirection = transform.forward;

        if (NetworkManager.ConnectedClients.TryGetValue(
            clientId, out NetworkClient client) &&
            client.PlayerObject != null)
        {
            throwDirection =
                (client.PlayerObject.transform.forward +
                 Vector3.up * 0.3f).normalized;
        }

        _holderId.Value = NoHolder;

        // Return ownership to the host.
        if (NetworkObject.OwnerClientId != NetworkManager.ServerClientId)
            NetworkObject.RemoveOwnership();

        // Resume physics.
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        if (throwBall)
        {
            _rigidbody.AddForce(
                throwDirection * throwForce,
                ForceMode.Impulse
            );
        }

        return true;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer && HolderId == clientId)
            TryRelease(clientId, false);
    }
}