# Polyglot state

Block runs language stages as separate processes in source order. A completed
stage can return serializable values to the engine, and later stages receive a
prepared copy of that state.

## Transfer model

```text
source state -> validate -> serialize -> launch next runtime -> inject state
```

Prefer strings, numbers, booleans, arrays, and plain objects. Open files,
sockets, database connections, callbacks, and other live handles cannot be
transferred between native processes.

## Practical example

```block
<py>
total = 15
label = "prepared"
</py>

<js>
console.log(`${label}: ${total}`)
</js>
```

The JavaScript stage receives the completed values rather than sharing Python's
memory. This boundary makes the workflow predictable, but each runtime still
has its own syntax, libraries, error behavior, and security model.

## Safety rules

- Validate data before using it in a shell command, path, query, or template.
- Keep transferred state small and intentionally structured.
- Do not put secrets in source files or serialized state unless the workflow has
  a deliberate secret-management design.
- Treat every runtime boundary as a trust boundary when input is untrusted.
