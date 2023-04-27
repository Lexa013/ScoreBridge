using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScoreBridge.Server.Interfaces;
using ScoreBridge.Server.Options.Scoreboards;

namespace ScoreBridge.Server.Listeners;

public class ScorepadListener : IListener
{
    private readonly ILogger<ScorepadListener> _logger;
    private readonly IOptions<ScorepadOptions> _options;
    
    private TcpListener _listener;
    private CancellationTokenSource _tokenSource;
    private Task? _clientHandlingTask;

    public ScorepadListener(ILogger<ScorepadListener> logger, IOptions<ScorepadOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public void Setup()
    {
        _listener = new TcpListener(IPAddress.Any, _options.Value.Port);
        _tokenSource = new CancellationTokenSource();
    }
    
    public async Task Start()
    {
        _listener.Start();
        
        _logger.LogInformation("Started listening on {@Address}", _listener.LocalEndpoint);
        
        while (!_tokenSource.IsCancellationRequested)
        {
            TcpClient client;
                
            try
            {
                client = await _listener.AcceptTcpClientAsync();
            }
            catch (Exception e)
            {
                continue;
            }
                
            _logger.LogInformation("Client connected from {@Endpoint}", client.Client.RemoteEndPoint);

            if (_clientHandlingTask is not null)
            {
                _logger.LogInformation("Client reconnected, recreating the task !");
                _clientHandlingTask.Dispose();
            }
            
            _clientHandlingTask = Task.Factory.StartNew(() =>
            {
                HandleClient(client);
            });
        }
    }

    public async Task HandleClient(TcpClient tcpClient)
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];
        int attackTime = 0;
        
        while (true)
        {
            int dataLenght;

            try
            {
                dataLenght = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                _logger.LogError("{@Exception}", e);
                throw;
            }
                
            string message = Encoding.ASCII.GetString(buffer, 0, dataLenght);

            if (message != "")
            {
                /*
                _logger.LogInformation("RECEIVED MESSAGE: {@Message}", message);
                */

                if (message.StartsWith("G50"))
                {
                    attackTime = int.Parse("" + message[7] + message[8]);
                }

                if (message.StartsWith("G18"))
                {
                    _logger.LogInformation("New stats received\n" +
                                           "Time: " + message[8] + message[9] + ":" + message[10] + message[11] + "\n" +
                                           "Période: " + message[16] + "\n" +
                                           "Possesion: " + (attackTime == 0 ? "Aucun" : attackTime) + "\n" +
                                           "Temps-morts Locaux: " + message[12] + "\n" +
                                           "Temps-morts Visiteurs: " + message[13] + "\n");
                }
            }
        }
    }
    
    public void Stop()
    {
        try
        {
            _listener.Stop();
        }
        catch (Exception e)
        {
            _logger.LogError("{@Exception}", e);
            throw;
        }
    }
}