using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Caching.Memory;

namespace TheHunt.Bot.Internal;

public class MySheet
{
    private readonly SheetsService _service;
    private const string SheetId = "1yPwaaznSMMXRVW4LUEm8L6-69dqFEdoeX7StQP0br-M";

    private IDictionary<ulong, string> SheetsMap { get; } = new Dictionary<ulong, string>();

    public MySheet()
    {
        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = GoogleCredential.FromFile("google.json")
        });
    }

    public async Task Playground()
    {
        // await CreateCompetition("teciui");
        // var re = _service.Spreadsheets.Values.Append(new ValueRange()
        // {
        //     Values = new[] { new[] {  } },
        //     
        // });
        // re.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

        // await AddMember()
        // await AddSubmission(1234, 1234, "https://cdn.discordapp.com/attachments/837377172321992754/1057686219124920421/best_scarf_open_eyes_no_bg.png",
        //     DateTime.UtcNow);
    }

    public async Task AddMember(ulong competitionId, ulong userId, string displayName, string? team)
    {
        var re = _service.Spreadsheets.Values.Append(new ValueRange()
        {
            Values = new IList<object?>[]
            {
                // Id, DisplayName, Team
                new object?[]
                {
                    userId, displayName, team
                }
            },
        }, SheetId, $"__{competitionId}__submissions!A1:D");
        re.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
    }

    public async Task AddSubmission(ulong competitionId, ulong submissionId, ulong submitterId, ulong verifierId, string imageUrl, DateTime date)
    {
        var re = _service.Spreadsheets.Values.Append(new ValueRange()
        {
            Values = new IList<object>[]
            {
                // Id, Image, Date, SubmitterName Lookup, VerifierName Lookup, Item, Bonus
                new object[]
                {
                    submissionId, $"=IMAGE(\"{imageUrl}\")", date.ToString("=DATE(yyyy,MM,dd) + TI\\ME(HH,mm,ss)"), submitterId, verifierId
                }
            },
        }, SheetId, $"__{competitionId}__submissions!A1:D");
        re.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

        var a = await re.ExecuteAsync();
        // a.
    }

    public async Task CreateCompetition(string competitionId)
    {
        var createBatch = await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
        {
            Requests = new[]
            {
                new Request()
                {
                    AddSheet = new AddSheetRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            Hidden = true,
                            Title = $"__{competitionId}__members",
                            GridProperties = new GridProperties() { FrozenRowCount = 1, ColumnCount = 4 },
                        }
                    }
                },
                new Request()
                {
                    AddSheet = new AddSheetRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            Hidden = true,
                            Title = $"__{competitionId}__submissions",
                            GridProperties = new GridProperties() { FrozenRowCount = 1, ColumnCount = 8 },
                        }
                    }
                },
            }
        }, SheetId).ExecuteAsync();

        await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
        {
            Requests = new[]
            {
                new Request()
                {
                    UpdateCells = new UpdateCellsRequest()
                    {
                        Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = createBatch.Replies[0].AddSheet.Properties.SheetId },
                        Fields = "*",
                        Rows = new[]
                        {
                            new RowData()
                            {
                                Values = new[]
                                {
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Id" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } },
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Name" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Team" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Role" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                },
                            }
                        }
                    }
                },
                new Request()
                {
                    UpdateCells = new UpdateCellsRequest()
                    {
                        Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = createBatch.Replies[1].AddSheet.Properties.SheetId },
                        Fields = "*",
                        Rows = new[]
                        {
                            new RowData()
                            {
                                Values = new[]
                                {
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Id" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } },
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Image" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Date" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Submitter" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Verifier" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Item" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Points" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                    new CellData()
                                    {
                                        UserEnteredValue = new ExtendedValue() { StringValue = "Points Bonus" },
                                        UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } }
                                    },
                                },
                            }
                        }
                    }
                }
            }
        }, SheetId).ExecuteAsync();
    }
}