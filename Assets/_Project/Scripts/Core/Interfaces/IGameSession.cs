using System;

public interface IGameSession
{
    event Action Changed;

    bool CanStart { get; }
    bool IsLoading { get; }
    bool IsGameplayActive { get; }

    void StartGame();
}