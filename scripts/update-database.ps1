if ($args.Count -ne 0) {
    Write-Host "Usage: ./scripts/update-database.ps1";
    exit 1;
}

Set-Location "$PSScriptRoot/../"
dotnet-ef.exe database update --project "./TheHunt.Domain/TheHunt.Domain.csproj" --startup-project "./TheHunt.Api/TheHunt.Api.csproj"