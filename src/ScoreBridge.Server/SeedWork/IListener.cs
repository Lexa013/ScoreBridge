namespace ScoreBridge.Server.SeedWork;

public interface IListener
{
    Task StartAsync();

    Task StopAsync();
}