using Discord;

namespace TheHunt.Bot.Utils;

public static class FormatUtils
{
    public static string GetSheetUrl(string spreadsheetId, int sheetId)
    {
        return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit#gid={sheetId}";
    }

    public static ActionRowBuilder WithSpreadsheetRefButton(this ActionRowBuilder builder, string label, string emoji, string spreadsheetId, int sheetId)
    {
        return builder.WithButton(label: label, emote: new Emoji(emoji), style: ButtonStyle.Link, url: GetSheetUrl(spreadsheetId, sheetId));
    }
}