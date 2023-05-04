namespace ScoreBridge.Server.Listeners;

// ReSharper disable once ClassNeverInstantiated.Global
public class TcpListenerOptions
{
    public string? WhitelistedAddress { get; set; }
    public int Port { get; set; }
}