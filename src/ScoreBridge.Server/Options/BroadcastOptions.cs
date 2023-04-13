namespace ScoreBridge.Server.Options;

public class BroadcastOptions
{
    public string Address { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 1868;

    public int HeartbeatDelay { get; set; } = 10;
    public string Password { get; set; } = "pass";
}