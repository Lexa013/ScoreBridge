namespace ScoreBridge.Server.SeedWork;

public interface IPacketFormatter
{
    Task FormatPacketAsync(byte[] bytes);
}