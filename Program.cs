using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PoisnCopy;

internal class Program
{
    private CancellationTokenSource _cts { get; set; }

    private IConfigurationRoot _config;

    private DiscordClient _discord;
    private InteractivityExtension _interactivity;

    private static async Task Main(string[] args) => await new Program().InitBot(args);

    private async Task InitBot(string[] args)
    {
        try
        {
            Console.WriteLine("[info] Welcome to my bot!");
            _cts = new CancellationTokenSource();

            Console.WriteLine("[info] Loading config file..");
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();

            Console.WriteLine("[info] Creating discord client..");
            _discord = new DiscordClient(
                new DiscordConfiguration
                {
                    Token = _config.GetValue<string>("discord:token"),
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
                }
            );

            _interactivity = _discord.UseInteractivity(
                new InteractivityConfiguration()
                {
                    PaginationBehaviour = PaginationBehaviour.WrapAround,
                    PaginationDeletion = PaginationDeletion.DeleteMessage,
                    Timeout = TimeSpan.FromSeconds(30)
                }
            );

            var services = BuildServices();

            var slash = _discord.UseSlashCommands(
                new SlashCommandsConfiguration { Services = services }
            );

            slash.RegisterCommands<SlashCommands>();

            Console.WriteLine("[info] Loading command modules..");

            await RunAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
        }
    }

    private async Task RunAsync(string[] args)
    {
        // Connect to discord's service
        Console.WriteLine("Connecting..");
        await _discord.ConnectAsync();
        Console.WriteLine("Connected!");
        var connections = _discord.Guilds.Count;
        Console.WriteLine($"I am running on {connections} servers");

        // Keep the bot running until the cancellation token requests we stop
        while (!_cts.IsCancellationRequested)
            await Task.Delay(TimeSpan.FromMinutes(1));
    }

    private ServiceProvider BuildServices()
    {
        var deps = new ServiceCollection();

        deps.AddSingleton(_interactivity)
            .AddSingleton(_cts)
            .AddSingleton(_config)
            .AddSingleton(_discord);
        return deps.BuildServiceProvider();
    }
}