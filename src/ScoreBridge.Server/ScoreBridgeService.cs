﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScoreBridge.Server.Interfaces;

namespace ScoreBridge.Server;

public class ScoreConnectService : IHostedService
{
    private readonly ILogger<ScoreConnectService> _logger;
    private readonly IListener _listener;
    private readonly IBroadcaster _broadcaster; 

    public ScoreConnectService(IServiceProvider serviceProvider, ILogger<ScoreConnectService> logger, IListener listener, IBroadcaster broadcaster)
    {
        _logger = logger;
        _listener = listener;
        _broadcaster = broadcaster;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting service, listening to {@Type}", _listener.GetType().Name);
        
        // Initialize and start the broadcaster
        _broadcaster.Setup();
        _broadcaster.Start();
        
        // Initialize and start the listener
        _listener.Setup();
        _listener.Start();
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _listener.Stop();
        _broadcaster.Stop();
        
        return Task.CompletedTask;
    }
}