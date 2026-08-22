param(
    [string]$ReleaseDirectory = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'
$bin = Join-Path $ReleaseDirectory 'bin'
if (-not (Test-Path -LiteralPath $bin)) {
    # GitHub Pages publishes the release binaries from the repository root.
    # Local builds may keep them under .\bin, so support both layouts.
    $bin = $ReleaseDirectory
}
$expected = @('block.exe', 'block-lite.exe', 'block-plus.exe')
$failures = New-Object System.Collections.Generic.List[string]

foreach ($name in $expected) {
    $path = Join-Path $bin $name
    if (-not (Test-Path -LiteralPath $path)) { $failures.Add("missing $name"); continue }
    $version = (& $path --version 2>&1 | Out-String).Trim()
    if ($version -notmatch 'v2\.2\.0') { $failures.Add("$name is not v2.2.0: $version") }
}

$hashFile = Join-Path $bin 'SHA256SUMS.txt'
if (Test-Path -LiteralPath $hashFile) {
    foreach ($line in Get-Content -LiteralPath $hashFile) {
        if ($line -notmatch '^([0-9a-fA-F]{64})\s+(.+)$') { $failures.Add("invalid hash line: $line"); continue }
        $path = Join-Path $bin $Matches[2]
        if (-not (Test-Path -LiteralPath $path)) { $failures.Add("hash target missing: $($Matches[2])"); continue }
        $actual = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        if ($actual -ne $Matches[1]) { $failures.Add("hash mismatch: $($Matches[2])") }
    }
}

foreach ($package in @('block-language-2.2.0.vsix', 'acode-plugin-block-2.2.0.zip', 'BlockSetup-v2.2.0.exe')) {
    if (-not (Test-Path -LiteralPath (Join-Path $ReleaseDirectory $package))) { $failures.Add("missing package: $package") }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host 'Block Engine v2.2.0 release verification passed.'
