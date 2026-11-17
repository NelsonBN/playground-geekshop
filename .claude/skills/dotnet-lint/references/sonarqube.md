# SonarQube feedback (optional)

Use this only when the SonarQube MCP server is available, that is, when tools named `mcp__sonarqube__*` exist in the session (load them with `ToolSearch` if they are deferred). If they are absent, skip the whole step silently: never fail, install or configure anything because of it.

SonarQube analyses the code **already pushed** (a branch or a pull request), so it can lag behind the working tree. Treat it as extra feedback that complements the local scan, never as a replacement and never as a gate for the stop hook.

## 1. Find the project

1. `search_my_sonarqube_projects`. Pick the project whose key or name matches the repository (the `origin` remote name, or `sonar.projectKey` in `sonar-project.properties`, for example `Owner_repo`).
2. No match or several candidates: skip and add "SonarQube project not identified" to the report's "Decisions needed". Do not guess.

## 2. Choose the analysis context

- Current branch has an open pull request: `list_pull_requests`, then use its key as `pullRequest`.
- Otherwise use `branch` with the git branch name (check it with `list_branches`; if the branch was never analysed, omit both to read the default branch and say so in the report).
- Never pass both, and never pass a git branch name as `pullRequest`.

## 3. Collect

Run in parallel:

- `get_project_quality_gate_status` (failed conditions explain what blocks the merge).
- `search_sonar_issues_in_projects` with `projectKeys`, `issueStatuses: ["OPEN", "CONFIRMED"]` and the context from step 2. For the `changed` scope, add `inNewCodePeriod: true` (valid only with exactly one entry in `projectKeys`).
- `search_security_hotspots` for the same context (status `TO_REVIEW`).

Read `show_rule` for any rule you do not already know before changing code.

## 4. Triage

Keep only findings in files that exist locally and that are in scope:

- `changed` scope: files in the diff against the default branch. Check the line in the working tree: if the code no longer matches the message, the issue is stale (already fixed locally); count it as stale, do not "fix" it.
- Other scopes: every open finding of the matching severity, code or category.

Map Sonar severities to the local order: `BLOCKER`, `HIGH` as error-level, `MEDIUM` as warning-level, `LOW`, `INFO` as info-level. Skip a finding that duplicates one already in `findings.tsv` (same file, line and equivalent rule).

## 5. Fix

Same rules as the local findings: fix the cause, keep to the scope, follow the tests-first step for behaviour changes, and respect the **NEVER** list (no suppressions, no `// NOSONAR`, no `#pragma`). Do not change issue status (`change_sonar_issue_status`, `change_security_hotspot_status`) unless the user asks: marking something false positive or accepted is a human decision.

## 6. Report

Pass what cannot be resolved locally through `--decisions`, one item per line, for example:

- `Sonar quality gate: ERROR (new_coverage 62% < 80%)`
- `Sonar S6966 Program.cs:16 left open: <reason>`
- `Sonar hotspot S5122 needs human review`
- `Sonar data is stale: branch has unpushed changes`

When Sonar was consulted, state in the final outcome line: project key, context (branch or PR), quality gate status, and counts of fixed, stale and remaining Sonar findings. Fixed ones only disappear from Sonar after the next push and analysis; say so.
