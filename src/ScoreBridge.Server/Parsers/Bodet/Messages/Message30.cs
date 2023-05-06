using System.Text;
using ScoreBridge.Server.SeedWork;

namespace ScoreBridge.Server.Parsers.Bodet.Messages;

public class Message30 : IPacketFormatter
{
    public async Task FormatPacketAsync(byte[] bytes)
    {
        string homeScorePath = BodetParser.OutputPath + "homeScore.txt";
        string awayScorePath = BodetParser.OutputPath + "awayScore.txt";

        bool inDecimal = bytes[6] == 0x44;

        var homeScore = "" + (char) bytes[3] + (char) bytes[4] + (char) bytes[5];
        var awayScore = "" + (char) bytes[6] + (char) bytes[7] + (char) bytes[8];


        using (FileStream fs = new FileStream(homeScorePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
        {
            await fs.WriteAsync(Encoding.ASCII.GetBytes(homeScore.Trim())); ;
        }
        
        using (FileStream fs = new FileStream(awayScorePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
        {
            await fs.WriteAsync(Encoding.ASCII.GetBytes(awayScore.Trim())); ;
        }
    }
}