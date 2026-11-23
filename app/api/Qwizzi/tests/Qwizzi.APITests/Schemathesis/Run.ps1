$ErrorActionPreference = "Stop"

$BaseUrl = "[https://localhost:7275](https://localhost:7275)"
$SchemaPath = Join-Path $PSScriptRoot "openapi.json"
$ReportRoot = Join-Path $PSScriptRoot "Schemathesis-report"

$Tags = @("Account", "AdminUsers", "Auth")
$Phases = @("examples", "coverage", "fuzzing", "stateful")

if (-not (Test-Path -LiteralPath $SchemaPath)) {
    throw "OpenAPI schema not found: $SchemaPath"
}

if (-not (Get-Command schemathesis -ErrorAction SilentlyContinue)) {
    throw "Schemathesis is unavailable. Activate the virtual environment first."
}

# Preserve non-zero exit codes without stopping the entire loop
# on PowerShell versions that support this preference.
$PSNativeCommandUseErrorActionPreference = $false

$Results = @()

foreach ($Tag in $Tags) {
    foreach ($Phase in $Phases) {
        $ReportPath = Join-Path (Join-Path $ReportRoot $Tag) $Phase

        New-Item -ItemType Directory -Path $ReportPath -Force | Out-Null

        Write-Host ""
        Write-Host "=================================================="
        Write-Host "Tag: $Tag | Phase: $Phase"
        Write-Host "=================================================="

        $CliArguments = @(
            "run"
            $SchemaPath
            "--url"
            $BaseUrl
            "--include-tag"
            $Tag
            "--phases"
            $Phase
            "--max-examples"
            "20"
            "--report"
            "html"
            "--report-html-path"
            $ReportPath
        )

        & schemathesis @CliArguments
        $RunExitCode = $LASTEXITCODE

        $Results += [PSCustomObject]@{
            Tag = $Tag
            Phase = $Phase
            ExitCode = $RunExitCode
            ReportDirectory = $ReportPath
        }
    }
}

Write-Host ""
Write-Host "Run summary"
$Results | Format-Table Tag, Phase, ExitCode -AutoSize

if (@($Results | Where-Object { $_.ExitCode -ne 0 }).Count -gt 0) {
    Write-Host "At least one run returned a non-zero exit code."
    Write-Host "Review the console output and generated reports."
    exit 1
}

Write-Host "All runs returned exit code 0."
exit 0
