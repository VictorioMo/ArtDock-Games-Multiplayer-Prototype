using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSessionController : NetworkBehaviour, IGameSession
{
    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Bootstrap")]
    [SerializeField] private Camera lobbyCamera;
    [SerializeField] private GameObject lobbyGround;

    private readonly NetworkVariable<bool> _gameReady = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool _loading;
    private bool _localSceneReady;
    private bool _gameplayActive;

    public event Action Changed;

    public bool CanStart =>
        IsSpawned &&
        IsServer &&
        !_loading &&
        !_gameReady.Value;

    public bool IsLoading => _loading;

    public bool IsGameplayActive => _gameplayActive;

    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.OnSceneEvent += HandleSceneEvent;

        NetworkManager.OnClientConnectedCallback +=
            HandleClientConnected;

        _gameReady.OnValueChanged += HandleGameReadyChanged;

        Changed?.Invoke();
    }

    public void StartGame()
    {
        // Only the host can start gameplay.
        if (!CanStart)
            return;

        SetLoading(true);

        SceneEventProgressStatus status =
            NetworkManager.SceneManager.LoadScene(
                gameplaySceneName,
                LoadSceneMode.Additive
            );

        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogError($"Scene loading failed: {status}");

            SetLoading(false);
        }
    }

    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneName != gameplaySceneName)
            return;

        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.Load:
                SetLoading(true);
                break;

            case SceneEventType.LoadEventCompleted:
                HandleLoadingCompleted();
                break;
        }
    }

    private void HandleLoadingCompleted()
    {
        Debug.Log("All participating clients finished loading!");

        _localSceneReady = true;

        // Only the host decides when gameplay is ready.
        if (IsServer)
        {
            _gameReady.Value = true;
        }

        TryActivateGameplay();
    }

    private void HandleGameReadyChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            TryActivateGameplay();
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // Handles the local client finishing initial synchronization,
        // including joining an already running session.
        if (clientId != NetworkManager.LocalClientId)
            return;

        if (!_gameReady.Value)
            return;

        Scene scene = SceneManager.GetSceneByName(gameplaySceneName);

        if (scene.IsValid() && scene.isLoaded)
        {
            _localSceneReady = true;
            TryActivateGameplay();
        }
    }

    private void TryActivateGameplay()
    {
        if (_gameplayActive || !_gameReady.Value || !_localSceneReady)
            return;

        Scene scene = SceneManager.GetSceneByName(gameplaySceneName);

        if (!scene.IsValid() || !scene.isLoaded)
            return;

        _gameplayActive = true;

        // Disable bootstrap visuals.
        if (lobbyCamera != null)
            lobbyCamera.gameObject.SetActive(false);

        if (lobbyGround != null)
            lobbyGround.SetActive(false);

        // Activate gameplay locally for every networked player.
        PlayerMovement[] players =
            FindObjectsByType<PlayerMovement>(
                FindObjectsSortMode.None
            );

        foreach (PlayerMovement player in players)
        {
            player.ActivateGameplay();
        }

        SetLoading(false);

        Debug.Log("Gameplay activated!");
    }

    private void SetLoading(bool loading)
    {
        _loading = loading;
        Changed?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.SceneManager != null)
        {
            NetworkManager.SceneManager.OnSceneEvent -=
                HandleSceneEvent;
        }

        NetworkManager.OnClientConnectedCallback -=
            HandleClientConnected;

        _gameReady.OnValueChanged -= HandleGameReadyChanged;

        _loading = false;
        _localSceneReady = false;
        _gameplayActive = false;

        Changed?.Invoke();
    }
}