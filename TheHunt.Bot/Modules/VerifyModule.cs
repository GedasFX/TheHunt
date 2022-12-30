using Discord;
using Discord.Interactions;
using TheHunt.Bot.Internal;
using TheHunt.Bot.Services;

namespace TheHunt.Bot.Modules;

public class VerifyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ActiveCompetitionsProvider _competitionsProvider;
    private readonly CompetitionsQueryService _competitionsQueryService;
    private readonly SpreadsheetQueryService _spreadsheetQueryService;
    private readonly SpreadsheetService _spreadsheetService;

    private IEmote VerifiedEmote { get; } = new Emoji("🤣");

    public VerifyModule(ActiveCompetitionsProvider competitionsProvider, CompetitionsQueryService competitionsQueryService,
        SpreadsheetQueryService spreadsheetQueryService, SpreadsheetService spreadsheetService)
    {
        _competitionsProvider = competitionsProvider;
        _competitionsQueryService = competitionsQueryService;
        _spreadsheetQueryService = spreadsheetQueryService;
        _spreadsheetService = spreadsheetService;
    }

    [MessageCommand("Verify Submission")]
    public async Task VerifySubmission(IUserMessage message)
    {
        if (!_competitionsProvider.ActiveChannelMap.ContainsKey(message.Channel.Id))
        {
            await RespondAsync("Unable to verify submission: Channel is not associated with a competition.", ephemeral: true);
            return;
        }

        if (await message.GetReactionUsersAsync(VerifiedEmote, 1).Flatten().AnyAsync())
        {
            await RespondAsync("Submission was already verified.", ephemeral: true);
            return;
        }

        await RespondWithModalAsync<SubmissionModal>($"submission_create:{message.Id}");
    }

    public class SubmissionModal : IModal
    {
        public string Title => "Verify Submission";

        [RequiredInput(false), InputLabel("[Optional] Item")]
        [ModalTextInput("sub_name", TextInputStyle.Short, "Hylian Shield / Broken Zenith", maxLength: 240)]
        public string? Item { get; set; }

        // Strings with the ModalTextInput attribute will automatically become components.
        [RequiredInput(false), InputLabel("[Optional] Bonus Points")]
        [ModalTextInput("sub_bonus", placeholder: "0 / 16 / -55", maxLength: 11)]
        public string? Bonus { get; set; }
    }

    [ModalInteraction("submission_create:*")]
    public async Task ModalResponse(ulong channelId, SubmissionModal modal)
    {
        var verifier = await _spreadsheetQueryService.GetCompetitionVerifier(Context.Channel.Id, Context.User.Id);
        if (verifier?.Role != 1)
        {
            await FollowupAsync("Unable to verify submission: You are not part of the verifiers group.", ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);

        var message = await Context.Channel.GetMessageAsync(channelId);

        var sheetsRef = await _competitionsQueryService.GetSpreadsheetRef(Context.Channel.Id);
        await _spreadsheetService.AddSubmission(sheetsRef!, Context.Channel.Id, message.Id, message.Author.Id, Context.User.Id, GetAttachedImageUrl(message),
            message.Timestamp.UtcDateTime, modal.Item, int.TryParse(modal.Bonus, out var bonus) ? bonus : 0);

        await message.AddReactionAsync(VerifiedEmote);
        await FollowupAsync("Submission verified successfully!", ephemeral: true);
    }

    private static string? GetAttachedImageUrl(IMessage message)
    {
        return message.Attachments.FirstOrDefault(a => a.ContentType.StartsWith("image/"))?.Url ??
               message.Embeds.FirstOrDefault(e => e.Type == EmbedType.Image)?.Url;
    }
}