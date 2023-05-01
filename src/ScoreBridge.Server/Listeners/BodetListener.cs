using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScoreBridge.Server.Interfaces;
using ScoreBridge.Server.Options;
using ScoreBridge.Server.Options.Scoreboards;

namespace ScoreBridge.Server.Listeners;

public class BodetListener : IListener
{
    private readonly IOptions<BodetOptions> _options;
    private readonly ILogger<BodetListener> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IBroadcaster _broadcaster;

    private CancellationTokenSource _tokenSource = null!;
    private SerialPort _serialPort = null!;

    public BodetListener(IOptions<BodetOptions> options, ILogger<BodetListener> logger, IHostApplicationLifetime applicationLifetime, IBroadcaster broadcaster)
    {
        _options = options;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _broadcaster = broadcaster;
        
    }

    public void Setup()
    {
        _tokenSource = new CancellationTokenSource();
        
        _serialPort = new SerialPort()
        {
            PortName = _options.Value.PortName,
            BaudRate = _options.Value.BaudRate,
            Parity = _options.Value.Parity,
            StopBits = _options.Value.StopBits
        };
    }

    public async Task Start()
    {
        _logger.LogInformation("Opening serial port on {@PortIdentifier}", _options.Value.PortName);

        try
        {
            _serialPort.Open();
        }
        catch (Exception e)
        {
            if (e is FileNotFoundException notFoundException)
            {
                _logger.LogError("Cannot find the serial port '{@PortName}", notFoundException.FileName);
                _applicationLifetime.StopApplication();
            }
                
        }

        Task.Factory.StartNew( async () =>
        {
            while (true)
            {
                using (StreamReader streamReader = new StreamReader(_options.Value.TestFile))
                {
                    // Note: As the scorepad sends us Ascii char we can use bytes instead of chars
                    byte[] buffer = new byte[5];
                    
                    // index represents the next position available in the buffer
                    int index = 0;
                        
                    // whether the buffer contains a start or not
                    bool hasStart = false;
                    
                    while (streamReader.Peek() >= 0)
                    {
                        byte byteValue = (byte) streamReader.Read();
                        
                        buffer[index] = byteValue;
                        index++;

                        if (index > buffer.Length - 1)
                        {
                            _broadcaster.Broadcast(buffer);
                            index = 0;
                            continue;
                        }
                        
//                         // If received char is STX
//                         if ((char) byteValue == '\x02')
//                         {
//                             index = 0;
//                             buffer[index] = byteValue;
//                             index++;
//                             
//                             hasStart = true;
//                             continue;
//                         }
//
//                         // If received char is ETX
//                         if ((char) byteValue == '\x03' && hasStart)
//                         {
//                             buffer[index] = byteValue;
// #pragma warning disable CS4014
//                             _broadcaster.Broadcast(buffer);
// #pragma warning restore CS4014
//                             
//                             index = 0;
//                             continue;
//                         }
//
//                         // Prevent buffer from overflowing
//                         if (index + 1 > buffer.Length)
//                         {
//                             _logger.LogWarning("Buffer had overflown, emptying it !");
//                             index = 0;
//                             continue;
//                         }
//
//                         buffer[index] = byteValue;
//                         index++;
                    }
                }
            }
        },_tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void Stop()
    {
        try
        {
            _serialPort.Close();
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException operationCanceledException)
            {
                _logger.LogError("Failed to close serial port '{@PortName}", _serialPort.PortName);
                _applicationLifetime.StopApplication();
            }
        }
    }
}