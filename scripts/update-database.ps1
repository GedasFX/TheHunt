if ($args.Count -ne 0) {
    Write-Host "Usage: ./scripts/update-database.ps1";
    exit 1;
}

Set-Location "$PSScriptRoot/../"
dotnet-ef.exe database update --project "./TheHunt.Data/TheHunt.Data.csproj"