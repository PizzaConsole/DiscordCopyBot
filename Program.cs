using DSharpPlus;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;

namespace PoisnCopy;

internal class Program
{
    public readonly EventId BotEventId = new(42, "ControlCopy");
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
            slash.SlashCommandErrored += Commands_CommandErrored;

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
        await _discord.ConnectAsync(new DiscordActivity { Name = "ko-fi.com/poisnfang", });
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

    private async Task Commands_CommandErrored(
        SlashCommandsExtension sender,
        SlashCommandErrorEventArgs e
    )
    {
        e.Context.Client.Logger.LogError(
            BotEventId,
            $"{e.Context.User.Username} tried executing '{e.Context.CommandName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
            DateTime.Now
        );

        if (e.Exception is SlashExecutionChecksFailedException ex)
        {
            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Access denied",
                Description =
                    $"{emoji} You do not have the permissions required to execute this command.",
                Color = new DiscordColor(0xFF0000) // red
            };
            await e.Context.CreateResponseAsync(embed);
        }
    }
}