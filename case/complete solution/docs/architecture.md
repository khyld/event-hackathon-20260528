# Architecture

## Components
- **AppHost** (`src/AppHost`): orchestrates local development with Aspire
- **ApiService** (`src/ApiService`): ASP.NET Core Web API
- **WebApp** (`src/WebApp`): React + Vite

## Flow
1. WebApp calls ApiService endpoints.
2. ApiService loads synthetic incidents from `data/incidents.json` (seed) and stores additions in-memory.
3. ApiService runs deterministic triage + recommendation logic.

## Extension points
- Replace deterministic logic with agentic pipelines (planner/implementer/reviewer/tester roles).
- Add persisted storage (e.g., Postgres) and wire via Aspire.
