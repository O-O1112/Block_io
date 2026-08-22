# Syntax

## Runtime blocks

Runtime blocks use matching opening and closing tags:

```block
<py>
numbers = [1, 2, 3]
</py>

<js>
console.log(numbers.length)
</js>
```

Common tags include `<py>`, `<js>`, `<php>`, `<lua>`, `<ps>`, `<sql>`, `<html>`, and `<json>`. Availability depends on the edition, configuration, and installed host runtime.

## Native Block language

The native layer does not require another language runtime. Compound statements end with a standalone `block` line:

```block
total = 0
for number in [1, 2, 3, 4]:
    if number == 2:
        continue
    block
    total = total + number
block
print(total)
```

The current core includes variables, arithmetic, comparisons, booleans, strings, `if` / `elif` / `else`, `while`, `for`, `break`, `continue`, `pass`, `return`, functions, lists, maps, indexing, `.length`, `.count`, and built-ins such as `range`, `len`, `str`, `int`, `float`, `bool`, `type`, `contains`, `keys`, `values`, and `sum`.

Function parameters and assignments are local. Functions can read shared state, while changes to shared collections should be made explicitly through indexed mutation.

```block
func greet(name):
    local_message = "Hello " + name
    return local_message
block

print(greet("Block"))
```

## State rules

Values crossing runtime boundaries should be plain serializable data: numbers, strings, booleans, lists, maps, and null. Do not pass open files, sockets, live database handles, functions, or circular objects between blocks.
