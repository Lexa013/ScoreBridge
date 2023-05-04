using System.Text;
using ScoreBridge.Server.Parsers.Bodet.Messages;
using ScoreBridge.Server.SeedWork;
using Serilog;

namespace ScoreBridge.Server.Parsers;

public class BodetParser : IParser
{
    private byte[] _buffer;
    private int _index = 0;
    private bool _hasStart = false;

    public BodetParser()
    {
        _buffer = new byte[2048];
    }

    public Task ParseAsync(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            byte currentByte = data[i];
            _buffer[_index] = currentByte;

            if (currentByte == 0x02)
            {
                _index = 0;
                _hasStart = true;
                continue;
            }
            else if (currentByte == 0x03)
            {
                if (!_hasStart)
                    continue;
                // 1 because we don't want the G letter
                Parsed(_buffer[1.._index]);
                _index = 0;
                continue;
            }

            _index++;
        }

        return Task.CompletedTask;
    }

    public void Parsed(byte[] str)
    {
        string messageIdString = Encoding.ASCII.GetString(str[0..2]);
        IPacketFormatter? packetFormatter = ToPacketFormatter(messageIdString);

        if (packetFormatter is null)
            return;

        try
        {
            packetFormatter.FormatPacketAsync(str);
        }
        catch (Exception e)
        {
            if (e is NotImplementedException)
            {
                Log.Warning("The packet formatter {@PacketFormatter} is not implemented. Ignoring !",
                    packetFormatter.GetType().Name);
                return;
            }

            Log.Error("Failed to format packet using {@PacketFormatter}: \n{@Exception}",
                packetFormatter.GetType().Name, e); ;
        }
    }

    public IPacketFormatter? ToPacketFormatter(string messageId) => messageId switch
    {
        "18" => new Message18(),
        "50" => new Message50(),
        
        _ => null
    };

}
