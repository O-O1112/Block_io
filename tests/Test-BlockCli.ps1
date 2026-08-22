param(
    [string]$EngineDirectory = (Join-Path $PSScriptRoot '..\bin')
)

$ErrorActionPreference = 'Stop'
$EngineDirectory = [IO.Path]::GetFullPath($EngineDirectory)
$failures = New-Object System.Collections.Generic.List[string]
$tempRoot = Join-Path ([IO.Path]::GetTempPath()) ('block-cli-tests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

function Invoke-Block([string]$Executable, [string[]]$Arguments) {
    $path = Join-Path $EngineDirectory $Executable
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing test executable: $path" }
    $output = (& $path @Arguments 2>&1 | Out-String).Trim()
    [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = $output }
}

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (-not $Condition) { $failures.Add($Message) }
}

try {
    $nativePath = Join-Path $tempRoot 'native cli test.blk'
    @'
message = "cli-ok"
print(message)
'@ | Set-Content -LiteralPath $nativePath -Encoding UTF8

    foreach ($executable in @('block.exe', 'block-lite.exe', 'block-plus.exe')) {
        $runtimes = Invoke-Block $executable @('runtimes')
        Assert-Condition ($runtimes.ExitCode -eq 0) "$executable runtimes failed: $($runtimes.Output)"
        Assert-Condition ($runtimes.Output -match 'Runtime Diagnostics') "$executable runtimes output was incomplete: $($runtimes.Output)"

        $info = Invoke-Block $executable @('info')
        Assert-Condition ($info.ExitCode -eq 0) "$executable info failed: $($info.Output)"
        Assert-Condition ($info.Output -match 'Edition:') "$executable info output was incomplete: $($info.Output)"

        $configPath = Invoke-Block $executable @('config', 'path')
        Assert-Condition ($configPath.ExitCode -eq 0) "$executable config path failed: $($configPath.Output)"
        Assert-Condition ($configPath.Output -match 'config\.json') "$executable config path output was unexpected: $($configPath.Output)"

        $check = Invoke-Block $executable @('check', $nativePath)
        Assert-Condition ($check.ExitCode -eq 0) "$executable check failed: $($check.Output)"
        Assert-Condition ($check.Output -match 'Syntax Check Passed') "$executable check output was unexpected: $($check.Output)"

        $run = Invoke-Block $executable @('run', $nativePath)
        Assert-Condition ($run.ExitCode -eq 0) "$executable run failed: $($run.Output)"
        Assert-Condition ($run.Output -match 'cli-ok') "$executable run did not execute the native script: $($run.Output)"
    }

    $projectPath = Join-Path $tempRoot 'project'
    $project = Invoke-Block 'block.exe' @('project', 'init', $projectPath, 'cli-project')
    Assert-Condition ($project.ExitCode -eq 0) "block project alias failed: $($project.Output)"
    Assert-Condition (Test-Path (Join-Path $projectPath 'block.project.json')) 'project alias did not create block.project.json'
} catch {
    $failures.Add($_.Exception.Message)
} finally {
    if (Test-Path -LiteralPath $tempRoot) { Remove-Item -LiteralPath $tempRoot -Recurse -Force }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host 'Block CLI tests passed.'
