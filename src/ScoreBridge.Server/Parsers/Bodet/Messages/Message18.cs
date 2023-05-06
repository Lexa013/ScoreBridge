using System.Text;
using ScoreBridge.Server.SeedWork;
using Serilog;

namespace ScoreBridge.Server.Parsers.Bodet.Messages;

public class Message18 : IPacketFormatter
{
    public async Task FormatPacketAsync(byte[] bytes)
    {
        string timePath = BodetParser.OutputPath + "time.txt";
        string periodPath = BodetParser.OutputPath + "period.txt";

        bool inDecimal = bytes[6] == 0x44;
        string time;

        if (!inDecimal)
            time = "" + (char) bytes[4] + (char) bytes[5] + ":" + (char) bytes[6] + (char) bytes[7];
        else
            time =  "00:" + (char) bytes[4] + (char) bytes[5] + "." + (char) bytes[7];


        using (FileStream fs = new FileStream(timePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
        {
            await fs.WriteAsync(Encoding.ASCII.GetBytes(time)); ;
        }
        
        using (FileStream fs = new FileStream(periodPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
        {
            await fs.WriteAsync(new[] {bytes[12]}); ;
        }
    }
}