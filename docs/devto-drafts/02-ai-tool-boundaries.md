---
title: "An AI Agent Is Only as Safe as Its Tool Boundary"
published: false
description: "A small Python tool-calling pattern with allowlists, typed arguments, and an audit trail."
tags: ai, python, security, programming
---

# An AI Agent Is Only as Safe as Its Tool Boundary

When people talk about AI agents, the conversation often starts with the model:

- Which model should choose the action?
- How large should the context window be?
- Should the agent plan in one step or five?

Those questions matter, but the most important boundary is usually outside the model.

It is the tool boundary.

If a model can call a function that deletes files, runs shell commands, sends messages, or changes production data, that function is part of the security model. A prompt saying “be careful” is not an access-control system.

## 💡 The Problem: Tool Calling Hides Authority

A naive agent often looks like this:

```python
command = model.generate(user_request)
subprocess.run(command, shell=True)
```

This is convenient, but it gives a text generator authority over the host shell. The model output is now both data and executable instruction.

The safer pattern is the opposite:

1. the model requests a named tool;
2. the application checks the tool name;
3. the application validates the arguments;
4. the application executes a narrow implementation;
5. the result is recorded and returned as data.

The model can propose. The application decides.

## ⚙️ Build a Small Allowlisted Registry

Here is a deliberately small registry. It can read a project status file and count lines in a known source file. It cannot execute arbitrary commands.

```python
from pathlib import Path
from typing import Any, Callable


ROOT = Path(".").resolve()


def resolve_project_file(relative_name: str) -> Path:
    candidate = (ROOT / relative_name).resolve()
    if ROOT not in candidate.parents:
        raise ValueError("Path escapes the project root")
    if not candidate.is_file():
        raise FileNotFoundError(relative_name)
    return candidate


def read_status(arguments: dict[str, Any]) -> dict[str, Any]:
    path = resolve_project_file("STATUS.md")
    return {"path": str(path.relative_to(ROOT)), "text": path.read_text()}


def count_lines(arguments: dict[str, Any]) -> dict[str, Any]:
    path = resolve_project_file(arguments["file"])
    return {"file": arguments["file"], "lines": len(path.read_text().splitlines())}


TOOLS: dict[str, Callable[[dict[str, Any]], dict[str, Any]]] = {
    "read_status": read_status,
    "count_lines": count_lines,
}
```

The registry is intentionally boring. Boring is good when it is holding authority.

## 🔒 Validate the Model Request Before Execution

The model should return structured data, not a shell command.

```python
def run_tool_request(request: dict[str, Any]) -> dict[str, Any]:
    name = request.get("tool")
    arguments = request.get("arguments", {})

    if not isinstance(name, str) or name not in TOOLS:
        raise ValueError("Tool is not allowlisted")
    if not isinstance(arguments, dict):
        raise ValueError("Tool arguments must be an object")

    result = TOOLS[name](arguments)
    return {
        "tool": name,
        "ok": True,
        "result": result,
    }
```

For `count_lines`, I would also validate the exact argument shape before calling the function. In a larger project, a schema library can do this, but the rule stays the same: validation belongs to the application, not to the model.

## 🧾 Keep an Audit Record

Every tool call should leave behind a safe, useful record:

```python
def audit_record(request: dict[str, Any], response: dict[str, Any]) -> dict[str, Any]:
    return {
        "request_id": request.get("request_id"),
        "tool": response.get("tool"),
        "ok": response.get("ok", False),
        "argument_keys": sorted(request.get("arguments", {}).keys()),
    }
```

Do not put passwords, tokens, or full private documents into the log. The audit record should explain what happened without becoming a second data leak.

## ⚡ Example Request and Output

```json
{
  "request_id": "agent-014",
  "tool": "count_lines",
  "arguments": {"file": "src/Parser.cs"}
}
```

```json
{
  "tool": "count_lines",
  "ok": true,
  "result": {"file": "src/Parser.cs", "lines": 218}
}
```

The model never receives a raw shell. It receives a small result that can be checked, displayed, or passed to the next stage.

## 🚀 Key Takeaways

- **Treat tools as capabilities:** every tool should have a narrow purpose and a clear owner.
- **Use allowlists:** unknown names must fail closed.
- **Resolve paths safely:** normalize paths and enforce a project boundary.
- **Validate twice when needed:** schema validation protects the application; business rules protect the operation.
- **Audit the decision:** a useful log makes agent behavior explainable after the fact.

The goal is not to make an agent powerless. The goal is to make its power visible, bounded, and testable.

## 🧭 Make Permissions Explicit

As the registry grows, I like to document each tool as a capability with a small permission matrix:

| Tool | Reads files | Writes files | Network | Destructive |
| --- | --- | --- | --- | --- |
| `read_status` | `STATUS.md` only | No | No | No |
| `count_lines` | One project file | No | No | No |
| `search_docs` | `docs/` only | No | No | No |
| `apply_patch` | Selected files | Yes | No | Potentially |

The table is not a replacement for code checks. It is a fast way for a reviewer to see that a new tool has more authority than the old ones.

I would also separate read-only tools from mutating tools in the registry:

```python
READ_ONLY_TOOLS = {
    "read_status": read_status,
    "count_lines": count_lines,
}

MUTATING_TOOLS = {
    # A future write tool belongs here and needs an additional approval step.
}
```

That distinction makes it possible to run an agent in a safe inspection mode. A user can ask the agent to analyze a repository without accidentally granting it write access.

## 🧪 Test Malformed and Hostile Requests

A tool registry is incomplete until it has tests for the requests that should fail.

```python
def test_unknown_tool_is_rejected():
    try:
        run_tool_request({"tool": "run_shell", "arguments": {"cmd": "dir"}})
    except ValueError as error:
        assert "allowlisted" in str(error)
    else:
        raise AssertionError("Unknown tool was accepted")


def test_path_escape_is_rejected():
    try:
        run_tool_request({
            "tool": "count_lines",
            "arguments": {"file": "..\\secrets.txt"},
        })
    except ValueError as error:
        assert "escapes" in str(error)
    else:
        raise AssertionError("Escaping path was accepted")
```

The exact error text can change, but the behavior should not: unknown capabilities and out-of-bound paths must fail closed.

## ⏱️ Add Approval for High-Impact Actions

Some tools are safe to run automatically. Others should pause and ask for approval. Sending an email, deleting data, changing permissions, or deploying code should not be treated like reading a file.

```python
def requires_approval(tool_name: str) -> bool:
    return tool_name in {"send_message", "delete_record", "deploy"}
```

This is not about distrusting every model response. It is about recognizing that an agent can be wrong in a perfectly confident way. A human approval step is a useful boundary when the consequence is external or difficult to reverse.

How do you currently decide which tools an AI agent is allowed to call?
