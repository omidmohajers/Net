namespace PA.Net.Core.Clients
{
    public enum ClientState
    {
        Connected,
        Disconnected,
        Connecting,
        Disconnecting,
        Error,
        Retry,
        Sending,
        Idle,
        DataReceive,
    }
}
