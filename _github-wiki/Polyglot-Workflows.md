# Polyglot Workflows

Block is most useful when each stage uses the language that already fits the task.

```block
<py>
rows = [10, 20, 30]
result = {"count": len(rows), "total": sum(rows)}
</py>

<js>
console.log(`count=${result.count}, total=${result.total}`)
</js>

<json>
{
  "result": {{result}}
}
</json>
```

## Design rules

1. Keep each runtime block focused on one responsibility.
2. Pass plain data at the boundary.
3. Name the values that downstream stages need.
4. Run `block check` before executing a workflow.
5. Keep secrets and live connections inside the stage that owns them.

## When Block is valuable

Block is a good fit for local ETL, report generation, automation, teaching, and AI-assisted workflows where the complete sequence should remain visible in one file.

It is not intended to hide arbitrary shell commands or replace a mature runtime's package ecosystem.
