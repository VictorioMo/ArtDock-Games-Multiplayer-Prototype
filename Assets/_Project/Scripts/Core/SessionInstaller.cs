using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class SessionInstaller : MonoBehaviour
{
    [SerializeField]
    private NetworkManager networkManager;

    [SerializeField]
    private ConnectionUI connectionUI;

    private void Awake()
    {
        UnityTransport transport =
            networkManager.GetComponent<UnityTransport>();

        INetworkSessionService sessionService =
            new NgoNetworkSessionService(
                networkManager,
                transport
            );

        connectionUI.Initialize(sessionService);
    }
}