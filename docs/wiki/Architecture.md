# Architecture

Block is a local-first orchestrator rather than a replacement virtual machine
for every embedded language.

## Execution pipeline

1. Parse the source and identify native logic, imports, and runtime blocks.
2. Validate the requested runtime and transferable state.
3. Prepare a temporary stage and launch the native process.
4. Capture output and returned state.
5. Merge compatible state and continue to the next stage.
6. Close the process tree and report success or failure.

The engine editions share the same core source and differ in the runtime and
tooling surface they expose. The Windows build is produced from `src/` by
`build.ps1`; the installer source is `Installer.cs`.

## Process isolation

Child processes are tracked as a group so a timed-out or failed stage does not
remain detached. Execution limits and serialization boundaries are guardrails,
not a substitute for reviewing the code being run.

## Extension architecture

The VS Code extension provides language recognition, snippets, commands, and
diagnostics. The Acode plugin provides mobile-editor integration. Both invoke
the local Block executable and therefore inherit the host runtime and trust
requirements.
