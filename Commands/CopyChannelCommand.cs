using CsvHelper;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Globalization;
using System.IO;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace PoisnCopy.Commands;

[RequirePermissions(
    Permissions.SendMessages
        & Permissions.AccessChannels
        & Permissions.ManageChannels
        & Permissions.ReadMessageHistory
        & Permissions.EmbedLinks
        & Permissions.AttachFiles
)]
[Hidden]
public class CopyChannelCommand : BaseCommandModule, IModule
{
    [Command("copychannel")]
    [Description("Copy a channel")]
    public async Task CopyChannel(CommandContext ctx)
    {
        await ctx.TriggerTypingAsync();

        await ctx.RespondAsync("One moment as I compile the list of channels");

        await ctx.TriggerTypingAsync();
        var textChannels = ctx.Guild.Channels.Where(i => i.Value.Type == ChannelType.Text).ToList();
        List<string> messages =
            new()
            {
                "Which Channel would you like to copy? (copy and paste the Id) Pleaes wait while I find all of your channels, I will give you a message when I have found them all."
            };

        foreach (var txtChan in textChannels)
        {
            messages.Add($"`{txtChan.Value.Id}-{txtChan.Value.Name}`");
            //var dismsg = await ctx.Channel.SendMessageAsync(
            //   $"`{txtChan.Value.Id}-{txtChan.Value.Name}`"
            //   );
            //await dismsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_up_small:"));
        }

        messages.Add("Thats all of the channels!");
        var msg = await ctx.RespondAsync(string.Join("\n", messages));

        //wait for reaction from user

        var intr = ctx.Client.GetInteractivity(); // Grab the interactivity module
        var response = await intr.WaitForMessageAsync(
            c => c.Author.Id == ctx.Message.Author.Id,
            TimeSpan.FromSeconds(60)
        );

        if (response.Result == null)
        {
            await ctx.RespondAsync("Sorry, I didn't get a response!");
            return;
        }

        var selectedChannel = textChannels.FirstOrDefault(
            i => i.Value.Id.ToString() == response.Result.Content
        );

        if (selectedChannel.Value == null)
        {
            await ctx.RespondAsync("Sorry, but that channel does not exist!");
            return;
        }

        await ctx.RespondAsync(
            $"Copy command: `pc.loadchannel {selectedChannel.Value.GuildId} {selectedChannel.Value.Id}`"
        );
        await ctx.RespondAsync(
            $"Export command: `pc.exportchannel {selectedChannel.Value.GuildId} {selectedChannel.Value.Id}`"
        );
    }

    [Command("loadchannel")]
    [Description("Load a copied channel")]
    public async Task LoadChannel(CommandContext ctx, ulong guildId, ulong channelId)
    {
        var guild = await ctx.Client.GetGuildAsync(guildId);
        var selectedChannel = guild.GetChannel(channelId);

        await ctx.RespondAsync("What do you want the new channel to be named?");

        var intr = ctx.Client.GetInteractivity(); // Grab the interactivity module
        var response = await intr.WaitForMessageAsync(
            c => c.Author.Id == ctx.Message.Author.Id, // Make sure the response is from the same person who sent the command
            TimeSpan.FromSeconds(60) // Wait 60 seconds for a response instead of the default 30 we set earlier!
        );

        if (response.Result == null)
        {
            await ctx.RespondAsync("Sorry, I didn't get a response!");
            return;
        }

        if (string.IsNullOrEmpty(response.Result.Content))
        {
            await ctx.Channel.SendMessageAsync("Name cannot be empty!");
            return;
        }

        var channelName = response.Result.Content;

        await ctx.Channel.SendMessageAsync("Starting copy...");

        await ctx.Channel.SendMessageAsync("Collecting messages...");

        var messag = await selectedChannel.GetMessagesAsync();

        var messCopy = messag.ToList();
        var more = await selectedChannel.GetMessagesAsync(100);

        while (more.Count > 0)
        {
            messCopy.AddRange(more);
            more = await selectedChannel.GetMessagesBeforeAsync(more.LastOrDefault().Id, 100);
        }

        await ctx.Channel.SendMessageAsync("Organizing messages...");

        messCopy.Reverse();

        var newMess = new List<DiscordMessage>();

        await ctx.Channel.SendMessageAsync("Creating channel...");

        var newChan = await ctx.Guild.CreateChannelAsync(channelName, selectedChannel.Type);

        await ctx.Channel.SendMessageAsync(
            $"Posting {messCopy.Count} messages... (this could take awhile)"
        );

        foreach (var mes in messCopy)
        {
            if (!string.IsNullOrEmpty(mes.Content))
            {
                var whAu = new EmbedAuthor
                {
                    Name = mes.Author.Username,
                    IconUrl = mes.Author.AvatarUrl
                };
                var what = new DiscordEmbedBuilder
                {
                    Description = mes.Content,
                    Author = whAu,
                    Timestamp = mes.Timestamp
                };

                await newChan.SendMessageAsync(what);
                await Task.Delay(800);
            }

            if (mes.Attachments.Count > 0)
            {
                foreach (var att in mes.Attachments)
                {
                    var whAu = new EmbedAuthor
                    {
                        Name = mes.Author.Username,
                        IconUrl = mes.Author.AvatarUrl
                    };
                    var what = new DiscordEmbedBuilder
                    {
                        ImageUrl = att.Url,
                        Author = whAu,
                        Timestamp = mes.Timestamp
                    };

                    await newChan.SendMessageAsync(what);
                    await Task.Delay(800);
                }
            }
        }

        await ctx.RespondAsync($"{newChan.Name} copy complete!");
    }

    [Command("exportchannel")]
    [Description("Export a copied channel")]
    public async Task ExportChannel(CommandContext ctx, ulong guildId, ulong channelId)
    {
        var guild = await ctx.Client.GetGuildAsync(guildId);
        var selectedChannel = guild.GetChannel(channelId);

        await ctx.Channel.SendMessageAsync("Starting export...");

        await ctx.Channel.SendMessageAsync("Collecting messages...");

        var messag = await selectedChannel.GetMessagesAsync();

        var messCopy = messag.ToList();
        var more = await selectedChannel.GetMessagesAsync(100);

        while (more.Count > 0)
        {
            messCopy.AddRange(more);
            more = await selectedChannel.GetMessagesBeforeAsync(more.LastOrDefault().Id, 100);
        }

        await ctx.Channel.SendMessageAsync("Organizing messages...");

        messCopy.Reverse();

        var messageExports = new List<MessageExport>();

        await ctx.Channel.SendMessageAsync(
            $"Exporting {messCopy.Count} messages... (this could take awhile)"
        );

        foreach (var mes in messCopy)
        {
            if (!string.IsNullOrEmpty(mes.Content))
            {
                var textMessage = new MessageExport
                {
                    AuthorName = mes.Author.Username,
                    IconUrl = mes.Author.AvatarUrl,
                    MessageConent = mes.Content,
                    Timestamp = mes.Timestamp.ToString("o")
                };
                messageExports.Add(textMessage);
            }

            if (mes.Attachments.Count > 0)
            {
                foreach (var att in mes.Attachments)
                {
                    var imageMessage = new MessageExport
                    {
                        AuthorName = mes.Author.Username,
                        IconUrl = mes.Author.AvatarUrl,
                        MessageConent = att.Url,
                        Timestamp = mes.Timestamp.ToString("o")
                    };
                    messageExports.Add(imageMessage);
                }
            }
        }

        try
        {
            using var memStream = new MemoryStream();
            using var writer = new StreamWriter(memStream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(messageExports);
            await writer.FlushAsync();
            memStream.Position = 0;
            var fileMessage = new DiscordMessageBuilder() { Content = "Messages exported" };
            fileMessage.AddFile($"{selectedChannel.Name}-export.csv", memStream, true);

            await ctx.Channel.SendMessageAsync(fileMessage);
        }
        catch (Exception e)
        {
            await ctx.Channel.SendMessageAsync(e.Message);
        }
    }
}
