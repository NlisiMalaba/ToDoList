# Branching & release model (trunk-based)

## Branches
- `main`: single long-lived branch, always releasable.
- `feature/*`: short-lived branches for development, created from `main`.

## Rules
- All coding happens on `feature/*`.
- Changes reach `main` **only via Pull Requests**:
  - required build + test checks must pass
  - at least N reviewers (e.g. 3) must approve
  - direct pushes to `main` are disabled
- Releases are identified by **image tags**, not by staging/testing branches:
  - example: `todolist-api:1.3.0+<short_sha>`

## Environment mapping
- **Staging**: deploys a chosen image tag from `main` to the staging stack.
- **Testing**: promotes the **same image tag** from Staging after QA approval.
- **Production**: promotes the **same image tag** from Testing after business/ops approval.

This keeps a single source of truth (`main`) while still giving you three isolated environments with approval gates.
