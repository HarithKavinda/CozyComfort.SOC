# CozyComfort - Run API + Web together (multiple projects)
# API: http://localhost:5023 | Web: http://localhost:5250

$api = Start-Process -FilePath "dotnet" -ArgumentList "run","--project","CozyComfort.API\CozyComfort.API.csproj" -PassThru -WindowStyle Normal
$web = Start-Process -FilePath "dotnet" -ArgumentList "run","--project","CozyComfort.Web\CozyComfort.Web.csproj" -PassThru -WindowStyle Normal

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CozyComfort - Both projects started" -ForegroundColor Cyan
Write-Host "  API:  http://localhost:5023" -ForegroundColor Green
Write-Host "  Web:  http://localhost:5250" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Press any key to stop both..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Stop-Process -Id $api.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $web.Id -Force -ErrorAction SilentlyContinue
