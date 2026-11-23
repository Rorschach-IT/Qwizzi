# Installation

1. Navigate to the test directory:

   ```powershell
   cd app/api/Qwizzi/tests/Qwizzi.APITests/Schemathesis
   ```

2. Create a Python virtual environment:

   ```powershell
   py -m venv .venv
   ```

3. Activate the environment:

   ```powershell
   .\.venv\Scripts\Activate.ps1
   ```

4. Update pip:

   ```powershell
   python -m pip install --upgrade pip
   ```

5. Install dependencies:

   ```powershell
   python -m pip install --upgrade schemathesis pytest
   ```

   `pytest` is intended for future Python-based tests. The CLI commands below do not require a pytest test file.

6. Check the installed version and available options:

   ```powershell
   schemathesis --version
   schemathesis run --help
   ```

   The installed version must support HTML reporting and the `--report-html-path` option.

# Before running tests

- Start Qwizzi.Api at `https://localhost:7275`.
- Place the OpenAPI document in this directory as `openapi.json`.
- Use an isolated test database with disposable accounts.
- The commands below disable TLS certificate verification with `--tls-verify=false` for local development only.
- Confirm that individual operations in `openapi.json` have the correct tags:
  - `Account`
  - `AdminUsers`
  - `Auth`

## Local HTTPS configuration

Every test run below includes:

```text
--tls-verify=false
```

Use this setting only for your controlled local test environment. Do not use it for production or untrusted remote servers.

For environments with a correctly trusted certificate, remove this option to retain TLS certificate verification.

Declaring tags only in the top-level `tags` array is not enough. Each operation must reference its tag, for example:

```json
{
  "paths": {
    "/api/example": {
      "get": {
        "tags": ["Account"],
        "responses": {
          "200": {
            "description": "Success"
          }
        }
      }
    }
  }
}
```

This is an illustrative fragment, not the complete Qwizzi schema.

## Authentication and CSRF

The commands below select operations and generate reports. They do not automatically:

- Log in as an administrator or a regular user.
- Read cookies from Postman or Chrome.
- Fetch and refresh CSRF tokens.
- Recreate accounts deleted by a previous test.
- Restore passwords, email addresses, or account statuses.

Authenticated testing requires additional session preparation, for example through a Python integration.

Without valid authentication and CSRF handling, many requests may return `401`, `403`, or `400`. Such responses do not prove that the protected business logic was tested successfully.

Do not use your main administrator account as disposable test data.

# Usage

Run the following commands from the directory containing `openapi.json`.

## Health check

Test only the `GET /api/health` operation:

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-method GET `
  --include-path /api/health `
  --phases examples,coverage `
  --max-examples 10
```

This command does not test the whole API. It selects only the specified method and path.

If your health endpoint has a different path, update `--include-path`.

## Console output and HTML reports

HTML reporting supplements the normal console output.

During a run, Schemathesis still displays progress, failures, warnings, and a summary in the terminal. The HTML report provides a separate view that can be opened after the run.

The commands below save reports under:

```text
Schemathesis-report/<tag>/<phase>/index.html
```

Running the same tag and phase again reuses the same report directory. Copy previous reports elsewhere if you need to retain them.

# Testing phases

| Phase      | Practical meaning                                                                                                                    |
| ---------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| `examples` | Runs explicit examples defined in the OpenAPI schema. If no usable examples exist, this phase may have little or nothing to execute. |
| `coverage` | Generates deterministic cases targeting schema constraints, boundary values, and input combinations.                                 |
| `fuzzing`  | Generates varied inputs using property-based testing.                                                                                |
| `stateful` | Chains API operations and passes information between requests when operation relationships can be established.                       |

The correct phase name is `stateful`, not `statefull`.

## Generation limits

The examples below use:

```text
--max-examples 20
```

Its meaning depends on the phase:

- `fuzzing`: maximum number of generated examples per operation.
- `stateful`: maximum number of generated sequences, not a total HTTP request limit.
- `examples` and `coverage`: this setting does not limit their predetermined test cases.

Four phases mean four separate test runs per tag, not exactly four HTTP requests or four generated test cases.

# Account tests

These runs select operations tagged `Account`.

## Account — examples

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag Account `
  --phases examples `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\Account\examples
```

## Account — coverage

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag Account `
  --phases coverage `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\Account\coverage
```

## Account — fuzzing

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag Account `
  --phases fuzzing `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\Account\fuzzing
```

## Account — stateful

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag Account `
  --phases stateful `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\Account\stateful
```

# AdminUsers tests

These runs select operations tagged `AdminUsers`.

For meaningful authorized testing, prepare an administrator session separately and use disposable target accounts.

## AdminUsers — examples

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag AdminUsers `
  --phases examples `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\AdminUsers\examples
```

## AdminUsers — coverage

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag AdminUsers `
  --phases coverage `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\AdminUsers\coverage
```

## AdminUsers — fuzzing

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag AdminUsers `
  --phases fuzzing `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\AdminUsers\fuzzing
```

## AdminUsers — stateful

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag AdminUsers `
  --phases stateful `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\AdminUsers\stateful
```

# Auth tests

These runs select operations tagged `Auth`.

Registration, login, and logout can change application state and trigger rate limits. Restore test data as needed between runs.

## Auth — examples

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag Auth `
  --phases examples `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\Auth\examples
```

## Auth — coverage

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag Auth `
  --phases coverage `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\Auth\coverage
```

## Auth — fuzzing

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag Auth `
  --phases fuzzing `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\Auth\fuzzing
```

## Auth — stateful

```powershell
schemathesis run .\openapi.json `
  --url https://localhost:7275 `
  --tls-verify=false `
  --include-tag Auth `
  --phases stateful `
  --max-examples 20 `
  --report html `
  --report-html-path .\Schemathesis-report\Auth\stateful
```

# Run all 12 combinations

Save the following script as:

```text
app/api/Qwizzi/tests/Qwizzi.APITests/Schemathesis/run-all.ps1
```

The script runs each tag and phase separately, keeps the standard console output visible, and prints a summary of exit codes.

It continues to the next combination after a failed Schemathesis run. It does not reset the database or prepare authenticated sessions between runs.

TLS certificate verification is disabled for every run using `--tls-verify=false`.

```powershell
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
            "--tls-verify=false"
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
```

Run the script from the Schemathesis directory:

```powershell
.\run-all.ps1
```

A non-zero exit code can indicate a test failure, a configuration problem, or another execution issue. Review the actual message rather than interpreting every non-zero result as a backend bug.

# Report directory structure

After successful report generation, the structure is:

```text
Schemathesis-report/
├── Account/
│   ├── examples/
│   │   └── index.html
│   ├── coverage/
│   │   └── index.html
│   ├── fuzzing/
│   │   └── index.html
│   └── stateful/
│       └── index.html
├── AdminUsers/
│   ├── examples/
│   │   └── index.html
│   ├── coverage/
│   │   └── index.html
│   ├── fuzzing/
│   │   └── index.html
│   └── stateful/
│       └── index.html
└── Auth/
    ├── examples/
    │   └── index.html
    ├── coverage/
    │   └── index.html
    ├── fuzzing/
    │   └── index.html
    └── stateful/
        └── index.html
```

Open a report in the default browser:

```powershell
Start-Process ".\Schemathesis-report\Account\fuzzing\index.html"
```

Reports may not be generated if execution fails before the test run initializes, for example because of invalid CLI options.

# Stateful testing limitations

Selecting the `stateful` phase does not automatically define a business workflow such as:

```text
Register → Login → Get CSRF → Update account → Logout
```

Schemathesis needs usable relationships between operations to build meaningful sequences.

Filtering each run to a single tag can exclude operations required for a cross-tag workflow. For example, an `Account` run may need authentication provided by an operation under `Auth`.

If a stateful run is skipped or cannot build useful sequences, inspect operation relationships and authentication setup. Do not interpret an empty or skipped run as successful coverage.

For complete business workflows, add dedicated authenticated integration tests or a customized stateful setup.

# Troubleshooting

| Problem                                      | What to check                                                                                                                           |
| -------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| No operations selected                       | Verify tags on individual operations and exact tag spelling.                                                                            |
| No examples executed                         | Add usable examples to the OpenAPI schema if example-based testing is required.                                                         |
| Stateful testing has no useful sequences     | Check relationships between operations and whether filtering excludes prerequisites.                                                    |
| Responses are mostly `401` or `403`          | Prepare a valid session with the appropriate role.                                                                                      |
| Modifying requests return CSRF-related `400` | Provide the required CSRF cookie and request token.                                                                                     |
| Responses are `429`                          | Check API rate limits and separate authentication tests from broader runs.                                                              |
| Local HTTPS certificate verification error   | Confirm that `--tls-verify=false` is present in the local test command. For normal verified HTTPS, configure certificate trust instead. |
| Connection refused or timeout                | Verify that the backend is running and the URL and port are correct. Disabling TLS verification does not fix connectivity problems.     |
| HTML option is unavailable                   | Check the installed Schemathesis version and `schemathesis run --help`.                                                                 |

# Repository hygiene

Do not commit virtual environments, generated reports, or credentials.

Example `.gitignore` entries:

```gitignore
.venv/
Schemathesis-report/
__pycache__/
.pytest_cache/
.env
```

Review reports before sharing them. They can contain request data, account information, and details about the tested application.
