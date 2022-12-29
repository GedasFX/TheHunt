using System.Net;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using TheHunt.Application;
using TheHunt.Domain.Models;

namespace TheHunt.Bot.Internal;

public class MySheet
{
    private readonly SheetsService _service;

    private static string[] Roles { get; } = { "Member", "🔍 Verifier" };
    // private const string SheetId = "1yPwaaznSMMXRVW4LUEm8L6-69dqFEdoeX7StQP0br-M";

    // private IDictionary<ulong, CompetitionMap> SheetsMap { get; } = new Dictionary<ulong, CompetitionMap>();

    public MySheet(string googleCredentialsFile)
    {
        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = GoogleCredential.FromFile(googleCredentialsFile)
        });
    }

    public async Task Playground()
    {
    }

    public async Task AddMember(SpreadsheetReference sheet, ulong userId, string displayName, int role, string? team)
    {
        await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
        {
            Requests = new[] 
            {
                SheetUtils.AppendRow(sheet.MembersSheet, new[]
                {
                    SheetUtils.StringCell(userId.ToString()),
                    SheetUtils.StringCell(displayName),
                    SheetUtils.StringDropdownCell(Roles[role], Roles),
                    SheetUtils.StringCell(team),
                })
            }
        }, sheet.SpreadsheetId).ExecuteAsync();
    }

    // public async Task AddSubmission(ulong competitionId, ulong submissionId, ulong submitterId, ulong verifierId, string imageUrl, DateTime date, string item,
    //     int bonus)
    // {
    //     var re = _service.Spreadsheets.Values.Append(new ValueRange()
    //     {
    //         Values = new IList<object>[]
    //         {
    //             // Id, Image, Date, Submitter, Verifier, Item, Points, Points Bonus
    //             new object[]
    //             {
    //                 submissionId, $"=IMAGE(\"{imageUrl}\")", date.ToString("=DATE(yyyy,MM,dd) + TI\\ME(HH,mm,ss)"), submitterId, verifierId, item,
    //                 "=VLOOKUP(INDIRECT(\"R[0]C[-1]\", FALSE), __teciui1_items!A2:C, 2, FALSE)"
    //             }
    //         },
    //     }, SheetId, $"__{competitionId}_submissions!A1:D");
    //     re.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
    //
    //     var a = await re.ExecuteAsync();
    //     // a.
    // }

    public async Task<SpreadsheetReference> CreateCompetition(string competitionId, string spreadsheetId)
    {
        try
        {
            var createBatch = await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
            {
                Requests = new[]
                {
                    new Request { AddSheet = CreateSheet(competitionId, "members", 4) },
                    new Request { AddSheet = CreateSheet(competitionId, "items", 3) },
                    new Request { AddSheet = CreateSheet(competitionId, "submissions", 9) },
                }
            }, spreadsheetId).ExecuteAsync();

            await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
            {
                Requests = new[]
                {
                    new Request { UpdateCells = AddHeaderRow(createBatch, 0, "Id", "Name", "Role", "Team") },
                    new Request { UpdateCells = AddHeaderRow(createBatch, 1, "Item Name", "Points Value", "Part of Set") },
                    new Request
                    {
                        UpdateCells = AddHeaderRow(createBatch, 2, "Id", "Image", "Date", "Submitter", "Verifier", "Item", "Item Points", "Bonus Points",
                            "Points Total")
                    }
                }
            }, spreadsheetId).ExecuteAsync();

            return new SpreadsheetReference
            {
                SpreadsheetId = spreadsheetId,
                MembersSheet = (int)createBatch.Replies[0].AddSheet.Properties.SheetId!,
                ItemsSheet = (int)createBatch.Replies[1].AddSheet.Properties.SheetId!,
                SubmissionsSheet = (int)createBatch.Replies[2].AddSheet.Properties.SheetId!
            };
        }
        catch (GoogleApiException e) when (e is { HttpStatusCode: HttpStatusCode.Forbidden, Error.Message: "The caller does not have permission" })
        {
            throw new EntityValidationException(
                "The bot does not have permission to edit this spreadsheet.\n" +
                "Send an invitation to 'the-hunt@the-hunt-373015.iam.gserviceaccount.com' with 'Editor' permissions.", e);
        }
    }

    private static UpdateCellsRequest AddHeaderRow(BatchUpdateSpreadsheetResponse createBatch, int index, params string[] header)
    {
        return new UpdateCellsRequest()
        {
            Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = createBatch.Replies[index].AddSheet.Properties.SheetId },
            Fields = "*", Rows = new[]
            {
                new RowData { Values = GetHeaderCells(header).ToList() }
            }
        };
    }

    private static AddSheetRequest CreateSheet(string competitionId, string name, int columnCount)
    {
        return new AddSheetRequest()
        {
            Properties = new SheetProperties()
            {
                Title = $"__{competitionId}_{name}", Hidden = true,
                GridProperties = new GridProperties() { FrozenRowCount = 1, ColumnCount = columnCount },
            }
        };
    }

    private static IEnumerable<CellData> GetHeaderCells(params string[] values)
    {
        return values.Select(value => new CellData()
        {
            UserEnteredValue = new ExtendedValue() { StringValue = value },
            UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
        });
    }
}