namespace ScoreBridge.Server.Interfaces;

public interface IListener
{
    void Setup();

    Task Start();

    void Stop();
}