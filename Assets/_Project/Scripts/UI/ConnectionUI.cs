using TMPro;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private TMP_InputField portInput;

    private INetworkSessionService _sessionService;

    public void Initialize(INetworkSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public void Host()
    {
        if (!TryGetPort(out ushort port))
            return;

        bool started = _sessionService.Host(port);

        Debug.Log($"Host started: {started}");
    }

    public void Join()
    {
        if (!TryGetPort(out ushort port))
            return;

        bool started = _sessionService.Join(
            ipInput.text.Trim(),
            port
        );

        Debug.Log($"Client connection initiated: {started}");
    }

    private bool TryGetPort(out ushort port)
    {
        if (!ushort.TryParse(portInput.text, out port)
            || port == 0)
        {
            Debug.LogWarning("Invalid port!");
            return false;
        }

        return true;
    }
}