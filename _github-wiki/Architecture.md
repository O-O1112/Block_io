# Architecture

Block has four main layers:

1. **Parser** — identifies tagged runtime blocks, imports, output blocks, and native Block sections.
2. **Execution state** — stores plain serializable values and validates state boundaries.
3. **Runtime executor** — launches the selected local host runtime with timeouts, output limits, and configuration checks.
4. **Native interpreter** — executes the built-in Block language core without an external runtime.

## Execution flow

```text
Block file → parser and syntax checks → ordered blocks → native interpreter or host runtime → validated state → next block/output
```

Each runtime is a separate process boundary. Block coordinates the workflow; Python, Node.js, PHP, Lua, and other runtimes retain their own semantics and libraries.

Use native Block syntax for deterministic variables, control flow, collections, and small functions. Use a tagged runtime block when the workflow needs a host language's standard library, packages, file APIs, network APIs, or advanced syntax.
