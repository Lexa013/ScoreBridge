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
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json", false, true))

    .ConfigureServices((host, services) =>
    {
        services.Configure<EthernetOptions>(host.Configuration.GetSection("Ethernet"));
        services.Configure<SerialOptions>(host.Configuration.GetSection("Serial"));
        services.Configure<BroadcastOptions>(host.Configuration.GetSection("Broadcast"));

        services.AddSingleton<IBroadcaster, EthernetBroadcaster>();
        services.AddHostedService<ScoreConnectService>();
    })
    
    .ConfigureLogging(builder => builder.ClearProviders())
    
    .UseSerilog((context, configuration) =>
    {
        configuration.Enrich.FromLogContext().WriteTo.Console();
    });


Parser.Default.ParseArguments<StartOptions>(args).WithParsed(options =>
{
    if (options.InputMode == StartOptions.InputModeEnum.Ethernet)
    {
        host.ConfigureServices(collection => collection.AddSingleton<IListener, EthernetListener>());
    }
    else if (options.InputMode == StartOptions.InputModeEnum.Serial)
    {
        host.ConfigureServices(collection => collection.AddSingleton<IListener, SerialListener>());
    }
}).WithNotParsed(errors =>
{
    Environment.Exit(0);
});


await host.Build().RunAsync();


// char[] buffer = new char[1024];
// List<string> trams = new();
//     
// int index = 0;
// bool hasStart = false;
//
// void ResetBuffer()
// {
//     buffer = new char[1024];
//     index = 0;
//     hasStart = false;
// }
//
// using (StreamReader streamReader = new StreamReader("E:\\Bureau\\serialOutput.txt"))
// {
//     while (streamReader.Peek() >= 0)
//     {
//         int charInt = streamReader.Read();
//         
//         buffer[index] = (char)charInt;
//         index++;
//         
//         
//         if ((char)charInt == '\x02')
//         {
//             hasStart = true;
//             index = 0;
//             continue;
//         }
//         
//         if ((char) charInt == '\x03' && hasStart)
//         {
//             string trame = new string(buffer, 0, index -1);
//             trams.Add(trame);
//             
//             ResetBuffer();
//             continue;
//         }
//         
//     }
//
//     Console.OutputEncoding = Encoding.ASCII;
//     foreach (var t in trams)
//     {
//         Console.WriteLine($"{t} - {t.Length}");
//     }