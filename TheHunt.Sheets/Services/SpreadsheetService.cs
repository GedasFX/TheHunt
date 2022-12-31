using System.Net;
using Discord;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using TheHunt.Application;
using TheHunt.Data.Models;
using TheHunt.Sheets.Models;
using TheHunt.Sheets.Utils;

namespace TheHunt.Sheets.Services;

public class SpreadsheetService
{
    private readonly SheetsService _service;

    public SpreadsheetService(string googleCredentialsFile)
    {
        _service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = GoogleCredential.FromFile(googleCredentialsFile)
        });
    }

    public async Task<IReadOnlyList<CompetitionUser>> GetMembers(SheetsRef sheet)
    {
        try
        {
            var data = await _service.Spreadsheets.Values.BatchGetByDataFilter(new BatchGetValuesByDataFilterRequest()
            {
                DataFilters = new[]
                {
                    new DataFilter { GridRange = new GridRange { SheetId = sheet.Sheets.Members, StartRowIndex = 1, StartColumnIndex = 0, EndColumnIndex = 1 } }
                }
            }, sheet.SpreadsheetId).ExecuteAsync();

            return (IReadOnlyList<CompetitionUser>?)data.ValueRanges[0].ValueRange.Values?.Select((t, i) => (List: t, Idx: i)).Where(t => t.List?.Count > 0)
                       .Select(t => new CompetitionUser
                       {
                           UserId = ulong.Parse((string)t.List[0]), RowIdx = t.Idx,
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

    public async Task AddMember(SheetsRef sheet, ulong userId, string displayName, string? team)
    {
        await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
        {
            Requests = new[]
            {
                SheetUtils.AppendRow(sheet.Sheets.Members, new[]
                {
                    SheetUtils.StringCell(userId.ToString()),
                    SheetUtils.StringCell(displayName),
                    SheetUtils.StringCell(team),
                })
            }
        }, sheet.SpreadsheetId).ExecuteAsync();
    }

    public async Task RemoveMember(SheetsRef sheet, int rowNumber)
    {
        await RemoveRow(sheet.SpreadsheetId, sheet.Sheets.Members, rowNumber);
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

    public async Task AddSubmission(SheetsRef sheetsRef, ulong submissionId, string submissionUrl, ulong submitterId, ulong verifierId, string? imageUrl,
        DateTime date, string? item, int bonus)
    {
        await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
        {
            Requests = new[]
            {
                SheetUtils.AppendRow(sheetsRef.Sheets.Submissions, new[]
                {
                    SheetUtils.FormulaCell($"=HYPERLINK(\"{submissionUrl}\", \"{submissionId}\")"),
                    SheetUtils.FormulaCell(imageUrl != null ? $"=HYPERLINK(\"{imageUrl}\", IMAGE(\"{imageUrl}\"))" : null),
                    SheetUtils.FormulaCell(date.ToString("=DATE(yyyy,MM,dd) + TI\\ME(HH,mm,ss)")),
                    SheetUtils.StringCell(item),
                    SheetUtils.FormulaCell(
                        $"=IF(ISBLANK(VLOOKUP(INDIRECT(\"R[0]C6\", FALSE), '__{sheetsRef.SheetName}_members'!A$2:D, 4, FALSE)), INDIRECT(\"R[0]C7\", FALSE), VLOOKUP(INDIRECT(\"R[0]C6\", FALSE), '__{sheetsRef.SheetName}_members'!A$2:D, 4, FALSE))"),
                    SheetUtils.StringCell(submitterId.ToString()),
                    SheetUtils.FormulaCell($"=VLOOKUP(INDIRECT(\"R[0]C6\", FALSE), '__{sheetsRef.SheetName}_members'!A$2:B, 2, FALSE)"),
                    SheetUtils.StringCell(verifierId.ToString()),
                    SheetUtils.FormulaCell($"=VLOOKUP(INDIRECT(\"R[0]C8\", FALSE), '__{sheetsRef.SheetName}_members'!A$2:B, 2, FALSE)"),
                    SheetUtils.FormulaCell($"=VLOOKUP(INDIRECT(\"R[0]C4\", FALSE), '__{sheetsRef.SheetName}_items'!A$2:C, 2, FALSE)"),
                    SheetUtils.NumberCell(bonus),
                    SheetUtils.FormulaCell("=IFNA(INDIRECT(\"R[0]C[-2]\", FALSE)) + INDIRECT(\"R[0]C[-1]\", FALSE)"),
                })
            }
        }, sheetsRef.SpreadsheetId).ExecuteAsync();
    }

    public async Task<SheetsRef> CreateCompetition(string spreadsheetId, string sheetName)
    {
        try
        {
            var createBatch = await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
            {
                Requests = new[]
                {
                    new Request { AddSheet = CreateSheet(sheetName, "overview", 3) },
                    new Request { AddSheet = CreateSheet(sheetName, "config", frozenRowCount: 0) },
                    new Request { AddSheet = CreateSheet(sheetName, "members", 3) },
                    new Request { AddSheet = CreateSheet(sheetName, "items", 3) },
                    new Request { AddSheet = CreateSheet(sheetName, "submissions", 12) },
                }
            }, spreadsheetId).ExecuteAsync();

            await _service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest()
            {
                Requests = new[]
                {
                    new Request { UpdateCells = AddHeaderRow(createBatch, 2, "Id", "Name", "Team") },
                    new Request { UpdateCells = AddHeaderRow(createBatch, 3, "Item Name", "Points Value", "Part of Set") },
                    new Request
                    {
                        UpdateCells = AddHeaderRow(createBatch, 4, "Id", "Image", "Date", "Item", "Team", "Submitter Id", "Submitter", "Verifier Id",
                            "Verifier", "Points Item", "Points Bonus", "Points Total")
                    },
                    new Request()
                    {
                        UpdateCells = new UpdateCellsRequest()
                        {
                            Start = new GridCoordinate() { SheetId = createBatch.Replies[1].AddSheet.Properties.SheetId, RowIndex = 0, ColumnIndex = 0 },
                            Fields = "*",
                            Rows = new[]
                            {
                                new RowData()
                                {
                                    Values = new[]
                                    {
                                        new CellData()
                                        {
                                            UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } },
                                            UserEnteredValue = new ExtendedValue { StringValue = "/competitions show" }
                                        }
                                    }
                                },
                                new RowData()
                                {
                                    Values = new[]
                                    {
                                        new CellData()
                                        {
                                            UserEnteredValue = new ExtendedValue
                                            {
                                                StringValue =
                                                    "(Up to) 20 rows in the table below can be configured to show data when users run /competitions show."
                                            }
                                        }
                                    }
                                },
                                new RowData()
                                {
                                    Values = new[]
                                    {
                                        new CellData()
                                        {
                                            UserEnteredValue = new ExtendedValue
                                            {
                                                StringValue =
                                                    "Feel free to edit the provided examples. They are here to show potential ways of using this section."
                                            }
                                        }
                                    }
                                },
                                new RowData(),
                                new RowData()
                                {
                                    Values = new[]
                                    {
                                        new CellData()
                                        {
                                            UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } },
                                            UserEnteredValue = new ExtendedValue { StringValue = "Section Title" }
                                        },
                                        new CellData()
                                        {
                                            UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } },
                                            UserEnteredValue = new ExtendedValue { StringValue = "Value" }
                                        },
                                        new CellData()
                                        {
                                            UserEnteredFormat = new CellFormat() { TextFormat = new TextFormat() { Bold = true } },
                                            UserEnteredValue = new ExtendedValue { StringValue = "Use Short Form?" }
                                        }
                                    }
                                },
                                new RowData()
                                {
                                    Values = new[]
                                    {
                                        new CellData()
                                        {
                                            UserEnteredValue = new ExtendedValue { StringValue = "Participants Count" }
                                        },
                                        new CellData()
                                        {
                                            UserEnteredValue = new ExtendedValue { FormulaValue = $"=COUNTA('__{sheetName}_members'!A2:A)" }
                                        },
                                        new CellData()
                                        {
                                            UserEnteredValue = new ExtendedValue { BoolValue = true }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }, spreadsheetId).ExecuteAsync();

            return new SheetsRef
            {
                SpreadsheetId = spreadsheetId, SheetName = sheetName,
                Sheets = new SheetsRef.Sheet()
                {
                    Overview = (int)createBatch.Replies[0].AddSheet.Properties.SheetId!,
                    Config = (int)createBatch.Replies[1].AddSheet.Properties.SheetId!,
                    Members = (int)createBatch.Replies[2].AddSheet.Properties.SheetId!,
                    Items = (int)createBatch.Replies[3].AddSheet.Properties.SheetId!,
                    Submissions = (int)createBatch.Replies[4].AddSheet.Properties.SheetId!,
                },
            };
        }
        catch (GoogleApiException e) when (e is { HttpStatusCode: HttpStatusCode.Forbidden, Error.Message: "The caller does not have permission" })
        {
            throw new EntityValidationException(
                "The bot does not have permission to edit this spreadsheet.\n" +
                "Send an invitation to `the-hunt@the-hunt-373015.iam.gserviceaccount.com` with `Editor` permissions.", e);
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

    public async Task<IReadOnlyList<EmbedFieldBuilder>> GetCompetitionShowFields(SheetsRef sheet)
    {
        try
        {
            var data = await _service.Spreadsheets.Values.BatchGetByDataFilter(new BatchGetValuesByDataFilterRequest()
            {
                DataFilters = new[]
                {
                    new DataFilter
                    {
                        GridRange = new GridRange
                        {
                            SheetId = sheet.Sheets.Config,
                            StartRowIndex = 5, EndRowIndex = 25,
                            StartColumnIndex = 0, EndColumnIndex = 3
                        }
                    }
                }
            }, sheet.SpreadsheetId).ExecuteAsync();

            return (IReadOnlyList<EmbedFieldBuilder>?)data.ValueRanges[0].ValueRange.Values?.Where(t => t?.Count > 0)
                       .Select(t => new EmbedFieldBuilder()
                       {
                           Name = t.ElementAtOrDefault(0) as string,
                           Value = t.ElementAtOrDefault(1) as string,
                           IsInline = !bool.TryParse(t.ElementAtOrDefault(2) as string, out var inline) || inline,
                       }).ToList()
                   ?? Array.Empty<EmbedFieldBuilder>();
        }
        catch (Exception e)
        {
            throw new EntityValidationException("""
Configuration sheet is malformed. This is generally caused by manual edits. To resolve issues, make sure:
  1. Section Title is not empty and no more than 256 characters long.
  2. Value is not empty and no more than 1024 characters in length.
""", e);
        }
    }

    private static AddSheetRequest CreateSheet(string sheetName, string name, int? columnCount = null, int? frozenRowCount = 1)
    {
        return new AddSheetRequest()
        {
            Properties = new SheetProperties()
            {
                Title = $"__{sheetName}_{name}",
                GridProperties = new GridProperties() { FrozenRowCount = frozenRowCount, ColumnCount = columnCount },
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