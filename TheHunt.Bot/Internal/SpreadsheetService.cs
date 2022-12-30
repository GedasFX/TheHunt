using System.Net;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using TheHunt.Application;
using TheHunt.Domain.Models;

namespace TheHunt.Bot.Internal;

public class SpreadsheetService
{
    private readonly SheetsService _service;

    private static string[] Roles { get; } = { "Member", "🔍 Verifier" };

    private const int MembersIdIndex = 0;
    private const int MembersNameIndex = 1;
    private const int MembersRoleIndex = 2;


    public SpreadsheetService(string googleCredentialsFile)
    {
        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = GoogleCredential.FromFile(googleCredentialsFile)
        });
    }

    public async Task<IReadOnlyList<CompetitionUser>> GetMembers(SpreadsheetReference sheet)
    {
        try
        {
            var data = await _service.Spreadsheets.Values.BatchGetByDataFilter(new BatchGetValuesByDataFilterRequest()
            {
                DataFilters = new[]
                {
                    new DataFilter()
                        { GridRange = new GridRange() { SheetId = sheet.MembersSheetId, StartRowIndex = 1, StartColumnIndex = 0, EndColumnIndex = 1 } },
                    new DataFilter()
                        { GridRange = new GridRange() { SheetId = sheet.MembersSheetId, StartRowIndex = 1, StartColumnIndex = 2, EndColumnIndex = 3 } },
                }
            }, sheet.SpreadsheetId).ExecuteAsync();

            var idColumnIdx = data.ValueRanges.IndexOf(data.ValueRanges.First(v => v.DataFilters[0].GridRange.StartColumnIndex == 0));
            var roleColumnIdx = idColumnIdx == 0 ? 1 : 0;
            return (IReadOnlyList<CompetitionUser>?)data.ValueRanges[idColumnIdx].ValueRange.Values?
                       .Select((t, i) => (List: t, Idx: i)).Where(t => t.List?.Count > 0).Select((t, i) => new CompetitionUser
                       {
                           UserId = ulong.Parse((string)t.List[0]), RowIdx = t.Idx,
                           Role = (string)data.ValueRanges[roleColumnIdx].ValueRange.Values[i][0] == Roles[1] ? (byte)1 : (byte)0
                       }).ToList()
                   ?? Array.Empty<CompetitionUser>();
        }
        catch (Exception e)
        {
            throw new EntityValidationException("""
Members sheet is malformed. This is generally caused by manual edits. To resolve issues, make sure:
  1. 1st column [id], does not have any duplicate entries.
  2. 3rd column [role], does not have any invalid values.
  3. 1st (A) and 3rd (C) columns are [id] and [role].
  4. There are no rows with missing [id].
""", e);
        }
    }

    public async Task AddMember(SpreadsheetReference sheet, ulong userId, string displayName, int role, string? team)
    {
        await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
        {
            Requests = new[]
            {
                SheetUtils.AppendRow(sheet.MembersSheetId, new[]
                {
                    SheetUtils.StringCell(userId.ToString()),
                    SheetUtils.StringCell(displayName),
                    SheetUtils.StringDropdownCell(Roles[role], Roles),
                    SheetUtils.StringCell(team),
                })
            }
        }, sheet.SpreadsheetId).ExecuteAsync();
    }

    public async Task UpdateMemberRole(SpreadsheetReference sheet, int rowNumber, int role)
    {
        await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
        {
            Requests = new[]
            {
                SheetUtils.UpdateCells(sheet.MembersSheetId, rowNumber + 1, MembersRoleIndex,
                    SheetUtils.SingleRow(SheetUtils.StringDropdownCell(Roles[role], Roles))),
            }
        }, sheet.SpreadsheetId).ExecuteAsync();
    }

    public async Task RemoveMember(SpreadsheetReference sheet, int rowNumber)
    {
        await RemoveRow(sheet.SpreadsheetId, sheet.MembersSheetId, rowNumber);
    }

    public async Task RemoveRow(string spreadsheetId, int sheetId, int rowNumber)
    {
        await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
        {
            Requests = new[]
            {
                new Request()
                {
                    DeleteDimension = new DeleteDimensionRequest()
                    {
                        Range = new DimensionRange()
                        {
                            Dimension = "ROWS", SheetId = sheetId,
                            StartIndex = rowNumber + 1, EndIndex = rowNumber + 2,
                        }
                    }
                }
            }
        }, spreadsheetId).ExecuteAsync();
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
                MembersSheetId = (int)createBatch.Replies[0].AddSheet.Properties.SheetId!,
                ItemsSheetId = (int)createBatch.Replies[1].AddSheet.Properties.SheetId!,
                SubmissionsSheetId = (int)createBatch.Replies[2].AddSheet.Properties.SheetId!
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