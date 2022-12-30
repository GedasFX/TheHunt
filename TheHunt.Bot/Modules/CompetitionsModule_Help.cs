using Discord;
using Discord.Interactions;

namespace TheHunt.Bot.Modules;

public partial class CompetitionsModule
{
    [Group("help", "Provides tools for managing competition members.")]
    public class CompetitionsHelpModule : InteractionModuleBase<SocketInteractionContext>
    {
        private static Embed HelpCreateEmbed { get; } = new EmbedBuilder()
            .WithTitle("Help - /competitions create")
            .AddField("Step 1 - Create a Google Spreadsheet", """
The spreadsheet will be used to track submissions and manage members of the competition. You can reuse older spreadsheets for multiple competitions.
""")
            .AddField("Step 2 - Invite the bot Service Account", """
For the bot to be able to make edits, it must have access to the spreadsheet. To achieve this, invite the bot with 'Editor' role. 
**Service Account (Email):**
```
the-hunt@the-hunt-373015.iam.gserviceaccount.com
```
""")
            .AddField("Step 3 - Extract Spreadsheet ID", """
Spreadsheet ID will be used to bind competitions to spreadsheets. 
ID can be extracted from the url (`SPREADSHEET_ID`):
```
https://docs.google.com/spreadsheets/d/SPREADSHEET_ID/edit#gid=0
```
""")
            .AddField("Step 4 - Create the Competition Channel", """
Create a new channel for the competition. This is the place where participants will be able to submit competition entries.

**NB!** Once competition is created, this channel be permanently bound to that competition.
""")
            .AddField("Step 5 - /competitions create", """
In the channel you wish to create the competition, run `/competitions create SPREADSHEET_ID`. This will create the competition, bind it to the spreadsheet, and create new sheets there for use in the competition.

**NB!** You need to have `Manage Channels` permission to run this command.

**NB!** It is not recommended to rename the newly created sheets (with exception of Overview) while competition is in progress.

**NB!** For more information about manually editing the sheets, use `/competitions help manual-edit`.
""")
            .AddField("[Optional] Step 6 - /competitions show", """
To see the competition information and status, use `/competitions show` in the channel the competition was created.
""")
            .Build();

        [SlashCommand("create", "Instructions on how to create a competition.")]
        public async Task HelpCreate()
        {
            await RespondAsync(embed: HelpCreateEmbed, ephemeral: true);
        }
    }
}