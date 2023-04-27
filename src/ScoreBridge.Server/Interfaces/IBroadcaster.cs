using ScoreBridge.Server.Entities;

namespace ScoreBridge.Server.Interfaces;

public interface IBroadcaster
{
    void Setup();
    Task Start();
    
    void Stop();
    
    Task Broadcast(byte[] message);
}