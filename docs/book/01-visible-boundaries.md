# 1. Visible boundaries

## The pipeline is more important than the model

An AI workflow usually has at least three responsibilities:

1. prepare and normalize input;
2. run inference or another computation;
3. present a result to a person or the next program.

The same shape appears in ordinary polyglot programs. One stage reads a file, another
calculates a value, and a third stage renders a report. The languages may change, but
the boundary problem stays the same.

When state is implicit, debugging becomes a guessing game. A later stage might be
reading an old temporary file, relying on an environment variable, or receiving a
value whose shape was never documented. When state is explicit, the boundary becomes
an interface that can be inspected and tested.

## Treat state as a contract

A useful state object says what crosses the boundary and why. For example:

```json
{
  "request_id": "demo-001",
  "text": "Block coordinates multiple native runtimes in one file.",
  "language": "en",
  "features": {
    "characters": 60,
    "words": 9
  },
  "model_input": "Classify this developer documentation sentence."
}
```

The exact fields will change from project to project. The discipline does not:

- name the stage that produced the value;
- keep the shape small enough to inspect;
- validate required fields at the receiving boundary;
- avoid putting secrets and non-serializable resources into shared state.

An open file handle, socket, database connection, function, or circular object is not a
portable value. It belongs inside the stage that owns it. Pass a path, identifier, or
small data record instead, when that is appropriate.

## A deterministic preparation stage

The first stage should be runnable without an AI provider. That makes it possible to
test the input contract before introducing model variability.

```block
<py>
import re

text = "Block coordinates multiple native runtimes in one file."
words = re.findall(r"[A-Za-z0-9_'-]+", text)
request_id = "demo-001"
features = {
    "characters": len(text),
    "words": len(words),
}
model_input = "Classify this developer documentation sentence: " + text
</py>
```

This stage performs ordinary, deterministic work. It does not need to know whether a
later stage uses a local model, a hosted API, or no model at all.

## Consume the same state in JavaScript

The next stage can use the values prepared by Python while keeping its own native
syntax:

```block
<js>
if (!request_id || !text || !features) {
  throw new Error("Invalid pipeline state");
}

const label = /runtime|python|javascript|code/i.test(text)
  ? "technical"
  : "general";

const result = {
  request_id,
  label,
  confidence: label === "technical" ? 0.98 : 0.61,
  words_seen: features.words,
};

console.log(JSON.stringify(result));
</js>
```

The classifier is intentionally simple. Its purpose is to make the transport and
validation visible before an external model is added. A model can later replace the
classifier behind the same input and output contract.

Expected output:

```text
{"request_id":"demo-001","label":"technical","confidence":0.98,"words_seen":9}
```

If this output changes, the investigation has a starting point. Did the Python stage
produce different text? Did the state shape change? Did the JavaScript stage receive a
missing field? A visible boundary narrows the search.

## Why the contract should stay narrow

Shared state is not a reason to copy every variable between runtimes. API keys, access
tokens, cookies, and private source documents should stay in the narrowest possible
scope. They should not appear in a general-purpose state object that can be printed,
serialized, or passed to another process.

The same principle applies to AI prompts. Send the model the fields it needs, and keep a
safe request ID so that retries can reuse the same prepared input without repeating the
whole workflow. A small contract is easier to redact, log, validate, and version.

## Test the boring path first

Before tuning a model, test the boundary with known data:

```python
def test_state_contract():
    state = build_state(
        "test-001",
        "Python sends structured state to JavaScript.",
    )

    assert state["request_id"] == "test-001"
    assert state["features"]["words"] == 7
    assert "Classify" in state["model_input"]
```

The receiving stage should reject missing fields instead of silently inventing
defaults. When a result changes, the test helps separate input changes from runtime or
model changes.

## The Block view

Block applies this boundary discipline to a single readable document. Each language
block remains native, while the engine makes stage order and serializable state visible.
The result is not magic shared memory; it is a sequence of explicit handoffs.

That makes Block useful for more than AI. It is a practical way to keep a data
preparation step, a computation step, and a presentation step together without making
one language pretend to be all the others.

In the next chapter, we will turn this mental model into a first runnable Block file and
separate installation problems from program problems.
