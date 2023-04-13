using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScoreBridge.Server.Entities;
using ScoreBridge.Server.Interfaces;
using ScoreBridge.Server.Options;

namespace ScoreBridge.Server.Broadcasters;

public class EthernetBroadcaster : IBroadcaster
{
    private List<Client> _clients;

    private byte[] _bufferRecv;
    private ArraySegment<byte> _bufferRecvSegment;
    private Socket _socket;
    private EndPoint _ep;

    private Task _listenerTask;
    private Task _heartsensorTask;

    private readonly IOptions<BroadcastOptions> _options;
    private readonly ILogger<EthernetBroadcaster> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

#pragma warning disable CS8618
    public EthernetBroadcaster(IOptions<BroadcastOptions> options, ILogger<EthernetBroadcaster> logger, IHostApplicationLifetime applicationLifetime)
#pragma warning restore CS8618
    {
        _options = options;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    public void Setup()
    {
        // Buffer initialization
        _bufferRecv = new byte[1024];
        _bufferRecvSegment = new(_bufferRecv);
        
        _clients = new List<Client>();
        
        _ep = new IPEndPoint(IPAddress.Any, _options.Value.Port);
        _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Tcp);

        try
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            const int SIO_UDP_CONNRESET = -1744830452;
            _socket.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, new byte[] { 0 });
            _socket.Bind(_ep);
        }
        catch (Exception e)
        {
            if (e is SocketException socketException)
                _logger.LogCritical("Something went wrong when starting the socket: {@ErrorCode}",
                    ((SocketError)socketException.ErrorCode).ToString());
            else
                _logger.LogCritical("Something went wrong when starting the socket: {@Exception}", e);

            _applicationLifetime.StopApplication();
        }
    }

    public void Connect()
    {
        // Start listening to connection

        _listenerTask = Task.Factory.StartNew(async () =>
        {
            SocketReceiveMessageFromResult res;
            while (true)
            {
                try
                {
                    res = await _socket.ReceiveMessageFromAsync(_bufferRecvSegment, SocketFlags.None, _ep);
                }
                catch (Exception e)
                {
                    _logger.LogInformation("Exception: {@Error}", e);
                    continue;
                }
                
                string text = Encoding.ASCII.GetString(_bufferRecv, 0, res.ReceivedBytes);
                OnUdpDataReceived(res.RemoteEndPoint, text);
            }
        }, TaskCreationOptions.LongRunning);
        
        HandleHeartBeat();
    }


    /// <summary>
    /// Disconnect the broadcaster
    /// </summary>
    public void Disconnect()
    {
        try
        {
            _socket.Close();
            _clients.Clear();
            
            _listenerTask.Dispose();
            _heartsensorTask.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error stopping EthernetBroadcaster: {@Exception}", ex);
            _applicationLifetime.StopApplication();
        }
    }

    public void HandleHeartBeat()
    {
        _heartsensorTask = Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                _logger.LogInformation("handshaker");
                foreach (var cl in _clients)
                {
                    if (cl.LastMessage.Add(TimeSpan.FromSeconds(_options.Value.HeartbeatDelay)) <=
                        DateTime.Now.TimeOfDay)
                    {
                        _logger.LogInformation("Heartbeat dead for {@Client}", cl.Ip);
                        _clients.Remove(cl);
                        _socket.SendTo(Encoding.ASCII.GetBytes("TIMEOUT"), SocketFlags.None, cl.EndPoint);
                        _logger.LogInformation("timeout msg sent");
                    }
                }

                await Task.Delay(1000);
            }
        }, TaskCreationOptions.LongRunning);
    }

    private void OnUdpDataReceived(EndPoint client, string text)
    {
        var remoteEndPoint = (IPEndPoint) client;
        
        
        // If the client already exists it will acknowledge the heartbeat otherwise it will check for the password
        if (ClientExists(remoteEndPoint, out Client? foundClient))
        {
            _logger.LogInformation("already exists");
            
            // Reset the heartbeat of the client
            foundClient.LastMessage = DateTime.Now.TimeOfDay;
        }
        else // Check for password
        {
            if (!string.IsNullOrEmpty(_options.Value.Password))
            {
                var correctPassword = _options.Value.Password.Trim();

                if (text == correctPassword)
                {
                    _logger.LogInformation("Valid password from @{Address}:{@Port}", remoteEndPoint.Address.ToString(),
                        remoteEndPoint.Port);

                    Client newClient = new Client()
                    {
                        EndPoint = remoteEndPoint,
                        LastMessage = DateTime.Now.TimeOfDay
                    };
                    
                    _clients.Add(newClient);
                    _socket.SendTo(Encoding.ASCII.GetBytes("SUCCESS"), SocketFlags.None, remoteEndPoint);
                }
                else
                {
                    _logger.LogInformation("Bad password from {@Address}:{@Port}", remoteEndPoint.Address.ToString(),
                        remoteEndPoint.Port);
                    _socket.SendTo(Encoding.ASCII.GetBytes("BADARG"), SocketFlags.None, remoteEndPoint);
                }
            }
        }

        _logger.LogInformation("Total clients: {@ClientCount}", _clients.Count);
    }

    /// <summary>
    /// Broadcast data all across connected clients
    /// </summary>
    /// <param name="data">The data that will be sent to all connected clients</param>
    public void Broadcast(string data)
    {
        foreach (var client in _clients)
        {
            try
            {
                _socket.SendTo(Encoding.ASCII.GetBytes(data), SocketFlags.None, client.EndPoint);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to send a detagram: {@Error}", e);
            }
        }
    }

    /// <summary>
    /// Checks if a client exists
    /// </summary>
    /// <param name="endpoint">The endpoint to compare</param>
    /// <param name="foundClient">The client that was found, equals to null if not found, refer to return value</param>
    /// <returns>Wether client has been found or not</returns>
    private bool ClientExists(IPEndPoint endpoint, out Client foundClient)
    {
        Client? client = _clients.FirstOrDefault(c => c.Ip == $"{endpoint.Address.ToString()}:{endpoint.Port}");

        if (client is null)
        {
            // Never use the default value
            foundClient = null!;
            return false;
        }

        foundClient = client;
        return true;
    }

    
}