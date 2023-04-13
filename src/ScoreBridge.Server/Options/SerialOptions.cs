using System.IO.Ports;

namespace ScoreBridge.Server.Options;

public class SerialOptions
{
    public string PortName { get; set; } = "COM1";
    public int BaudRate { get; set; } = 9600;
    public Parity Parity { get; set; } = Parity.None;
    public StopBits StopBits { get; set; }
}