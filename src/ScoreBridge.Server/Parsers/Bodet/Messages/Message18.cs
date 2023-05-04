using System.Text;
using ScoreBridge.Server.SeedWork;
using Serilog;

namespace ScoreBridge.Server.Parsers.Bodet.Messages;

public class Message18 : IPacketFormatter
{
    public Task FormatPacketAsync(byte[] bytes)
    {
        Log.Information("Message 18: {@Message}", Encoding.ASCII.GetString(bytes));

        return Task.CompletedTask;
    }
}