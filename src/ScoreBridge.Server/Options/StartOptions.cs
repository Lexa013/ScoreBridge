using CommandLine;

namespace ScoreBridge.Server.Options;

public class StartOptions
{
    public enum InputModeEnum
    {
        Bodet,
        Scorepad 
    }
    
    [Option('i', "inputmode", Required = true, HelpText = "Input mode")]
    public InputModeEnum InputMode { get; set; }
}