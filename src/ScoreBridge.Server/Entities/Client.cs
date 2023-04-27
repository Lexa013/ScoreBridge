using System.Net;
using System.Net.Sockets;

namespace ScoreBridge.Server.Entities;

public class Client
{
    public TcpClient TcpClient { get; set; }

    public bool Authed { get; set; } = false;

    public bool Stream { get; set; } = false;

    public Client(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
    }
    
    public string GetIpAndPort()
    {
        if (TcpClient.Client.RemoteEndPoint is null)
            return "Invalid endpoint";
        
        IPEndPoint ipEndpoint = (IPEndPoint)TcpClient.Client.RemoteEndPoint;

        return $"{ipEndpoint.Address}:{ipEndpoint.Port}";
    }
}