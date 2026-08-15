- Enables network guard to prevent scripts from establishing network connections;

- Each execution block has a timeout limit;

- There are limits on the number of requests, parallel executions, input size, and output size;

- `.pfx` files, passwords, or private keys will not be placed in Block projects.

Security settings cannot replace code review. Do not execute `.blk`, `.blkl`, or `.blkp` files from unknown sources, as language blocks may call the local Python, Node.js, or other external runtimes.

---

## Common Errors

### 1. Inconsistent End Tags

Error:

```block

<py>

print("hello")

</js>

```

Correct:

```block

<py>

print("hello")

</py>

```

### 2. No Corresponding Runtime on Local Machine

Block is only responsible for coordination and will not automatically generate all language runtimes. If using `<py>`, please ensure Python can be executed in the terminal; if using `<js>`, please ensure Node.js is available.

### 3. Shared Values ​​Are Not Serializable

Pass strings, numbers, booleans, arrays, and objects between blocks. Do not pass open files, connections, or functions directly.

### 4. Place Plain Text Outside Language Blocks

Executable code should be enclosed in tags such as `<py>...</py>`, `<js>...</js>`, etc. Text placed outside these tags alone will not be executed as Python or JavaScript.

---

## Block+ Development Tools

Plus provides additional scripting tools:

```powershell
block-plus fmt main.blkp
block-plus check main.blkp
block-plus doc main.blkp

```

Purpose:

- `fmt`: Organizes the Block format, keeping a `.bak` backup before overwriting;

- `check`: Parses the script and lists the found blocks;

- `doc`: Generates a block summary file for the script.

````` ---

## A Complete Example

```block

<import src="modules/config.blk" />

<py>
numbers = [1, 2, 3, 4, 5]

total = sum(numbers)

average = total / len(numbers)

</py>

<js>
const label = `total=${total}, average=${average}`;

console.log(label);

</js>

<html>

<!doctype html>

<html lang="en">

<body>

<main>

<h1>Block result</h1>

<p>Total: {{total}}</p>

<p>Average: {{average}}</p>

</main>

</body>

</html>

</html>

```

This is Block The core principle: Each piece of code uses its most familiar language, while the entire process still has only one entry point, one script, and a clear data flow.

---
## A word to first-time users

**Leave the language to the language, and the flow to the Block.**

You don't need to abandon your familiar tools to connect different languages; simply put the code into the correct Block and let the data flow naturally to the next section.
