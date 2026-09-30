## Agent skills

### Issue tracker

GitHub Issues, driven by the `gh` CLI. A spec is a parent issue; its tickets are its sub-issues. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context: `CONTEXT.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.

### Steering

The rules in `docs/agents/rules/`, the review files in `docs/agents/`, and the Suite in `docs/agents/suite.json`. The team owns them.

### Rules

The rules load into every session through these imports. Each one lives in `docs/agents/rules/`.

@docs/agents/rules/comments.md
@docs/agents/rules/determinism.md
@docs/agents/rules/file-placement.md
@docs/agents/rules/testing.md
@docs/agents/rules/words.md
