using System.Text;
using ScoreBridge.Server.SeedWork;
using Serilog;

namespace ScoreBridge.Server.Parsers.Bodet.Messages;

public class Message50 : IPacketFormatter
{
    public async Task FormatPacketAsync(byte[] bytes)
    {
        const string path = "C:\\Users\\Alex\\Documents\\ScoreBridgeOutput\\possession.txt";
        
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            await fs.WriteAsync(bytes[3..5]);
        }
    }
}