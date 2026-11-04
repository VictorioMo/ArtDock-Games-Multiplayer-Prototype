public interface INetworkSessionService
{
    bool Host(ushort port);
    bool Join(string ipAddress, ushort port);
}