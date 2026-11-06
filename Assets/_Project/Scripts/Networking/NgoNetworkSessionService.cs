using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NgoNetworkSessionService : INetworkSessionService
{
    private readonly NetworkManager _networkManager;
    private readonly UnityTransport _transport;

    public NgoNetworkSessionService(
        NetworkManager networkManager,
        UnityTransport transport)
    {
        _networkManager = networkManager;
        _transport = transport;
    }

    public bool Host(ushort port)
    {
        if (_networkManager.IsListening)
            return false;

        _transport.SetConnectionData(
            "127.0.0.1",
            port,
            "0.0.0.0"
        );

        bool started = _networkManager.StartHost();

        if (started)
        {
            _networkManager.SceneManager.SetClientSynchronizationMode(
                UnityEngine.SceneManagement.LoadSceneMode.Additive
            );
        }

        return started;
    }

    public bool Join(string ipAddress, ushort port)
    {
        if (_networkManager.IsListening)
            return false;

        _transport.SetConnectionData(ipAddress, port);

        return _networkManager.StartClient();
    }
}