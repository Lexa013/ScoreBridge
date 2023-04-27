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

    public class TcpBroadcaster : IBroadcaster
    {
        private List<Client> _clients;
        
        private TcpListener _listener;
        private CancellationTokenSource _tokenSource;

        private readonly IOptions<BroadcastOptions> _options;
        private readonly ILogger<TcpBroadcaster> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;

    #pragma warning disable CS8618
        public TcpBroadcaster(IOptions<BroadcastOptions> options, ILogger<TcpBroadcaster> logger, IHostApplicationLifetime applicationLifetime)
    #pragma warning restore CS8618
        {
            _options = options;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
        }

        public void Setup()
        {
            _listener = new TcpListener(IPAddress.Any, _options.Value.Port);

            _tokenSource = new CancellationTokenSource();

            _clients = new();
        }

        public async Task Start()
        {
            _listener.Start();
            
            while (!_tokenSource.IsCancellationRequested)
            {
                TcpClient client;
                
                try
                {
                    client = await _listener.AcceptTcpClientAsync();
                    client.NoDelay = true;
                }
                catch (Exception e)
                {
                    continue;
                }
                
                _logger.LogInformation("Client connected from {@Endpoint}", client.Client.RemoteEndPoint);

                Task.Run(() =>
                {
                    HandleClient(client);
                }, _tokenSource.Token);
                
                _logger.LogInformation("waiting for for other client");
            }
        }

        public async Task HandleClient(TcpClient tcpClient)
        {
            NetworkStream stream = tcpClient.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                Client? client = GetClientFromTcpClient(tcpClient);
                
                // Check if client exists
                if (client is null)
                {
                    var newClient = new Client(tcpClient);
                    _clients.Add(newClient);
                    client = newClient;
                }

                // Check if client is authed
                if (!client.Authed)
                {
                    await AuthClientAsync(client);
                }

                int dataLenght;
                try
                {
                    dataLenght = await stream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch (IOException e)
                {
                    KickClient(client);
                    break;
                }

                // If data == 0 it means the user disconnected
                if (dataLenght == 0)
                {
                    KickClient(client);
                    break;
                }
                
                string message = Encoding.ASCII.GetString(buffer, 0, dataLenght);

                if (message.ToLower() == "start")
                {
                    client.Stream = true;
                    _logger.LogInformation("{@Client} want the data", client.GetIpAndPort());
                }
                else if (message.ToLower() == "stop")
                {
                    client.Stream = false;
                    _logger.LogInformation("{@Client} don't the data", client.GetIpAndPort());
                }

                _logger.LogInformation("Client sent: {@Message}", message);
            }
        }

        /// <summary>
        /// Disconnect the broadcaster
        /// </summary>
        public void Stop()
        {
            try
            {
                _listener.Stop();
                _tokenSource.Cancel();
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Error stopping EthernetBroadcaster: {@Exception}", ex);
                _applicationLifetime.StopApplication();
            }
        }

        /// <summary>
        /// Broadcast data all across connected clients
        /// </summary>
        /// <param name="bytes">The data that will be sent to all connected clients</param>
        public async Task Broadcast(ReadOnlySpan<byte> bytes)
        { 
            _logger.LogInformation(Encoding.ASCII.GetString());

            foreach (var client in _clients)
            {
                if(!client.Authed) continue;
                if(!client.Stream) continue;
                
                try
                {
                    NetworkStream stream = client.TcpClient.GetStream();
                    await stream.Wr(bytes); }
                catch (IOException)
                {
                    KickClient(client);
                }
            }
        }

        /// <summary>
        /// Gets the Client from a TcpClient
        /// </summary>
        /// <param name="tcpClient">The TcpClient to find</param>
        /// <returns>Returns a <see cref="Client"/> if a client has been found, otherwise it returns null</returns>
        private Client? GetClientFromTcpClient(TcpClient tcpClient)
        {
            return _clients.FirstOrDefault(c => c.TcpClient == tcpClient);
        }

        private void KickClient(Client client)
        {
            client.TcpClient.Close();
            _clients.Remove(client);
            
            _logger.LogInformation("{@Endpoint} has been kicked !", client.GetIpAndPort());
        }

        private async Task AuthClientAsync(Client client)
        {
            NetworkStream stream = client.TcpClient.GetStream();
            byte[] buffer = new byte[1024];
            
            // If password invalid, auth the client without verifications
            if (string.IsNullOrEmpty(_options.Value.Password))
            {
                client.Authed = true;
                return;
            }
            
            // Read password
            int responseLenght;
            try
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes("PASSWORD"));
                responseLenght = await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                KickClient(client);
                return;
            }

            string response = Encoding.ASCII.GetString(buffer, 0, responseLenght);

            if (response == _options.Value.Password)
            {
                client.Authed = true;
                try
                {
                    await stream.WriteAsync(Encoding.ASCII.GetBytes("SUCCESS"));
                }
                catch (IOException)
                {
                    KickClient(client);
                }
            }
            else
            {
                try
                {
                    await stream.WriteAsync(Encoding.ASCII.GetBytes("BAD_PASSWORD"));
                } catch(IOException) { }
                
                KickClient(client);
            }
        }
    }