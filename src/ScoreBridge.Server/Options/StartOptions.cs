using CommandLine;

namespace ScoreBridge.Server.Options;

public class StartOptions
{
    public enum InputModeEnum
    {
        Serial,
        Ethernet 
    }
    
    [Option('i', "inputmode", Required = true, HelpText = "Input mode")]
    public InputModeEnum InputMode { get; set; }
}