param(
    [string]$EngineDirectory = (Join-Path $PSScriptRoot 'bin'),
    [string]$OutputDirectory = $PSScriptRoot
)

$ErrorActionPreference = 'Stop'
$EngineDirectory = [IO.Path]::GetFullPath($EngineDirectory)
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$packages = @(
    @{ Archive = 'block-lite.zip'; Executable = 'block-lite.exe' },
    @{ Archive = 'block.zip'; Executable = 'block.exe' },
    @{ Archive = 'block-plus.zip'; Executable = 'block-plus.exe' }
)

foreach ($package in $packages) {
    $executable = Join-Path $EngineDirectory $package.Executable
    if (-not (Test-Path -LiteralPath $executable)) {
        throw "Missing engine executable: $executable"
    }

    $archive = Join-Path $OutputDirectory $package.Archive
    if (Test-Path -LiteralPath $archive) { Remove-Item -LiteralPath $archive -Force }
    Compress-Archive -LiteralPath $executable -DestinationPath $archive -CompressionLevel Optimal
    Write-Host "Created $archive"
}
