namespace ScoreBridge.Server.SeedWork;

public interface Scoreboard
{
    public abstract IListener? Listener { get; set; }
    
    public abstract IParser? Parser { get; set; }
}