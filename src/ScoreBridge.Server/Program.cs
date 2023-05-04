using CommandLine;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ScoreBridge.Server;

class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        StartOptions? startOptions = null;
        
        Parser.Default.ParseArguments<StartOptions>(args).WithParsed(options =>
        {
            startOptions = options;
        }).WithNotParsed(errors =>
        {
            startOptions = null;
        });
        
        if (startOptions is null)
            return;

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        ScoreBridgeService bridgeService = new ScoreBridgeService(startOptions, config);

        Console.CancelKeyPress += async (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            tokenSource.Cancel();
        };

        await bridgeService.StartAsync(tokenSource.Token);
    }
}