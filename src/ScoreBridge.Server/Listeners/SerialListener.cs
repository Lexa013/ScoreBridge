using System.IO.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScoreBridge.Server.Interfaces;
using ScoreBridge.Server.Options;

namespace ScoreBridge.Server.Listeners;

public class SerialListener : IListener
{
    private readonly IOptions<SerialOptions> _options;
    private readonly ILogger<SerialListener> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private IBroadcaster _broadcaster;

    private SerialPort _serialPort;

    public SerialListener(IOptions<SerialOptions> options, ILogger<SerialListener> logger, IHostApplicationLifetime applicationLifetime, IBroadcaster broadcaster)
    {
        _options = options;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _broadcaster = broadcaster;
    }

    public void Setup()
    {
        _serialPort = new SerialPort()
        {
            PortName = _options.Value.PortName,
            BaudRate = _options.Value.BaudRate,
            Parity = _options.Value.Parity,
            StopBits = _options.Value.StopBits
        };
    }

    public void Start()
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

        Task.Run(async () =>
        {
            while (true)
            {
                _broadcaster.Broadcast(DateTime.Now.ToString());
        
                await Task.Delay(500);
            }
        });
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