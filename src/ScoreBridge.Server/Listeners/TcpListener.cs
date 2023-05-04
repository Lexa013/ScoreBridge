using System.Net;
using System.Net.Sockets;
using System.Text;
using ScoreBridge.Server.SeedWork;
using Serilog;

namespace ScoreBridge.Server.Listeners;

public class TcpListener : IListener
{
    private readonly TcpListenerOptions _options;
    private readonly System.Net.Sockets.TcpListener _listener;

    private bool _shouldStop = false;
    
    private IParser Parser { get; init; }
    private Task? _clientHandlingTask;

    public TcpListener(TcpListenerOptions options, IParser parser)
    {
        _options = options;
        _listener = new System.Net.Sockets.TcpListener(IPAddress.Any, _options.Port);
        Parser = parser;
    }

    public Task StartAsync()
    {
        _listener.Start();
        Log.Information("Started {@Listener} listener", GetType().Name);

        _ = HandleConnectionAsync();
        
        return Task.CompletedTask;
    }

    private async Task HandleConnectionAsync()
    {
        while (true)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();

            if (client.Client.RemoteEndPoint is null)
            {
                client.Close();
                Log.Warning("Client remote endpoint is null, Skipping !");
                continue;
            }
            
            string clientAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            if (!string.IsNullOrEmpty(_options.WhitelistedAddress) && !_options.WhitelistedAddress.Equals(clientAddress, StringComparison.OrdinalIgnoreCase))
            {
                client.Close();
                Log.Information("Client {@IpAddress} tried to join but whitelist ip is {@WhitelistIp} !", clientAddress,
                    _options.WhitelistedAddress);
                continue;
            }

            // If the client handling tasks has been defined, it means the client is trying to reconnect
            if (_clientHandlingTask is not null)
            {
                Log.Information("Client reconnected, recreating the task !");
                _clientHandlingTask.Dispose();
            }

            _clientHandlingTask = Task.Run(() => HandleClientAsync(client));
            Log.Information("Client connected");
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        
        while (!_shouldStop)
        {
            int dataLenght;
            
            try
            {
                dataLenght = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (IOException e)
            {
                break;
            }
            
            string message = Encoding.ASCII.GetString(buffer, 0, dataLenght);
            await Parser.ParseAsync(buffer[..dataLenght]);
        }
    }

    public Task StopAsync()
    {
        _shouldStop = true;
        _listener.Stop();
        
        Log.Information("Stopped {@Listener} listener", GetType().Name);

        return Task.CompletedTask;
    }
}