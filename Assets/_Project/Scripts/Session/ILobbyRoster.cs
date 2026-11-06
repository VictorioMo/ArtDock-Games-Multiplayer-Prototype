using System;

public interface ILobbyRoster
{
    event Action Changed;

    bool IsAvailable { get; }
    int PlayerCount { get; }

    string GetPlayerName(int index);
}