using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBallInteractor : NetworkBehaviour
{
    [SerializeField] private float grabRange = 3f;

    private Camera playerCamera;
    private PlayerMovement playerMovement;

    private NetworkBall _heldBall;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCamera = GetComponentInChildren<Camera>(true);
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner || !playerMovement.IsGameplayActive)
            return;

        if (Keyboard.current?.eKey.wasPressedThisFrame != true)
            return;

        if (Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out RaycastHit hit,
            grabRange))
        {
            if (hit.rigidbody != null &&
                hit.rigidbody.TryGetComponent(
                    out NetworkObject networkObject))
            {
                RequestGrabRpc(networkObject.NetworkObjectId);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestGrabRpc(ulong objectId, RpcParams rpcParams = default)
    {
        // Verify the sender owns this player.
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        // Player cannot grab another ball while holding one.
        if (_heldBall != null)
            return;

        // Find the requested network object.
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
            objectId, out NetworkObject target))
            return;

        // Accept only NetworkBall objects.
        if (!target.TryGetComponent(out NetworkBall ball))
            return;

        // Validate the distance on the host.
        float distance = Vector3.Distance(
            transform.position,
            ball.transform.position
        );

        if (distance > grabRange + 0.5f)
        {
            Debug.Log("Grab rejected: too far!");
            return;
        }

        // Attempt to grab the ball.
        if (!ball.TryGrab(OwnerClientId))
        {
            Debug.Log("Grab rejected: ball already held!");
            return;
        }

        // Remember which ball this player holds.
        _heldBall = ball;

        Debug.Log(
            $"Player {OwnerClientId} grabbed ball {objectId}!"
        );
    }
}