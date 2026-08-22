# Block Language in Practice

## One File, Every Runtime

This directory contains the source manuscript for the first Block Language field guide.

The book is intentionally maintained as plain Markdown so that it can be reviewed in
GitHub, updated alongside the engine, and converted to PDF or EPUB after the content
has been tested by real readers.

## Edition 0.1

- **Working title:** *Block Language in Practice: One File, Every Runtime*
- **Audience:** developers who already use more than one language and want a visible,
  local-first way to compose those runtimes
- **Primary language:** English, with a Traditional Chinese edition planned after the
  English structure stabilizes
- **Status:** outline and opening chapter in progress
- **Source of truth:** this directory, not a generated PDF

The first edition should be a practical field guide rather than a feature catalogue.
Every chapter should answer three questions:

1. What problem does this pattern solve?
2. What is the smallest runnable example?
3. How can a reader inspect or test the result when it fails?

## Contents

Read the [book outline](OUTLINE.md) for the complete chapter plan.

| File | Purpose |
| --- | --- |
| `00-preface.md` | Why this book exists and what Block does not promise |
| `01-visible-boundaries.md` | The core idea: keep runtime and state boundaries inspectable |
| `OUTLINE.md` | Chapter goals, examples, and completion criteria |
| `README.md` | Book metadata and publishing notes |

The three AI essays and the Block introduction remain available in
[`docs/devto-drafts/`](../devto-drafts/). They are source material for the book, but
the book will reorganize and expand them instead of copying posts as-is.

## Publication plan

The planned order is:

1. review the Markdown manuscript in GitHub;
2. test every command and example on a clean machine;
3. add diagrams, troubleshooting notes, and edition-specific differences;
4. generate a free PDF and EPUB;
5. publish the downloads with a versioned release and checksum;
6. collect reader corrections before calling the book 1.0.

The book is a first-party guide. It can improve onboarding and make the project easier
to understand, but it is not a substitute for independent third-party validation.
External reports should continue to use
[`docs/THIRD-PARTY-VALIDATION.md`](../THIRD-PARTY-VALIDATION.md).

## Licensing note

The repository currently uses the MIT license. This manuscript stays under that
repository policy until a separate prose-license decision is made. Do not describe the
book as CC BY-SA or place a different license on the prose without updating the
repository notice and release metadata together.
