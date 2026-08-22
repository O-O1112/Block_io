# Installation

## Choose an edition

| Edition | Best for | Typical file |
| --- | --- | --- |
| Block Lite | Lightweight Python, JavaScript, HTML, and native Block workflows | `.blkl` |
| Block Standard | Recommended general-purpose workflows | `.blk` |
| Block+ | Expanded runtime matrix and custom runtime definitions | `.blkp` |

The engine does not bundle every host runtime. Install Python, Node.js, PHP, Lua, or another runtime separately when a script uses it.

## Verify the installation

```powershell
block --version
block runtimes
block doctor
```

`runtimes` reports detected tools. `doctor` adds configuration and diagnostic information.

## Check before running

```powershell
block check .\workflow.blk
block run .\workflow.blk
```

The original file-only form remains supported:

```powershell
block .\workflow.blk
```

## Installation safety

Only run scripts and installers you trust. Release binaries should be verified against the published SHA256 manifest. PowerShell execution is disabled by default in new configurations. Review the [Security](Security) page before enabling additional runtimes.
