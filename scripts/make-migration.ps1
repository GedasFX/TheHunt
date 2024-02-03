if ($args.Count -ne 1) {
    Write-Host "Usage: ./scripts/make-migration.ps1 <MigrationName>";
    exit 1;
}

Set-Location "$PSScriptRoot/../"
dotnet-ef.exe migrations add --project "./src/TheHunt.Data/TheHunt.Data.csproj" $args[0]