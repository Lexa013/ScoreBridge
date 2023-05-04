namespace ScoreBridge.Server.SeedWork;

public interface IParser
{
    Task ParseAsync(byte[] data);
}