using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using TheHunt.Application;
using TheHunt.Application.Helpers;
using TheHunt.Domain;
using TheHunt.Domain.Models;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    [Group("members", "Provides tools for managing competition members.")]
    public class CompetitionsMembersModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AppDbContext _dbContext;

        public CompetitionsMembersModule(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("invite", "Adds a user to the competition.")]
        public async Task Invite(
            [Summary(description: "User to invite to the competition.")]
            IGuildUser user)
        {
            await DeferAsync(ephemeral: true);

            if (await _dbContext.CompetitionUsers.AsNoTracking()
                    .AnyAsync(c => c.UserId == user.Id && c.CompetitionId == Context.Channel.Id))
                throw new EntityValidationException($"<@{user.Id}> is already part of the competition.");

            _dbContext.Add(new CompetitionUser { UserId = user.Id, CompetitionId = Context.Channel.Id, RegistrationDate = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync();

            await FollowupAsync($"<@{user.Id}> was successfully added to the competition.", ephemeral: true);
            await FollowupAsync($"<@{Context.User.Id}> added <@{user.Id}> to the competition.");
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("kick", "Removes a user from the competition.")]
        public async Task Kick(
            [Summary(description: "User to remove from the competition.")]
            IGuildUser user)
        {
            await DeferAsync(ephemeral: true);

            if (!await _dbContext.CompetitionUsers.AsNoTracking()
                    .AnyAsync(c => c.UserId == user.Id && c.CompetitionId == Context.Channel.Id))
                throw new EntityValidationException($"<@{user.Id}> is not part of the competition.");

            _dbContext.Remove(new CompetitionUser { UserId = user.Id, CompetitionId = Context.Channel.Id });
            await _dbContext.SaveChangesAsync();

            await FollowupAsync($"<@{user.Id}> was successfully removed from the competition.", ephemeral: true);
            await FollowupAsync($"<@{Context.User.Id}> removed <@{user.Id}> from the competition.");
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("set-role", "Changes the permissions of the user.")]
        public async Task SetRole(
            [Summary(description: "User to remove from the competition.")]
            IGuildUser user,
            [Summary(description: "Privilege level.")] [Choice("Verifier", 1), Choice("Participant", 0)]
            int role)
        {
            await DeferAsync(ephemeral: true);

            var dbUser = await _dbContext.CompetitionUsers.SingleOrDefaultAsync(c => c.UserId == Context.User.Id && c.CompetitionId == Context.Channel.Id);
            if (dbUser == null)
                throw new EntityValidationException($"<@{user.Id}> is not part of the competition.");

            dbUser.IsModerator = role == 1;
            await _dbContext.SaveChangesAsync();

            await FollowupAsync($"<@{user.Id}> is now a {(dbUser.IsModerator ? "Verifier" : "Regular participant")}.", ephemeral: true);
            await FollowupAsync($"<@{Context.User.Id}> made <@{user.Id}> a {(dbUser.IsModerator ? "Verifier" : "Regular participant")}.");
        }
    }
}