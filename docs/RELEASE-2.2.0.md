# Block Engine v2.2.0 release

This release is built for Windows 10/11 and publishes the Lite, Standard, and
Block+ engine editions together with the Windows installer and editor plugins.

## Reproducible release flow

Run the complete release pipeline from a Windows checkout:

```powershell
powershell -ExecutionPolicy Bypass -File .\build-release.ps1
powershell -ExecutionPolicy Bypass -File .\tests\Test-BlockEngine.ps1 -EngineDirectory .
powershell -ExecutionPolicy Bypass -File .\verify-release.ps1
```

The pipeline compiles the engine editions, packages matching engine ZIPs,
embeds those ZIPs into the installer, packages both extensions, and generates
`SHA256SUMS.txt` for every published artifact.

## Published artifacts

- `BlockSetup-v2.2.0.exe` — versioned Windows installer.
- `BlockSetup.exe` — stable download alias for the installer.
- `block-lite.zip` — Lite engine bundle.
- `block.zip` — Standard engine bundle.
- `block-plus.zip` — Block+ engine bundle.
- `block-language-2.2.0.vsix` — VS Code extension.
- `acode-plugin-block-2.2.0.zip` — Acode plugin.
- `SHA256SUMS.txt` — SHA-256 checksums for all published artifacts.

The installer can install the core engine even if an optional runtime package
cannot be downloaded. Missing optional runtimes are reported for later manual
installation.

## Verification requirements

Before publishing, `verify-release.ps1` must pass. It checks engine versions,
required files, all release hashes, exact engine-bundle contents, plugin
manifests and licenses, and the stable installer alias.

The GitHub Actions workflow repeats the build and smoke tests on a clean
Windows runner for pushes and pull requests.
