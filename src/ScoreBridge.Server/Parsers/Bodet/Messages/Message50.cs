using System.Text;
using ScoreBridge.Server.SeedWork;
using Serilog;

namespace ScoreBridge.Server.Parsers.Bodet.Messages;

public class Message50 : IPacketFormatter
{
    public async Task FormatPacketAsync(byte[] bytes)
    {
        using (FileStream fs = File.Create("/Users/vanstaen/Desktop/output/poss.txt"))
        {
            await fs.WriteAsync(bytes[3..5]);
            Log.Information(Encoding.ASCII.GetString(bytes[3..5]));
        }
    }
}