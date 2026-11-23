param(
    [Parameter(Mandatory)]
    [ValidateSet('Compile', 'EditMode', 'PlayMode')]
    [string] $Phase
)

$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$versionFile = Join-Path $project 'ProjectSettings/ProjectVersion.txt'
$versionMatch = Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersionWithRevision: (\S+) \(([0-9a-f]+)\)$'
if (-not $versionMatch) {
    throw 'ProjectVersion.txt must identify the exact Unity Editor version and revision.'
}
$version = $versionMatch.Matches[0].Groups[1].Value
$revision = $versionMatch.Matches[0].Groups[2].Value
$editor = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe"
if (-not (Test-Path -LiteralPath $editor -PathType Leaf)) {
    throw "Unity $version is missing. Configure UNITY_CI_RUNNER with a Windows runner that has the project Editor installed and licensed."
}

$results = Join-Path $project 'Logs/UnityCI'
New-Item -ItemType Directory -Path $results -Force | Out-Null
$rawLog = Join-Path $project "Logs/UnityCI-$Phase.raw.log"
$log = Join-Path $results "$Phase.log"
$report = Join-Path $results "$Phase.xml"
foreach ($path in @($rawLog, $log, $report)) {
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        Remove-Item -LiteralPath $path
    }
}

$unityArguments = @(
    '-batchmode', '-nographics', '-buildTarget', 'Win64',
    '-projectPath', ('"{0}"' -f $project),
    '-logFile', ('"{0}"' -f $rawLog)
)

if ($Phase -eq 'Compile') {
    $unityArguments += '-quit'
} else {
    # Unity's test runner exits the Editor after the report is written.
    $unityArguments += @('-runTests', '-testPlatform', $Phase, '-testResults', ('"{0}"' -f $report))
}

$process = Start-Process -FilePath $editor -ArgumentList $unityArguments -WindowStyle Hidden -Wait -PassThru
if (Test-Path -LiteralPath $rawLog -PathType Leaf) {
    # Actions artifacts can be public. Exclude licensing diagnostics from the published log.
    Get-Content -LiteralPath $rawLog | Where-Object { $_ -notmatch '^\[Licensing::' } |
        Set-Content -LiteralPath $log -Encoding utf8
}
if ($process.ExitCode -ne 0) {
    if (Test-Path -LiteralPath $log) {
        Get-Content -LiteralPath $log -Tail 60
    }
    throw "Unity $Phase exited with code $($process.ExitCode). See $Phase.log."
}

$expectedVersion = [regex]::Escape("$version ($revision)")
if (-not (Test-Path -LiteralPath $log -PathType Leaf) -or
    -not (Select-String -LiteralPath $log -Pattern "Version is '${expectedVersion}[ ']" -Quiet)) {
    throw "Unity $Phase did not report the expected Editor revision $version ($revision)."
}
if (Select-String -LiteralPath $log -Pattern '\berror CS\d{4}\b|Scripts have compiler errors' -Quiet) {
    throw "Unity $Phase reported C# compilation errors. See $Phase.log."
}

if ($Phase -ne 'Compile') {
    if (-not (Test-Path -LiteralPath $report -PathType Leaf)) {
        throw "Unity $Phase did not produce an NUnit XML report."
    }
    $testRun = ([xml](Get-Content -LiteralPath $report -Raw)).'test-run'
    if ($testRun.result -ne 'Passed' -or [int]$testRun.passed -lt 1 -or [int]$testRun.failed -ne 0) {
        throw "Unity $Phase did not pass. See $Phase.xml and $Phase.log."
    }
    Write-Host "$Phase passed $($testRun.passed) tests."
} else {
    Write-Host "Unity $version ($revision) compiled successfully."
}
