# Installation

## Supported editions

- **Block Lite (`.blkl`)** — the smallest runtime surface for lightweight scripts.
- **Block Standard (`.blk`)** — the recommended general-purpose edition.
- **Block+ (`.blkp`)** — expanded runtime and tooling support.

The Windows installer is published as `BlockSetup-v2.2.0.exe` in the repository
root and on the [download page](../../downloads.html). Existing runtimes are
detected before optional runtime installation is attempted.

## Install and verify

1. Run the installer and choose an install directory.
2. Select an engine edition. Standard is the recommended default.
3. Select optional runtimes required by your scripts.
4. Open a new PowerShell or Command Prompt window.
5. Verify the installation:

   ```powershell
   block --version
   ```

The core engine can complete installation even when an optional runtime package
cannot be downloaded. The installer reports those optional failures so they
can be installed later and the installer can be run again.

## Runtime requirements

Block delegates language blocks to local runtimes; it does not replace them.
Install and verify the runtimes required by the selected tags, then confirm
that they are available on `PATH`. Never run scripts from an untrusted source:
language blocks execute native programs on the host machine.
