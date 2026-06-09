# Agentic Incident Service — Golden Solution (Hackathon Reference)

This repository is a **golden/reference implementation** for the hackathon case:
- .NET Aspire AppHost (Aspire 13.x)
- ASP.NET Core Web API backend
- React + Vite frontend
- Unit + Integration tests
- CI builds both .NET and the frontend

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js (18+)
- Aspire CLI installed (recommended)

### Start with Aspire
From the repo root:

```bash
cd case/agentic-incident-service
aspire run --project src/AppHost
```

If you prefer not to use Aspire CLI, you can also run:

```bash
cd case/agentic-incident-service
dotnet run --project src/AppHost
```

Alternatively, run the API and frontend separately:

```bash
# API
dotnet run --project src/ApiService

# Frontend
cd src/WebApp
npm install
npm run dev
```

### Run tests
Using the reference solution test script:

```bash
cd "case/complete solution"
./scripts/run-tests.sh
```

Or run directly:

```bash
cd case/agentic-incident-service
dotnet test src/AgenticIncidentService.slnx
```

You can also run the integration tests directly:

```bash
cd case/agentic-incident-service
dotnet test tests/IntegrationTests
```

## API endpoints

**Implemented:**
- `GET /health`
- `GET /api/incidents`

**Implemented:**
- `GET /api/incidents/{id}` — see [spec](specs/incident-triage/spec.md)
- `GET /api/incidents/{id}/recommendation` — see [spec](specs/incident-triage/spec.md)

## Hackathon alignment
- Repo instructions: `.github/copilot-instructions.md`
- Agent roles: `.github/agents/*`
- Skills: `.github/skills/*`
- Specs: `/specs/*`

> Tip: Use PRs for every change and request Copilot code review.
