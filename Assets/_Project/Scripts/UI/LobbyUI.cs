using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Lobby")]
    [SerializeField] private TMP_Text playerListText;
    [SerializeField] private Button startGameButton;

    private ILobbyRoster _roster;
    private IGameSession _gameSession;

    public void Initialize(
        ILobbyRoster roster,
        IGameSession gameSession)
    {
        _roster = roster;
        _gameSession = gameSession;

        _roster.Changed += Refresh;
        _gameSession.Changed += Refresh;

        startGameButton.onClick.AddListener(OnStartGameClicked);

        Refresh();
    }

    private void Refresh()
    {
        bool connected =
            _roster != null && _roster.IsAvailable;

        bool loading =
            connected && _gameSession.IsLoading;

        bool playing =
            connected && _gameSession.IsGameplayActive;

        connectionPanel.SetActive(!connected);

        lobbyPanel.SetActive(
            connected && !loading && !playing
        );

        loadingPanel.SetActive(loading);

        if (!connected)
            return;

        startGameButton.gameObject.SetActive(
            _gameSession.CanStart
        );

        StringBuilder text = new StringBuilder();

        text.AppendLine(
            $"Connected Players: {_roster.PlayerCount}/4"
        );

        text.AppendLine();

        for (int i = 0; i < _roster.PlayerCount; i++)
        {
            text.AppendLine(_roster.GetPlayerName(i));
        }

        playerListText.text = text.ToString();
    }

    private void OnStartGameClicked()
    {
        _gameSession.StartGame();
    }

    private void OnDestroy()
    {
        if (_roster != null)
            _roster.Changed -= Refresh;

        if (_gameSession != null)
            _gameSession.Changed -= Refresh;

        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);
    }
}