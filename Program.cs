using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace PoisnCopy
{
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

        /* Use the async main to create an instance of the class and await it(async main is only available in C# 7.1 onwards). */

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
                        PaginationBehaviour = PaginationBehaviour.WrapAround, // What to do when a pagination request times out
                        PaginationDeletion = PaginationDeletion.DeleteMessage, // How long to wait before timing out
                        Timeout = TimeSpan.FromSeconds(30) // Default time to wait for interactive commands like waiting for a message or a reaction
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
                        }, // Load the command prefix(what comes before the command, eg "!" or "/") from our config file
                        Services = services,
                        EnableDms = false
                    }
                );

                Console.WriteLine("[info] Loading command modules..");

                var type = typeof(IModule); // Get the type of our interface
                var types = AppDomain.CurrentDomain
                    .GetAssemblies() // Get the assemblies associated with our project
                    .SelectMany(s => s.GetTypes()) // Get all the types
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterface); // Filter to find any type that can be assigned to an IModule

                var typeList = types as Type[] ?? types.ToArray(); // Convert to an array
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
}