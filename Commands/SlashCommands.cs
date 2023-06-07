using CsvHelper;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System.Globalization;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace PoisnCopy;

[SlashRequirePermissions(
    Permissions.SendMessages
        | Permissions.AccessChannels
        | Permissions.ManageChannels
        | Permissions.ReadMessageHistory
        | Permissions.EmbedLinks
        | Permissions.AttachFiles
        | Permissions.ReadMessageHistory
        | Permissions.CreatePrivateThreads
        | Permissions.CreatePublicThreads
        | Permissions.ManageThreads
        | Permissions.SendMessagesInThreads
        | Permissions.ManageEmojis
        | Permissions.UseExternalEmojis
)]
public class SlashCommands : ApplicationCommandModule
{
    [SlashCommand("copychannel", "Copy a channel")]
    public async Task CopyChannel(InteractionContext ctx)
    {
        try
        {
            await ctx.DeferAsync();

            var textChannels = ctx.Guild.Channels
                .Where(i => i.Value.Type == ChannelType.Text)
                .ToList();
            List<string> messages = new() { "Here is your list of channels:" };

            foreach (var txtChan in textChannels)
            {
                messages.Add($"`{txtChan.Value.Id}-{txtChan.Value.Name}`");
            }

            messages.Add("**--Thats all of the channels!--**");
            messages.Add(
                $"Copy command: `/loadchannel` server_id:`{ctx.Guild.Id}` channel_id:`###`"
            );
            messages.Add(
                $"Export command: `/exportchannel` server_id:`{ctx.Guild.Id}` channel_id:`###`"
            );
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent(string.Join("\n", messages))
            );
        }
        catch (Exception e)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(e.Message));
            return;
        }
    }

    [SlashCommand("loadchannel", "Load a copied channel")]
    public async Task LoadChannel(
        InteractionContext ctx,
        [Option("server_id", "Originating Server ID")] string server_id,
        [Option("channel_id", "Channel ID")] string channel_id,
        [Option("new_channel_name", "The name of the new channel to create")]
            string new_channel_name
    )
    {
        try
        {
            var chanParsed = ulong.TryParse(channel_id, out ulong chan_id);

            if (!chanParsed)
            {
                await ctx.CreateResponseAsync("Invalid Channel ID");

                return;
            }

            var serverParsed = ulong.TryParse(server_id, out ulong serv_id);

            if (!serverParsed)
            {
                await ctx.CreateResponseAsync("Invalid Server ID");

                return;
            }

            var guild = await ctx.Client.GetGuildAsync(serv_id);
            var selectedChannel = guild.GetChannel(chan_id);

            await ctx.CreateResponseAsync("Collecting messages...");

            await ctx.Channel.TriggerTypingAsync();
            var messag = await selectedChannel.GetMessagesAsync();

            var messCopy = messag.ToList();
            var more = await selectedChannel.GetMessagesBeforeAsync(messCopy.LastOrDefault().Id);

            while (more.Count > 0)
            {
                messCopy.AddRange(more);
                await Task.Delay(800);
                more = await selectedChannel.GetMessagesBeforeAsync(more.LastOrDefault().Id, 100);
            }

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("Organizing messages...")
            );

            messCopy.Reverse();

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("Creating channel...")
            );

            var newChan = await ctx.Guild.CreateChannelAsync(
                new_channel_name,
                selectedChannel.Type,
                nsfw: selectedChannel.IsNSFW
            );

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent(
                    $"Posting {messCopy.Count} messages... (this could take awhile)"
                )
            );
            using var httpClient = new HttpClient();
            foreach (var mes in messCopy)
            {
                if (mes.Embeds.Count > 0)
                {
                    var embeds = new DiscordMessageBuilder().AddEmbeds(mes.Embeds);
                    await newChan.SendMessageAsync(embeds);
                    await Task.Delay(800);
                }

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

                foreach (var att in mes.Attachments)
                {
                    var whAu = new EmbedAuthor
                    {
                        Name = mes.Author.Username,
                        IconUrl = mes.Author.AvatarUrl
                    };
                    var what = new DiscordEmbedBuilder
                    {
                        Description = att.FileName,
                        Author = whAu,
                        Timestamp = mes.Timestamp,
                    };

                    await newChan.SendMessageAsync(what);
                    await Task.Delay(800);
                    var attachStream = await httpClient.GetStreamAsync(att.Url);

                    // upload attachment to discord
                    var attachDiscord = new DiscordMessageBuilder().AddFiles(
                        new Dictionary<string, Stream> { { att.FileName, attachStream } }
                    );

                    await newChan.SendMessageAsync(attachDiscord);
                    await Task.Delay(800);
                }
            }

            await ctx.FollowUpAsync(
                new DiscordFollowupMessageBuilder().WithContent($"{newChan.Name} copy complete!")
            );
        }
        catch (Exception e)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(e.Message));
            return;
        }
    }

    [SlashCommand("exportchannel", "Export a copied channel")]
    public async Task ExportChannel(
        InteractionContext ctx,
        [Option("server_id", "Originating Server ID")] string server_id,
        [Option("channel_id", "Channel ID")] string channel_id
    )
    {
        try
        {
            var chanParsed = ulong.TryParse(channel_id, out ulong chan_id);

            if (!chanParsed)
            {
                await ctx.CreateResponseAsync("Invalid Channel ID");

                return;
            }

            var serverParsed = ulong.TryParse(server_id, out ulong serv_id);

            if (!serverParsed)
            {
                await ctx.CreateResponseAsync("Invalid Server ID");

                return;
            }

            var guild = await ctx.Client.GetGuildAsync(serv_id);
            var selectedChannel = guild.GetChannel(chan_id);

            await ctx.CreateResponseAsync("Collecting messages...");

            await ctx.Channel.TriggerTypingAsync();
            var messag = await selectedChannel.GetMessagesAsync();

            var messCopy = messag.ToList();
            var more = await selectedChannel.GetMessagesAsync(100);

            while (more.Count > 0)
            {
                messCopy.AddRange(more);
                await Task.Delay(800);
                more = await selectedChannel.GetMessagesBeforeAsync(more.LastOrDefault().Id, 100);
            }

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("Organizing messages...")
            );

            messCopy.Reverse();
            var messageExports = new List<MessageExport>();

            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent(
                    $"Exporting {messCopy.Count} messages... (this could take awhile)"
                )
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

            using var memStream = new MemoryStream();
            using var writer = new StreamWriter(memStream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(messageExports);
            await writer.FlushAsync();
            memStream.Position = 0;
            var fileMessage = new DiscordFollowupMessageBuilder().WithContent(
                $"{selectedChannel.Name} export complete!"
            );
            fileMessage.AddFile($"{selectedChannel.Name}-export.csv", memStream, true);

            await ctx.FollowUpAsync(fileMessage);
        }
        catch (Exception e)
        {
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(e.Message));
            return;
        }
    }
}