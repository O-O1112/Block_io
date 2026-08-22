# Security

## Treat scripts as code

Block files can invoke local language runtimes. Never run an unknown `.blk`, `.blkl`, or `.blkp` file merely because it looks like a data document.

## Runtime boundaries

- PowerShell is disabled by default in new configurations.
- Runtime processes are subject to execution timeouts and output limits.
- Imports are restricted to approved project boundaries.
- Local server and API features are intended for local development.
- Shared state should contain plain data only.

These controls reduce accidental exposure; they do not make untrusted code safe. Review every runtime block and custom runtime definition before execution.

## Release verification

Download releases from the official repository, compare SHA256 values with `SHA256SUMS.txt`, and keep antivirus protection enabled. If a security product reports a release, do not whitelist it automatically. Preserve the detection name and file path, quarantine the file, and report the result through the repository's security process.

## Reporting

Do not publish exploit details in a public issue. Use the repository's [security policy](https://github.com/O-O1112/Block_lang/security/policy) when available.
