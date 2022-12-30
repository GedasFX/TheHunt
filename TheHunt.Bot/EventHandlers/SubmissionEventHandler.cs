using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TheHunt.Application;
using TheHunt.Bot.Services;

namespace TheHunt.Bot.EventHandlers;

public static class SubmissionEventHandler
{
    private static IEmote EmojiInfo { get; } = new Emoji("🔍");
    private static IEmote EmojiCancel { get; } = new Emoji("❌");

    public static void Register(DiscordSocketClient discord, IServiceProvider serviceProvider)
    {
        var channelsCacheProvider = serviceProvider.GetRequiredService<ActiveCompetitionsProvider>();

        discord.MessageReceived += message =>
        {
            Task.Run(async () =>
            {
                if (message.Attachments.Count == 0 && message.Embeds.Count == 0)
                    return;

                if (!channelsCacheProvider.ActiveChannelMap.ContainsKey(message.Channel.Id))
                    return;

                // Message is intended as a competition submission.

                // var embed = new EmbedBuilder()
                //     .WithAuthor(message.Author)
                //     .WithCurrentTimestamp()
                //     .Build();

                switch (ValidateSubmission(message))
                {
                    case 0:
                    {
                        return;
                    }
                    case 1:
                    {
                        await message.AddReactionAsync(EmojiInfo);
                        await message.AddReactionAsync(EmojiCancel);
                        return;
                    }
                    default:
                    {
                        await message.Author.SendMessageAsync(
                            $"I see you sent multiple images at once in {MentionUtils.MentionChannel(message.Channel.Id)}. I am easily overwhelmed, so if you wish to make a submission, make sure to send only one image per message.");
                        return;
                    }
                }
            });
            return Task.CompletedTask;
        };

        discord.ReactionAdded += async (cacheable, cacheable1, reaction) =>
        {
            // Ignore self
            if (!reaction.User.IsSpecified || reaction.User.Value.Id == discord.CurrentUser.Id)
                return;

            if (Equals(reaction.Emote, EmojiInfo) && channelsCacheProvider.ActiveChannelMap.ContainsKey(reaction.Channel.Id))
            {
                var message = await cacheable.GetOrDownloadAsync();
                if (ValidateSubmission(message) != 1)
                    return;

                await message.Author.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle("Submission info")
                    .WithDescription($"Submitted in <#{reaction.Channel.Id}>")
                    .WithAuthor(message.Author)
                    .WithImageUrl(GetAttachedImageUrl(message))
                    .WithTimestamp(message.Timestamp)
                    .Build());
            }
        };
    }

    private static int ValidateSubmission(IMessage message)
    {
        var counter = 0;
        foreach (var attachment in message.Attachments)
            if (attachment.ContentType.StartsWith("image/") && ++counter > 1)
                return counter;

        foreach (var embed in message.Embeds)
            if (embed.Type == EmbedType.Image && ++counter > 1)
                return counter;

        return counter;
    }

    private static string GetAttachedImageUrl(IMessage message)
    {
        var url = message.Attachments.FirstOrDefault(a => a.ContentType.StartsWith("image/"))?.Url;
        if (url != null)
            return url;

        url = message.Embeds.FirstOrDefault(e => e.Type == EmbedType.Image)?.Url;
        if (url != null)
            return url;

        throw new EntityValidationException("No image was found on the message.");
    }
}