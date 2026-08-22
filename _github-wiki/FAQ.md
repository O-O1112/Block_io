# FAQ

## Is Block a programming language?

Block is both a workflow language and an execution engine. Its native core supports variables, expressions, control flow, collections, functions, and built-ins. Its defining feature is coordinating existing runtimes in one explicit document.

## Does Block replace Python or JavaScript?

No. Block lets each stage keep using the runtime that fits it best.

## Do I need to install every runtime?

No. Install only the runtimes used by your files. Native Block programs do not need Python or Node.js.

## Why did my script fail to run?

Start with:

```powershell
block check .\your-file.blk
block runtimes
block doctor
```

Then verify matching tags, file paths, edition support, configuration flags, and host runtime availability.

## Can Block run untrusted code safely?

No. Block invokes host runtimes and should be treated like a code launcher. Use source review, antivirus scanning, least privilege, and isolated test environments.

## How can I contribute?

Start with a reproducible example, documentation improvement, test case, or issue report. Read the repository's contribution and security policies before opening a change.
