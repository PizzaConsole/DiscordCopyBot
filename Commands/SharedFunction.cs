using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace PoisnCopy;

public static class SharedFunction
{
    public static async Task<(
        List<DiscordMessage>,
        DiscordChannel
    )> CollectAndOrganizeMessagesAsync(
        this InteractionContext ctx,
        ulong server_id,
        ulong channel_id
    )
    {
        var guild = await ctx.Client.GetGuildAsync(server_id);
        DiscordChannel selectedChannel = guild.GetChannel(channel_id);

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

        return (messCopy, selectedChannel);
    }
}