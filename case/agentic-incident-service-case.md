
# Agentic Engineering using GitHub Copilot Hackathon

This guide is designed for a customer hackathon where teams build a small **incident-to-fix** solution in **.NET Aspire (.NET 10) + React** and evolve it in four layers during the day.

---

## Outcomes (what “good” looks like by end of day)

Teams should be able to demo:
1. **Running full-stack app under Aspire** (AppHost starts API + React) and showing incidents in UI. 
2. A **spec + instructions** that act as the team’s durable “agent onboarding” (repo guidance) and demonstrate scope/constraints. 
3. A **PR-based workflow** that uses **Copilot code review** as an accelerator, not a replacement for human review. 
4. **Unit + integration tests** in CI as a gate (tests must pass before merge). 
5. A **security checklist** + at least one automated security guardrail (policy/workflow placeholder is fine if GHAS isn’t enabled). 

---

## Agenda 

### Sprint 0 — Baseline 
Goal: run AppHost, load incidents, display in UI.

### Topic 1 — Reliable agent-driven development: primitives + specs + gates
Sprint 1: add primitives + first spec + PR workflow.

### Topic 2 — Orchestrator efficiency: roles, skills, and Copilot code review
Sprint 2: introduce agent roles + reusable skills + Copilot review.

### Topic 3 — Testing as an evaluation primitive
Sprint 3: expand tests + CI enforcement.

### Topic 4 — Security and resilience
Sprint 4: secret hygiene + security checks.

### Demo
Selected teams shows 5-8 minutes demo.

---

## Setup

### Setup checklist (10 min)
Teams should:
- Clone the starter repo.
- Confirm they can start AppHost and see both services.
- Confirm they can create a branch and open a PR.

### Copilot prompts (setup)
Use in VS Code Copilot Chat:

```text
Scan this repo and explain how to run the system locally (AppHost + API + WebApp). Point me to the exact commands and the relevant folders.
```

```text
Create a short “Getting Started” section for README that includes: prerequisites, how to run AppHost, how to run tests.
```

### Expected outputs
- AppHost launches API + WebApp. citeturn5search250turn5search264
- Team can open a PR.

---

## Sprint 0 — Baseline 

### Team tasks
1. Run the solution.
2. Make sure `GET /api/incidents` returns data.
3. UI renders incident list and can select one incident.

### Copilot prompts (implementation)

**Planner-style prompt**
```text
Read /specs and /docs if present. Propose a minimal plan to ensure the UI lists incidents and shows incident details. Keep changes small and PR-friendly.
```

**Implementer-style prompt**
```text
Implement the smallest change set to:
1) ensure /api/incidents returns id,title,severity,system,tags,observedAt
2) ensure the React UI renders the list and a details panel.
Provide the exact files to change and the code.
```

### Expected outputs
- Working baseline UI.
- First PR: “Baseline app runs and lists incidents.”

---

## Topic 1 — Reliable agent-driven development: primitives + specs + gates

### Sprint 1 — Apply layer 1

#### Team tasks
1. Add or refine `.github/copilot-instructions.md`.
2. Create/update a first spec (e.g., incident details + recommendation behavior).
3. Add a PR template checklist and use it.

#### Copilot prompts

**Create repo instructions**
```text
Create or refine .github/copilot-instructions.md for this repo.
Include: architecture summary (.NET Aspire + API + React), coding standards, test expectations, security basics, and definition of done.
Keep it concise and actionable.
```

**Write a feature spec**
```text
Create a spec under /specs/incident-triage/spec.md for “incident details + recommendation”.
Include: goal, scope/non-goals, API contracts, acceptance criteria, edge cases, and test plan.
```

**Gatekeeping prompt (human validation)**
```text
Review this spec like a tech lead. List missing constraints, unclear acceptance criteria, and tests we should require.
```

#### Expected outputs (you check)
- `.github/copilot-instructions.md` exists and is readable. citeturn1search79turn1search1
- `/specs/...` has acceptance criteria + test plan. 
- PR includes “spec first” changes.

---

## Topic 2 — Orchestrator efficiency: roles, skills, and Copilot code review

### Sprint 2 — Apply layer 2

#### Team tasks
1. Add 2–3 agent roles under `.github/agents/` (planner, implementer, reviewer, tester).
2. Add 1–2 skills under `.github/skills/` (e.g., `testing`, `code-review`).
3. Require: every PR requests Copilot review.

#### Copilot prompts

**Define/extend an agent role**
```text
Create a reviewer agent config in .github/agents/reviewer.agent.md.
It should focus on spec compliance, security issues, and missing tests.
```

**Create a skill**
```text
Create a testing skill in .github/skills/testing/SKILL.md.
Include when to use it, how to structure tests, and how to handle edge cases.
```

**Use Copilot for PR review (human-in-the-loop)**
```text
Given the current PR diff, act as a strict reviewer. Check: spec compliance, correctness, security, and missing tests.
Output: prioritized findings and recommended fixes.
```

#### Expected outputs
- `.github/agents/*` exists. citeturn1search79turn1search1
- `.github/skills/*/SKILL.md` exists. citeturn1search78turn1search79
- PR shows Copilot review feedback and changes addressed. citeturn1search85turn1search90

---

## Topic 3 — Testing as an evaluation primitive

### Sprint 3 — Apply layer 3

#### Team tasks
1. Add unit tests for triage and recommendation logic.
2. Add integration tests for the key endpoints.
3. Make CI fail if tests fail.

#### Copilot prompts

**Generate unit tests**
```text
Write xUnit unit tests for the triage logic.
Cover: severity mapping, keyword escalation, and at least one edge case.
Keep tests deterministic.
```

**Generate integration tests**
```text
Add integration tests using WebApplicationFactory for:
- GET /health
- GET /api/incidents
- POST /api/incidents (validate Created)
Include assertions that are stable and not time-dependent.
```

**When tests fail**
```text
Here is the failing test output (paste). Explain the root cause, propose the smallest fix, and update tests if needed.
```

#### Expected outputs
- Tests exist and pass locally.
- CI enforces tests. citeturn5file278turn1search1

---

## Topic 4 — Security and resilience

### Sprint 4 — Apply layer 4

#### Team tasks
1. Add `.env.example` and confirm no secrets are committed.
2. Add PR checklist items for security.
3. Add at least one automated security check (workflow placeholder is OK).

#### Copilot prompts

**Threat modeling (lightweight)**
```text
Threat-model this repo at a high level.
List the top 5 likely risks (secrets, injection, logging, deps, auth) and one mitigation per risk.
Keep it practical for a hackathon.
```

**Secure-by-default review**
```text
Review the API endpoints for input validation and safe error handling.
Suggest concrete changes to reduce risk (no sensitive logs, validate request payloads).
```

#### Expected outputs
- `.env.example` present.
- PR template includes security checks.
- Security workflow exists or org-level controls are enabled. citeturn1search71turn1search74

---

# Quick-reference prompt pack (copy/paste)

## Planner
```text
You are the planner. Read the spec under /specs and propose a step-by-step plan.
Return: tasks, files to change, and test strategy.
```

## Implementer
```text
Implement the next task from the plan.
Constraints: follow .github/copilot-instructions.md, keep changes small, add tests.
Return: patch-level guidance and commands to run.
```

## Reviewer
```text
Review this PR against the spec and repo instructions.
Return: prioritized findings, security issues, and missing tests.
```

## Tester
```text
Write/extend tests that prove the acceptance criteria.
Return: test cases, rationale, and how to run.
```
