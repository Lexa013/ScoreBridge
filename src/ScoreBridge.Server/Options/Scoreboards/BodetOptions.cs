using System.IO.Ports;

namespace ScoreBridge.Server.Options.Scoreboards;

public class BodetOptions
{
    public string PortName { get; set; } = "COM1";
    public int BaudRate { get; set; } = 9600;
    public Parity Parity { get; set; } = Parity.None;
    public StopBits StopBits { get; set; }
    public string TestFile { get; set; } = "";
}