using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class SessionInstaller : MonoBehaviour
{
    [SerializeField]
    private NetworkManager networkManager;

    [SerializeField]
    private ConnectionUI connectionUI;

    [SerializeField] private LobbyState lobbyState;
    [SerializeField] private LobbyUI lobbyUI;

    [SerializeField] private GameSessionController gameSession;

    private ConnectionApprovalService _approvalService;

    private void Awake()
    {
        UnityTransport transport =
            networkManager.GetComponent<UnityTransport>();

        // Networking service
        INetworkSessionService sessionService =
            new NgoNetworkSessionService(
                networkManager,
                transport
            );

        // Connection approval service
        _approvalService =
            new ConnectionApprovalService(networkManager);

        // Inject dependencies
        connectionUI.Initialize(sessionService);
        lobbyUI.Initialize(lobbyState, gameSession);
    }

    private void OnDestroy()
    {
        _approvalService?.Dispose();
    }
}