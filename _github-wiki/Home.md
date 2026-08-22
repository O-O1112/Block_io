# Block Language

Block is a local-first polyglot programming engine. It lets one readable script coordinate Python, JavaScript, Lua, PHP, SQL, PowerShell, and other host runtimes while passing explicit serializable state between stages.

## Why Block exists

Many useful workflows are already written in more than one language. Block keeps those tools instead of forcing a rewrite:

- one entry point for the whole workflow;
- explicit language boundaries such as `<py>`, `<js>`, and `<json>`;
- a shared state pipeline for plain data;
- syntax checking before execution;
- native Block control flow for small tasks that need no external runtime.

Block is an orchestrator with a native language core. It does not replace the standard libraries or package ecosystems of Python, JavaScript, or other runtimes.

## Quick start

1. Download the edition that matches your use case from the repository releases.
2. Install the host runtimes required by your script.
3. Save a file as `hello.blk`:

```block
<py>
message = "Hello from Python"
</py>

<js>
console.log(message)
</js>
```

4. Check and run it:

```powershell
block check .\hello.blk
block run .\hello.blk
```

## Documentation

- [Installation](Installation)
- [Syntax](Syntax)
- [Polyglot Workflows](Polyglot-Workflows)
- [Architecture](Architecture)
- [Security](Security)
- [FAQ](FAQ)

Source, examples, release files, and issue tracking are available in the [GitHub repository](https://github.com/O-O1112/Block_lang).

## Project character

Block is developed through human direction and AI-assisted implementation. Changes are reviewed as source code, tested where possible, and documented with their limitations.
