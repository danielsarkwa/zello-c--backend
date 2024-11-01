#!/usr/bin/env pwsh

Write-Host "🔍 Running StyleCop checks..." -ForegroundColor Cyan

# Clean previous builds
dotnet clean --verbosity quiet
Write-Host "✨ Cleaned previous builds" -ForegroundColor Gray

# Build with style checking enabled
$buildOutput = dotnet build /p:EnforceCodeStyleInBuild=true /clp:NoSummary --verbosity quiet 2>&1

# Filter and display style-related messages
$styleIssues = $buildOutput | Where-Object { 
    $_ -match "SA\d{4}" -or              # StyleCop rules
    $_ -match "error CS\d{4}" -or        # C# compiler errors
    $_ -match "warning CS\d{4}"          # C# compiler warnings
}

if ($styleIssues) {
    Write-Host "`n❌ Style issues found:" -ForegroundColor Red
    $styleIssues | ForEach-Object {
        Write-Host $_ -ForegroundColor Yellow
    }
} else {
    Write-Host "`n✅ No style issues found!" -ForegroundColor Green
}

# Return exit code based on build success
exit $LASTEXITCODE