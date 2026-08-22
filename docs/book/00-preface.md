# Preface

## Why glue code became the project

Most software does not begin as a large system. It begins as a useful piece of code.

Python is a good fit for preparing data. JavaScript is a good fit for a web-facing
result. SQL is often the clearest way to ask a database a question. Lua, PHP, Ruby,
PowerShell, C#, or a systems language may be the right choice for another part of the
same workflow.

Using more than one language is not the problem. The problem is what grows around the
boundaries:

- temporary files that quietly become an API;
- shell commands that contain the real execution order;
- duplicated serialization and parsing code;
- environment variables used as an undocumented state bus;
- several entry points that must be opened before the workflow can be understood.

After a while, the glue is no longer a convenience. It is the project.

Block Language started from a simple question: can the execution plan stay in one
readable document while each language remains native?

The answer is a local-first polyglot engine. A Block file contains explicit language
blocks. The engine parses those blocks, invokes the corresponding host runtime, and
passes compatible serializable state to the next stage. Python remains Python.
JavaScript remains JavaScript. Block coordinates the order and the boundary; it does
not emulate either language.

That distinction is the most important idea in this book.

## What this book will teach

This is a field guide for building small, inspectable workflows. It starts with a
minimal file and then expands carefully:

1. run one native block;
2. pass simple state between two runtimes;
3. render an output document;
4. add tests and failure evidence;
5. apply the same boundary discipline to AI workflows;
6. choose an edition and package a reproducible release.

The examples are intentionally modest. A small program whose inputs, stages, and
outputs can be inspected is more useful for learning than a spectacular demo that
cannot explain its own failures.

## What Block does not promise

Block is an orchestrator, not a universal runtime bundle. The corresponding host
language runtimes must be installed locally, and the available languages differ by
edition and environment.

Block also does not make arbitrary code trustworthy. A `.blkl`, `.blk`, or `.blkp` file
can invoke host-language features and dependencies. Read a script before running it,
review its imports, and treat downloaded programs with the same caution as shell
scripts or executables.

Finally, a first-party guide is not independent proof. The examples and claims in this
book should be backed by reproducible commands. External users are still encouraged to
submit independent results through the repository's validation process.

## How to use the book

If you are new to Block, read Chapters 1–3 in order and run every example. If you are
evaluating the engine for an existing project, start with Chapter 5 and then use
Chapter 7 to build a minimal reproduction for your workflow. If you are integrating an
AI stage, read Chapter 4 before adding a model provider.

The source of this book is Markdown in the repository. That is deliberate. It keeps
the prose reviewable beside the code and lets corrections land before a PDF or EPUB is
generated. Generated editions should always point back to a tagged source revision.
