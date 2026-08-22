# Block examples

These examples are intentionally small and copy-ready. They are useful for evaluating Block, sharing the project with a teammate, or checking that the local runtimes are available.

## Run one in three steps

1. Install [Block Engine](../downloads.html) and the runtimes used by the example.
2. Open a PowerShell prompt in the repository root.
3. Run the matching command:

```powershell
block .\examples\hello-polyglot.blk
block .\examples\local-data-pipeline.blk
block .\examples\native-control-flow.blk
```

The first two examples use Python and Node.js. `native-control-flow.blk` uses the built-in Block syntax and does not need another runtime.

## Examples at a glance

| File | What it demonstrates | Edition |
| --- | --- | --- |
| [`hello-polyglot.blk`](hello-polyglot.blk) | Python creates state, JavaScript consumes it, and HTML presents the result | Standard |
| [`local-data-pipeline.blk`](local-data-pipeline.blk) | A small local data workflow with Python preparation and JavaScript reporting | Standard |
| [`native-control-flow.blk`](native-control-flow.blk) | Variables, conditions, and output using Block-native syntax | Standard |

Block delegates each language block to the corresponding local runtime. Install Python or Node.js separately when an example needs it, and never run scripts you do not trust.
