using System.Net;

namespace ScoreBridge.Server.Entities;

public class Client
{
    public IPEndPoint EndPoint;
    public TimeSpan LastMessage;

    public string Ip
        => $"{EndPoint.Address.ToString()}:{EndPoint.Port}";
}