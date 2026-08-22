---
title: "The Smallest Useful RAG Prototype Is the One You Can Debug"
published: false
description: "Build a tiny retrieval-augmented generation pipeline with visible scoring before adding a vector database."
tags: ai, python, beginners, programming
---

# The Smallest Useful RAG Prototype Is the One You Can Debug

Retrieval-augmented generation is often introduced with a long list of components:

- an embedding model;
- a vector database;
- a chunking framework;
- a reranker;
- a prompt template;
- and finally a chat model.

All of those tools can be useful. They can also make it difficult to answer the first practical question:

**Did the system retrieve the right context?**

Before adding a vector database, I like to build a small retrieval loop that exposes every decision. If the tiny version cannot find the right document, a more expensive stack will only hide the problem behind more infrastructure.

## 💡 The Problem: Retrieval Errors Look Like Model Errors

Suppose a user asks:

```text
How do I run the Standard Block edition?
```

If the answer is wrong, there are at least two possibilities:

1. the retriever returned the wrong context;
2. the model saw the right context but generated a poor answer.

Those failures need different fixes. The first prototype should make the difference visible.

## ⚙️ Stage 1: Normalize and Score Documents in Python

This example uses token overlap instead of embeddings. It is not a replacement for semantic retrieval. It is a transparent baseline that can be tested with a few lines of code.

```python
import json
import re
import sys


DOCUMENTS = [
    {
        "id": "install-standard",
        "text": "Install Block Standard and run a .blk file with: block example.blk",
    },
    {
        "id": "install-plus",
        "text": "Block+ runs .blkp files and adds expanded runtime and tooling support.",
    },
    {
        "id": "security",
        "text": "Do not execute untrusted Block files because language blocks call host runtimes.",
    },
]


def tokens(value: str) -> set[str]:
    return set(re.findall(r"[a-z0-9]+", value.lower()))


def retrieve(question: str, limit: int = 2) -> list[dict]:
    query = tokens(question)
    scored = []
    for document in DOCUMENTS:
        overlap = query & tokens(document["text"])
        score = len(overlap)
        scored.append({
            "id": document["id"],
            "text": document["text"],
            "score": score,
            "matched_terms": sorted(overlap),
        })
    return sorted(scored, key=lambda item: item["score"], reverse=True)[:limit]


question = " ".join(sys.argv[1:]) or "How do I run Standard?"
print(json.dumps({"question": question, "matches": retrieve(question)}))
```

This prototype gives us three things that are easy to inspect:

- the retrieved document IDs;
- the numeric score;
- the exact terms that caused the match.

## 💻 Stage 2: Prepare a Model Context in JavaScript

The next stage does not pretend that retrieval is the final answer. It turns the selected documents into a bounded context and prints the prompt that would be sent to a model.

```javascript
let raw = "";

process.stdin.setEncoding("utf8");
process.stdin.on("data", chunk => {
  raw += chunk;
});

process.stdin.on("end", () => {
  const state = JSON.parse(raw);
  const context = state.matches
    .filter(match => match.score > 0)
    .map(match => `[${match.id}] ${match.text}`)
    .join("\n");

  const prompt = [
    "Answer only from the supplied context.",
    `Question: ${state.question}`,
    "Context:",
    context || "No matching context was found.",
  ].join("\n");

  console.log(prompt);
});
```

The model call can be added after this point. More importantly, it can be tested separately from retrieval.

## ⚡ Output: Make the Retrieval Decision Visible

```text
Question: How do I run Standard?
Context:
[install-standard] Install Block Standard and run a .blk file with: block example.blk
```

If the output contains only `No matching context was found`, changing the prompt will not fix the pipeline. The retrieval stage needs better tokenization, chunking, metadata, or a semantic index.

## 🚀 Key Takeaways

- **Start with a transparent baseline:** you need to see why a document was selected.
- **Separate retrieval from generation:** each stage should have its own input, output, and test.
- **Keep context bounded:** more documents do not automatically mean better answers.
- **Measure the failure mode:** wrong retrieval and wrong generation are different bugs.
- **Upgrade one layer at a time:** add embeddings only after the baseline has taught you what it misses.

RAG becomes easier to improve when it stops being one large “AI answer” function and becomes a sequence of observable stages.

## 🧪 Evaluate Retrieval Before You Tune Prompts

Before adding embeddings, I would create a tiny evaluation set:

```python
EVALUATION = [
    {
        "question": "How do I run Standard?",
        "expected": "install-standard",
    },
    {
        "question": "Which edition supports .blkp files?",
        "expected": "install-plus",
    },
    {
        "question": "Can I execute an unknown script safely?",
        "expected": "security",
    },
]


def evaluate():
    passed = 0
    for item in EVALUATION:
        result = retrieve(item["question"], limit=1)
        selected = result[0]["id"] if result else None
        passed += selected == item["expected"]
        print({
            "question": item["question"],
            "selected": selected,
            "expected": item["expected"],
            "ok": selected == item["expected"],
        })
    print(f"retrieval accuracy: {passed}/{len(EVALUATION)}")
```

The point is not to win a benchmark. The point is to know whether a change improved the behavior you care about. If a new chunking rule lowers the score from `3/3` to `2/3`, that is more useful than a vague feeling that the answers became worse.

## 🧱 Chunking and Metadata Come Before More Infrastructure

The small baseline also shows what information is missing. If two documents have the same words but different editions, metadata can make the distinction explicit:

```python
{
    "id": "install-standard",
    "edition": "standard",
    "topic": "installation",
    "text": "Install Block Standard and run a .blk file with: block example.blk",
}
```

A retriever can then filter by edition or topic before scoring text. That is often a better first improvement than immediately adding a larger model.

The same principle applies to chunk size. A chunk that is too large contains distracting instructions. A chunk that is too small loses the relationship between a command and its explanation. There is no universal number; the evaluation questions should guide the choice.

## 🔍 Know When to Upgrade to Embeddings

Token overlap is a useful baseline, but it has predictable limits. It will miss relationships such as:

- “run the regular edition” and “execute a `.blk` file”;
- “do not trust the script” and “review code before execution”;
- “load a module” and “import a local Block file”.

When those misses matter, semantic embeddings are a sensible next step. The baseline still pays off because it gives you a reference point. You can compare the embedding retriever against the simple one instead of comparing it against intuition.

## 🔒 Keep the Generation Prompt Honest

The final prompt should tell the model what to do when retrieval fails:

```text
Answer only from the supplied context.
If the context does not contain the answer, say that no matching documentation was found.
Do not invent commands, versions, or security guarantees.
```

That instruction cannot make a model perfectly reliable, but it makes the desired failure mode explicit. A useful RAG system is not one that always answers. It is one that can say when its evidence is insufficient.

The same stage-by-stage approach is useful for polyglot workflows too. I am building [Block Language](https://github.com/O-O1112/Block_lang) around that idea: each runtime stays native while the execution and state boundaries remain visible.

What is the first retrieval failure you would want your RAG prototype to explain?
