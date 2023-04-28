using System.Text;
using ScoreBridge.Server;
using ScoreBridge.Server.Broadcasters;
using ScoreBridge.Server.Interfaces;
using ScoreBridge.Server.Listeners;
using ScoreBridge.Server.Options;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScoreBridge.Server.Options.Scoreboards;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json", false, true))

    .ConfigureServices((host, services) =>
    {

        services.Configure<BroadcastOptions>(host.Configuration.GetSection("Broadcast"));

        services.AddSingleton<IBroadcaster, TcpBroadcaster>();
        services.AddHostedService<ScoreConnectService>();
    })
    
    .ConfigureLogging(builder => builder.ClearProviders())
    
    .UseSerilog((context, configuration) =>
    {
        configuration.Enrich.FromLogContext().WriteTo.Console();
    });


Parser.Default.ParseArguments<StartOptions>(args).WithParsed(options =>
{
    if (options.InputMode == StartOptions.InputModeEnum.Scorepad)
    {
        host.ConfigureServices((context, collection) =>
        {
            collection.AddSingleton<IListener, ScorepadListener>();
            collection.Configure<ScorepadOptions>(
                context.Configuration.
                    GetSection("Scoreboards")
                    .GetSection("Scorepad"));
        });
    }
    else if (options.InputMode == StartOptions.InputModeEnum.Bodet)
    {
        host.ConfigureServices((context, collection) =>
        {
            collection.AddSingleton<IListener, BodetListener>();
            collection.Configure<BodetOptions>(
                context.Configuration
                    .GetSection("Scoreboards")
                    .GetSection("Bodet"));
        });
    }
}).WithNotParsed(errors =>
{
    Environment.Exit(0);
});

Console.OutputEncoding = Encoding.ASCII;

await host.Build().RunAsync();