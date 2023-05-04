using Microsoft.Extensions.Configuration;
using ScoreBridge.Server.Listeners;
using ScoreBridge.Server.Scoreboards.Scorepad;
using ScoreBridge.Server.SeedWork;
using Serilog;

namespace ScoreBridge.Server;

public class ScoreBridgeService
{
    public StartOptions StartOptions { get; init; }  
    
    public IConfigurationRoot Configuration { get; init; }  
    
    public Scoreboard? CurrentScoreboard { get; set; }

    private bool _shouldStop = false;
    
    public ScoreBridgeService(StartOptions startOptions, IConfigurationRoot configuration)
    {
        StartOptions = startOptions;
        Configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (StartOptions.ScoreboardType == StartOptions.ScoreboardTypeEnum.Scorepad)
        {
            TcpListenerOptions options = GetOptionsFromConfiguration<TcpListenerOptions>("Listeners:Tcp");
            CurrentScoreboard = new ScorepadScoreboard(options);
        }
        else if (StartOptions.ScoreboardType == StartOptions.ScoreboardTypeEnum.Bt6000)
        {
            throw new NotImplementedException("BT 6000 will be implemented in a future update");
        }
        
        Log.Information("Current scoreboard is {@Scoreboard}", CurrentScoreboard.GetType().Name);

        if (CurrentScoreboard.Listener is null)
        {
            Log.Error("The scoreboard: @{Scoreboard} didn't initialized the Listener in it's constructor, Aborting !",
                CurrentScoreboard.GetType().Name);
            _shouldStop = true;
        }

        await CurrentScoreboard.Listener!.StartAsync();

        while (!cancellationToken.IsCancellationRequested ^ _shouldStop)
        {
            await Task.Delay(1000);
        }

        await StopAsync();
    }
    
    public async Task StopAsync()
    {
        await CurrentScoreboard?.Listener?.StopAsync()!;
    }

    public T GetOptionsFromConfiguration<T>(string section) where T : class
    {
        try
        {
            var instance = Activator.CreateInstance<T>();
            ConfigurationBinder.Bind(Configuration.GetSection(section), instance);
            
            return instance;
        }
        catch (InvalidOperationException e)
        {
            Log.Error("Failed to bind the class: {@Class} using the section: {@Section}", typeof(T), section);
            throw;
        }
    }
}