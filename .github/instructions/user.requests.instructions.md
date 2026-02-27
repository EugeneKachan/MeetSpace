---
description: Instructions for maintaining USER_REQUESTS.md
applyTo: 'documentation/USER_REQUESTS.md'
---

# User Requests Log Instructions

This document provides guidance for updating `USER_REQUESTS.md`, which records every request the user makes to the AI assistant during development.

## When to Update

- After each new user request is received.
- When a request is clarified or modified by the user.
- When the outcome of a request changes (e.g., task completed or abandoned).

## How to Update

1. Open `documentation/USER_REQUESTS.md`.
2. Add a new section at the top with the current date.
3. Include:
   - A concise description of the user request.
   - Any follow-up questions and answers.
   - The outcome or action taken.
   - List of files created, modified, or deleted due to the request.
4. Keep entries in reverse chronological order (newest first).

## Structure Example

```markdown
### Request X: [Short description]
**Time**: [Timestamp]
**Request**: "..."

**Outcome**:
- ...

**Files Affected**:
- path/to/file1
- path/to/file2
```

## Purpose
Maintaining this log ensures traceability of user instructions, aids in auditing decisions, and helps new contributors understand project history.

---
