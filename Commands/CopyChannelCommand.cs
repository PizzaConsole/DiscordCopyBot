using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace PoisnCopy.Commands
{
    [RequirePermissions(Permissions.Administrator)]
    [Hidden]
    public class CopyChannelCommand : IModule
    {
        [Command("copychannel")]
        [Description("Copy a channel")]
        public async Task CopyChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync("Which Channel would you like to copy? (copy and paste the Id) Pleaes wait while I find all of your channels, I will give you a message when I have found them all.");

            await ctx.TriggerTypingAsync();
            var textChannels = ctx.Guild.Channels.Where(i => i.Type == ChannelType.Text).ToList();
            foreach (var txtChan in textChannels)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"`{txtChan.Id}-{txtChan.Name}`");
                await ctx.TriggerTypingAsync();
            }
            await ctx.RespondAsync($"Thats all of the channels!");

            var intr = ctx.Client.GetInteractivityModule(); // Grab the interactivity module
            var response = await intr.WaitForMessageAsync(
                c => c.Author.Id == ctx.Message.Author.Id, // Make sure the response is from the same person who sent the command
                TimeSpan.FromSeconds(60) // Wait 60 seconds for a response instead of the default 30 we set earlier!
            );

            if (response == null)
            {
                await ctx.RespondAsync("Sorry, I didn't get a response!");
                return;
            }

            var selectedChannel = textChannels.FirstOrDefault(i => i.Id.ToString() == response.Message.Content);

            if (selectedChannel == null)
            {
                await ctx.RespondAsync("Sorry, but that channel does not exist!");
                return;
            }

            await ctx.RespondAsync($"Copy command: `pc.loadchannel {selectedChannel.GuildId} {selectedChannel.Id}`");
        }

        [Command("loadchannel")]
        [Description("Load a copied channel")]
        public async Task LoadChannel(CommandContext ctx, ulong guildId, ulong channelId)
        {
            var guild = await ctx.Client.GetGuildAsync(guildId);
            var selectedChannel = guild.GetChannel(channelId);

            await ctx.RespondAsync("What do you want the new channel to be named?");

            var intr = ctx.Client.GetInteractivityModule(); // Grab the interactivity module
            var response = await intr.WaitForMessageAsync(
                c => c.Author.Id == ctx.Message.Author.Id, // Make sure the response is from the same person who sent the command
                TimeSpan.FromSeconds(60) // Wait 60 seconds for a response instead of the default 30 we set earlier!
            );

            if (response == null)
            {
                await ctx.RespondAsync("Sorry, I didn't get a response!");
                return;
            }

            if (string.IsNullOrEmpty(response.Message.Content))
            {
                await ctx.RespondAsync("Name cannot be empty!");
                return;
            }

            var channelName = response.Message.Content;

            await ctx.RespondAsync("Starting copy...");

            await ctx.RespondAsync("Collecting messages...");

            var messag = await selectedChannel.GetMessagesAsync();

            var messCopy = messag.ToList();

            var more = await selectedChannel.GetMessagesAsync(100, messag.LastOrDefault().Id);

            while (more.Count > 0)
            {
                messCopy.AddRange(more);
                more = await selectedChannel.GetMessagesAsync(100, more.LastOrDefault().Id);
            }

            await ctx.RespondAsync("Organizing messages...");

            messCopy.Reverse();

            var newMess = new List<DiscordMessage>();

            await ctx.RespondAsync("Creating channel...");

            var newChan = await ctx.Guild.CreateChannelAsync(channelName, selectedChannel.Type);

            await ctx.RespondAsync($"Posting {messCopy.Count} messages... (this could take awhile)");

            foreach (var mes in messCopy)
            {
                if (!string.IsNullOrEmpty(mes.Content))
                {
                    var whAu = new EmbedAuthor { Name = mes.Author.Username, IconUrl = mes.Author.AvatarUrl };
                    var what = new DiscordEmbedBuilder { Description = mes.Content, Author = whAu, Timestamp = mes.Timestamp };

                    await newChan.SendMessageAsync(null, false, what);
                }

                if (mes.Attachments.Count > 0)
                {
                    foreach (var att in mes.Attachments)
                    {
                        var whAu = new EmbedAuthor { Name = mes.Author.Username, IconUrl = mes.Author.AvatarUrl };
                        var what = new DiscordEmbedBuilder { ImageUrl = att.Url, Author = whAu, Timestamp = mes.Timestamp };

                        await newChan.SendMessageAsync(null, false, what);
                    }
                }
            }

            await ctx.RespondAsync($"{newChan.Name} copy complete!");
        }
    }
}