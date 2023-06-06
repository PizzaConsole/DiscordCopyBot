using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PoisnCopy;

internal class Program
{
    /* This is the cancellation token we'll use to end the bot if needed(used for most async stuff). */
    private CancellationTokenSource _cts { get; set; }

    /* We'll load the app config into this when we create it a little later. */
    private IConfigurationRoot _config;

    /* These are the discord library's main classes */
    private DiscordClient _discord;
    private CommandsNextExtension _commands;
    private InteractivityExtension _interactivity;

    private static async Task Main(string[] args) => await new Program().InitBot(args);

    private async Task InitBot(string[] args)
    {
        try
        {
            Console.WriteLine("[info] Welcome to my bot!");
            _cts = new CancellationTokenSource();

            // Load the config file(we'll create this shortly)
            Console.WriteLine("[info] Loading config file..");
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();

            // Create the DSharpPlus client
            Console.WriteLine("[info] Creating discord client..");
            _discord = new DiscordClient(
                new DiscordConfiguration
                {
                    Token = _config.GetValue<string>("discord:token"),
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
                }
            );

            // Create the interactivity module(I'll show you how to use this later on)
            _interactivity = _discord.UseInteractivity(
                new InteractivityConfiguration()
                {
                    PaginationBehaviour = PaginationBehaviour.WrapAround,
                    PaginationDeletion = PaginationDeletion.DeleteMessage,
                    Timeout = TimeSpan.FromSeconds(30)
                }
            );

            // Build dependancies and then create the commands module.
            var services = BuildServices();
            _commands = _discord.UseCommandsNext(
                new CommandsNextConfiguration
                {
                    StringPrefixes = new List<string>
                    {
                        _config.GetValue<string>("discord:CommandPrefix")
                    },
                    Services = services,
                    EnableDms = false
                }
            );
            var slash = _discord.UseSlashCommands();

            slash.RegisterCommands<SlashCommands>();

            Console.WriteLine("[info] Loading command modules..");

            var type = typeof(IModule);
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

            var typeList = types as Type[] ?? types.ToArray();
            foreach (var t in typeList)
            {
                _commands.RegisterCommands(t);
            }

            Console.WriteLine($"[info] Loaded {typeList.Count()} modules.");
            await RunAsync(args);
        }
        catch (Exception ex)
        {
            // This will catch any exceptions that occur during the operation/setup of your bot.

            // Feel free to replace this with what ever logging solution you'd like to use.
            // I may do a guide later on the basic logger I implemented in my most recent bot.
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

    /*
     DSharpPlus has dependancy injection for commands, this builds a list of dependancies.
     We can then access these in our command modules.
    */

    private ServiceProvider BuildServices()
    {
        var deps = new ServiceCollection();

        deps.AddSingleton(_interactivity) // Add interactivity
            .AddSingleton(_cts) // Add the cancellation token
            .AddSingleton(_config) // Add our config
            .AddSingleton(_discord); // Add the discord client

        return deps.BuildServiceProvider();
    }
}