using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionExitHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    private NetworkManager networkManager;

    private const string BootstrapScene = "Bootstrap";

    private static string _pendingMessage;
    private bool _returningToMenu;

    private void Awake()
    {
        networkManager = NetworkManager.Singleton;

        // Display a message carried over from the previous session.
        if (statusText != null)
            statusText.text = _pendingMessage ?? string.Empty;

        _pendingMessage = null;
    }

    private void OnEnable()
    {
        networkManager.OnClientStopped += HandleClientStopped;
    }

    private void OnDisable()
    {
        if (networkManager != null)
            networkManager.OnClientStopped -= HandleClientStopped;
    }

    private void HandleClientStopped(bool wasHost)
    {
        if (_returningToMenu)
            return;

        _returningToMenu = true;

        _pendingMessage = wasHost
            ? "Session closed."
            : "Disconnected from host.";

        // Start again with fresh local session state.
        SceneManager.LoadScene(
            BootstrapScene,
            LoadSceneMode.Single
        );
    }
}