# Repository layout

This repository serves two purposes: it is the Block Engine source repository
and the GitHub Pages download site. The root-level website files and release
artifacts are therefore intentionally kept in place so existing download URLs
continue to work.

## Main directories

| Path | Purpose |
| --- | --- |
| `src/` | Block Engine C# source shared by the Lite, Standard, and Plus builds. |
| `block-vscode-extension/` | VS Code language extension source and manifest. |
| `acode-plugin-block/` | Acode plugin source and manifest. |
| `docs/` | Maintainer documentation and repository notes. |
| `docs/wiki/` | Version-controlled Markdown Wiki source. |
| `tests/` | Windows PowerShell engine smoke tests. |

## Root-level files

| Path | Purpose |
| --- | --- |
| `Installer.cs` | Windows installer source. |
| `build.ps1` | Builds the three engine editions into `bin/`. |
| `package-extensions.ps1` | Packages the VS Code and Acode extensions. |
| `package-engine.ps1` | Packages each built engine into its matching ZIP. |
| `build-installer.ps1` | Rebuilds the installer with the three engine ZIP resources. |
| `build-release.ps1` | Runs the complete build, packaging, hashing, and verification flow. |
| `verify-release.ps1` | Verifies versions, hashes, and required release files. |
| `index.html`, `downloads.html`, `wiki*.html`, `styles.css`, `script.js` | GitHub Pages site. |
| `block.exe`, `block-lite.exe`, `block-plus.exe` | Published engine binaries. |
| `BlockSetup-v2.2.0.exe` | Published Windows installer. |
| `block-language-2.2.0.vsix` | Published VS Code extension package. |
| `acode-plugin-block-2.2.0.zip` | Published Acode plugin package. |
| `SHA256SUMS.txt` | SHA-256 checksums for the published engine binaries. |

## Project governance

| Path | Purpose |
| --- | --- |
| `LICENSE` | MIT license for the project. |
| `CONTRIBUTING.md` | Development and pull request workflow. |
| `SECURITY.md` | Private vulnerability reporting guidance. |
| `CODE_OF_CONDUCT.md` | Community participation standards. |
| `.github/ISSUE_TEMPLATE/` | Structured bug and feature reports. |

Historical packages remain at the root for backwards compatibility. Before
moving or removing any release artifact, search the site files for references
and verify the resulting GitHub Pages URLs.

## Maintainer workflow

From a Windows checkout:

```powershell
powershell -ExecutionPolicy Bypass -File .\build-release.ps1
powershell -ExecutionPolicy Bypass -File .\tests\Test-BlockEngine.ps1 -EngineDirectory .
powershell -ExecutionPolicy Bypass -File .\verify-release.ps1
```

The verification script accepts both the local `bin/` build layout and the
published repository-root layout.
