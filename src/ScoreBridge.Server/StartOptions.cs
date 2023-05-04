using CommandLine;

namespace ScoreBridge.Server;

public class StartOptions
{
    public enum ScoreboardTypeEnum
    {
        Bt6000,
        Scorepad 
    }
    
    [Option('i', "inputmode", Required = true, HelpText = "Input mode")]
    public ScoreboardTypeEnum ScoreboardType { get; set; }
}