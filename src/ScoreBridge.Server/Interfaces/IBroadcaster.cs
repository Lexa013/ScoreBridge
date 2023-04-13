using ScoreBridge.Server.Entities;

namespace ScoreBridge.Server.Interfaces;

public interface IBroadcaster
{
    void Setup();
    void Connect();
    
    void Disconnect();
    
    void Broadcast(string message);
}