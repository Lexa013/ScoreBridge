namespace ScoreBridge.Server.Interfaces;

public interface IListener
{
    void Setup();

    void Start();

    void Stop();
}