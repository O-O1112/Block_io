# Syntax

## Runtime blocks

Use explicit tags around native-language code:

```block
<py>
message = "hello"
print(message)
</py>

<js>
console.log("received from the previous stage")
</js>
```

Common tags include `<py>`, `<js>`, `<html>`, `<php>`, `<lua>`, and `<sqlite>`.
Block+ adds the expanded runtime set described in the release documentation;
the selected edition and installed host tools determine what can actually run.

## Native control flow

The native Block layer uses an explicit `block` terminator:

```block
score = 80

if score >= 60:
    result = "pass"
else:
    result = "retry"
block

print(result)
```

The native layer supports assignments, expressions, `if`/`elif`/`else`,
`while`, `for`, `range`, `func`, and `return`. Indentation is encouraged for
readability, but the closing `block` keyword defines scope.

## File extensions

The command-line editions use `.blkl`, `.blk`, and `.blkp` for Lite, Standard,
and Plus respectively. The editor integrations also recognize the aliases
`.block`, `.blocklite`, and `.blockplus`.

Keep opening and closing tags balanced, and use values that can be serialized
when data must cross from one runtime stage to another.
