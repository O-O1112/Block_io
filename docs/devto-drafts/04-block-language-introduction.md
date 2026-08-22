---
title: "[Python/JS/C#] Block Language 2.2.0: Run Polyglot Workflows in One Document"
published: false
description: "I got tired of maintaining glue scripts, so I built a local-first engine for running Python, JavaScript, Lua, PHP, and more in one readable file."
tags: showdev, opensource, python, javascript
---

# [Python/JS/C#] Block Language 2.2.0: Run Polyglot Workflows in One Document

I got tired of writing small glue programs around otherwise simple workflows.

Python was the right tool for data preparation. JavaScript was the right tool for formatting and application logic. Sometimes Lua, PHP, SQLite, or a systems language was the right tool for another stage.

The annoying part was not using multiple languages.

The annoying part was keeping all the boundaries between them alive:

- temporary JSON files;
- local HTTP endpoints;
- duplicated serialization code;
- shell scripts that quietly became the real application;
- and several files that had to be opened just to understand one workflow.

Coming from a visual block-coding mindset, I kept thinking:

> Why can’t I write the native blocks in one document and let the variables flow through an explicit pipeline?

So I built **Block Language**.

## 💡 The Problem: Glue Code Becomes the Project

A small data workflow can require a surprising amount of ceremony:

1. prepare values in Python;
2. serialize them to disk;
3. start another process;
4. parse the file in JavaScript;
5. format the result somewhere else;
6. remember which file is the current source of truth.

Block keeps the runtime boundary visible while making the execution plan a single readable file.

## ⚡ How Block Works

A Block file is a sequence of native language blocks. The C# orchestrator parses the file, runs each stage through its local runtime, and prepares compatible state for the next stage.

```block
<py>
name = "Block"
values = [2, 4, 6, 8]
total = sum(values)
</py>

<js>
const average = total / values.length;
console.log(`[Node.js] ${name}: total=${total}, average=${average}`);
</js>

<html>
<h1>{{name}} result</h1>
<p>Total: {{total}}</p>
<p>Average: {{average}}</p>
</html>
```

Python still runs through Python. JavaScript still runs through Node.js. Block coordinates the order and the state transfer; it does not try to emulate either language.

## 💻 Running the Example

Save the file as `demo.blk` and run it with the Standard engine:

```powershell
block demo.blk
```

The corresponding host runtimes must be installed locally. Block is an orchestrator, not a bundle containing every language runtime.

The console output is intentionally simple:

```text
[Node.js] Block: total=20, average=5
[HTML] Output written to -> ...\block_output.html
```

The same model works with Lite (`.blkl`) and Plus (`.blkp`) when the project needs a smaller or broader runtime surface.

## 🔗 Native Syntax Is Also Available

Not every workflow needs a second runtime. Standard Block files can use native variables and control flow directly:

```block
score = 80
if score >= 60:
    result = "pass"
else:
    result = "retry"
block
print(result)
```

```text
pass
```

This makes Block useful for small scripts as well as polyglot pipelines.

## 🚀 Key Features in 2.2.0

- **One readable entry point:** keep the execution plan in one `.blkl`, `.blk`, or `.blkp` file.
- **Shared state pipeline:** serializable values can move from one runtime stage to the next.
- **Progressive editions:** start with Lite, use Standard for everyday workflows, or choose Block+ for expanded runtime support.
- **Native ecosystem access:** use the Python packages, Node modules, PHP libraries, and other tools already installed on the host.
- **Visible boundaries:** runtime startup, state transfer, and errors remain inspectable.
- **MIT licensed:** the engine, website, and editor integrations are available in the repository.

## 🔒 A Security Note

Block invokes host runtimes. That is the point, and it is also the reason you should never execute an untrusted `.blk`, `.blkl`, or `.blkp` file.

The engine includes import limits, path checks, runtime policy controls, and validation for malformed blocks, but those controls do not turn arbitrary code into safe code. Review the script and its dependencies before running it.

## 🔗 Links & Resources

- **GitHub Repository:** https://github.com/O-O1112/Block_lang
- **Documentation:** https://block-io.blockengine.workers.dev/wiki.html
- **Downloads:** https://block-io.blockengine.workers.dev/downloads.html
- **Examples:** https://github.com/O-O1112/Block_lang/tree/main/examples

I would love to hear what you would build with a visible Python-to-JavaScript or Python-to-Lua state pipeline. Which runtime should Block support next?
